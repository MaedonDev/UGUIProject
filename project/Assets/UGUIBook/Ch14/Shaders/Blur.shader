// 模糊：两个 Pass 的可分离高斯模糊（Pass 0 横向、Pass 1 纵向），配合降采样的 RenderTexture 用 Graphics.Blit 调用。
// 它不直接画在 UI 上（结果交给 RawImage 显示），所以不需要 Stencil / 裁剪那一套。
Shader "UGUIBook/Ch14/Blur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size (texels)", Float) = 1
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    sampler2D _MainTex;
    float4 _MainTex_TexelSize;   // (1/宽, 1/高, 宽, 高)，Unity 自动填
    float _BlurSize;

    struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

    v2f vert(appdata_img v)
    {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv = v.texcoord;
        return o;
    }

    //#region gauss
    // 9 个采样点的一维高斯权重（σ≈2），两遍（横 + 竖）等价于 9×9 的二维高斯，但只要 18 次采样而不是 81 次
    static const float W[5] = { 0.2270270, 0.1945946, 0.1216216, 0.0540541, 0.0162162 };

    half4 Blur1D(float2 uv, float2 dir)
    {
        half4 c = tex2D(_MainTex, uv) * W[0];
        for (int i = 1; i < 5; i++)
        {
            float2 off = dir * _MainTex_TexelSize.xy * i * _BlurSize;
            c += tex2D(_MainTex, uv + off) * W[i];
            c += tex2D(_MainTex, uv - off) * W[i];
        }
        return c;
    }
    //#endregion

    half4 fragH(v2f i) : SV_Target { return Blur1D(i.uv, float2(1, 0)); }
    half4 fragV(v2f i) : SV_Target { return Blur1D(i.uv, float2(0, 1)); }
    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass { CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragH
            ENDCG }
        Pass { CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragV
            ENDCG }
    }
}
