// Overdraw 可视化：每个 UI 片元都往颜色缓冲里“加”一个固定值（Blend One One），
// 被画了几层，像素就有多亮。不读贴图——透明像素在 GPU 上一样要跑片元着色器、一样要混合，这正是 Overdraw 的代价。
// 保留了 Stencil 和 RectMask2D 裁剪，让 Mask / RectMask2D 下的结果与实际一致。
Shader "UGUIBook/Ch20/Overdraw"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Step ("Color Per Layer", Color) = (0.0627, 0.0627, 0.0627, 1)

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
        Blend One One
        ColorMask [_ColorMask]

        Pass
        {
            Name "Overdraw"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT

            struct appdata_t
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 worldPosition : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Step;
            float4 _ClipRect;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                #ifdef UNITY_UI_CLIP_RECT
                // UnityUI.cginc 里的 UnityGet2DClipping：点在裁剪矩形内返回 1，否则 0
                clip(UnityGet2DClipping(IN.worldPosition.xy, _ClipRect) - 0.5);
                #endif
                return _Step;
            }
        ENDCG
        }
    }
}
