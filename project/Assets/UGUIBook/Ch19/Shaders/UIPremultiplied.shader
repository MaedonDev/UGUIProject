// 显示“预乘 Alpha”贴图的 UI Shader。相机渲染到透明背景的 RenderTexture 时，边缘像素的颜色已经和
// 透明黑色混合过（等于预乘过 alpha）。UI/Default 会在片元里再乘一次 alpha，边缘就会发黑、发暗。
// 这个 Shader 只在“贴图本身不再乘 alpha”这一点上和 UI/Default 不同。
Shader "UGUIBook/Ch19/UIPremultiplied"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture (Premultiplied)", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }

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

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _ClipRect;
            float4 _MainTex_ST;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            //#region frag
            fixed4 frag(v2f IN) : SV_Target
            {
                half4 tex = tex2D(_MainTex, IN.texcoord);   // rgb 已经预乘过 a
                // 顶点色（含 CanvasGroup 透明度）仍按预乘方式作用：rgb 和 a 一起乘 IN.color.a
                half4 color = half4(tex.rgb * IN.color.rgb, tex.a) * IN.color.a;
                #ifdef UNITY_UI_CLIP_RECT
                color *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif
                return color;
            }
            //#endregion
        ENDCG
        }
    }
}
