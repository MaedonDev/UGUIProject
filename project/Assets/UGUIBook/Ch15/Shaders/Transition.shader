// 屏幕转场：_Progress 0 = 完全透明，1 = 完全盖住。_Mode 0 斜向擦除 / 1 圆形收缩 / 2 噪声溶解。
// 结构与第 14 章的 UI Shader 相同（保留 Stencil 与裁剪），用在一张全屏 Image 上。
// 底本：Unity 2022.3 内置 UI/Default（DefaultResourcesExtra/UI/UI-Default.shader）。
// 保留了 Stencil 属性块、UNITY_UI_CLIP_RECT、UNITY_UI_ALPHACLIP，所以能放在 Mask / RectMask2D 下面正常裁剪。
Shader "UGUIBook/Ch15/Transition"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Progress ("Progress", Range(0, 1)) = 0.5
        _Mode ("Mode (0 Wipe, 1 Iris, 2 Dissolve)", Float) = 0
        _Center ("Iris Center", Vector) = (0.5, 0.5, 0, 0)
        _Aspect ("Aspect (w/h)", Float) = 1.777
        _Soft ("Edge Softness", Range(0.001, 0.3)) = 0.06
        _NoiseTex ("Noise", 2D) = "gray" {}
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
            float _Progress, _Mode, _Aspect, _Soft;
            float4 _Center;
            sampler2D _NoiseTex;
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

                //#region transition
                half4 color = IN.color * (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd);
                float p = IN.param, s = _Soft, cover;
                float2 uv = IN.luv;                      // 全屏 Image 的局部坐标：左下 (0,0) 右上 (1,1)
                if (_Mode < 0.5)
                {
                    // 斜向擦除：x' = (u + 0.35v) / 1.35，覆盖边界从左下扫到右上
                    float x = (uv.x + uv.y * 0.35) / 1.35;
                    cover = 1 - smoothstep(p * (1 + s) - s, p * (1 + s), x);
                }
                else if (_Mode < 1.5)
                {
                    // 圆形收缩：圆外盖住，圆的半径随 progress 从“能包住全屏”缩到 0
                    float2 d = (uv - _Center.xy) * float2(_Aspect, 1);
                    float2 far = max(_Center.xy, 1 - _Center.xy) * float2(_Aspect, 1);
                    float r = (1 - p) * (length(far) + s);
                    cover = smoothstep(r - s, r, length(d));
                }
                else
                {
                    // 噪声溶解：噪声值低于阈值的地方先被盖住
                    float n = saturate((tex2D(_NoiseTex, uv * float2(_Aspect, 1) * 0.5).r - 0.2) / 0.55);
                    cover = 1 - smoothstep(p * (1 + s) - s, p * (1 + s), n);
                }
                color.a *= cover;
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
