// 反面教材：一个“看起来能用”的 UI Shader——能显示贴图和顶点色，但删掉了 Stencil 属性块和 UNITY_UI_CLIP_RECT。
// 放在 Mask 下面它会“漏出来”，放在 RectMask2D 下面只会被整块剔除、不会被逐像素裁剪。不要在项目里这样写。
Shader "UGUIBook/Ch14/BadNoStencil"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            fixed4 _Color;
            struct appdata_t { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; fixed4 color : COLOR; float2 uv : TEXCOORD0; };
            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color * _Color;
                o.uv = v.uv;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target { return tex2D(_MainTex, i.uv) * i.color; }
        ENDCG
        }
    }
}
