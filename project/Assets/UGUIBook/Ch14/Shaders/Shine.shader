// 流光：一条斜向高光带扫过图形。_Progress 0→1 从左扫到右；只在不透明像素上叠加。
// 底本：Unity 2022.3 内置 UI/Default（DefaultResourcesExtra/UI/UI-Default.shader）。
// 保留了 Stencil 属性块、UNITY_UI_CLIP_RECT、UNITY_UI_ALPHACLIP，所以能放在 Mask / RectMask2D 下面正常裁剪。
Shader "UGUIBook/Ch14/Shine"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Progress ("Progress", Range(0, 1)) = 0.5
        _ShineColor ("Shine Color", Color) = (1,1,1,0.8)
        _ShineWidth ("Shine Width", Range(0.01, 1)) = 0.18
        _Angle ("Angle", Range(-60, 60)) = 25
        _Softness ("Softness", Range(0.01, 1)) = 0.5
        // 1 = 主参数改从顶点 uv1.x 读取（配合 UIParamEffect，每个元素各自的值，不打断合批）
        [Toggle] _UseVertexParam ("Use Vertex Param (uv1.x)", Float) = 0

        //#region stencilprops
        // ---- 以下属性给 Mask 用：StencilMaterial.Add 会在材质副本上改写它们，名字一个字都不能变 ----
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        //#endregion
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        //#region stencil
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]
        //#endregion

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 texcoord1 : TEXCOORD1;   // uv1：UIParamEffect 写入 (参数, 0, 局部u, 局部v)
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float4 mask : TEXCOORD2;
                float param : TEXCOORD3;
                float2 luv : TEXCOORD4;         // 元素内的局部坐标 0~1（不受九宫格、图集影响）
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;
            int _UIVertexColorAlwaysGammaSpace;
            float _UseVertexParam;
            float _Progress;
            fixed4 _ShineColor;
            float _ShineWidth;
            float _Angle;
            float _Softness;
            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                float4 vPosition = UnityObjectToClipPos(v.vertex);
                OUT.worldPosition = v.vertex;
                OUT.vertex = vPosition;

                float2 pixelSize = vPosition.w;
                pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

                float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                OUT.mask = float4(v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));

                if (_UIVertexColorAlwaysGammaSpace)
                {
                    if(!IsGammaSpace())
                    {
                        v.color.rgb = UIGammaToLinear(v.color.rgb);
                    }
                }

                OUT.color = v.color * _Color;
                // 主参数：材质上的值，或顶点 uv1.x（lerp 代替 if，不产生额外变体）
                OUT.param = lerp(_Progress, v.texcoord1.x, _UseVertexParam);
                OUT.luv = lerp(v.texcoord.xy, v.texcoord1.zw, _UseVertexParam);
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                const half alphaPrecision = half(0xff);
                const half invAlphaPrecision = half(1.0/alphaPrecision);
                IN.color.a = round(IN.color.a * alphaPrecision)*invAlphaPrecision;

                //#region shine
                half4 color = IN.color * (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd);
                // 用局部坐标 luv（而不是贴图 uv）：九宫格、图集里的精灵 uv 不是均匀的 0~1，高光带会变形
                // 沿倾斜方向投影：x' = u + v·tan(angle)，范围是 [min(0,k), 1+max(0,k)]，
                // 高光带中心从范围左端外侧走到右端外侧，保证 progress 为 0 和 1 时都看不到高光
                float k = tan(radians(_Angle));
                float x = IN.luv.x + IN.luv.y * k;
                float center = lerp(min(0, k) - _ShineWidth, 1 + max(0, k) + _ShineWidth, IN.param);
                float d = abs(x - center) / _ShineWidth;               // 0 = 带中心，1 = 带边缘
                float band = 1 - smoothstep(1 - _Softness, 1, d);
                color.rgb += _ShineColor.rgb * band * _ShineColor.a;  // 加亮；alpha 不变，透明处仍透明
                //#endregion

                #ifdef UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
                color.a *= m.x * m.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                color.rgb *= color.a;
                return color;
            }
        ENDCG
        }
    }
}
