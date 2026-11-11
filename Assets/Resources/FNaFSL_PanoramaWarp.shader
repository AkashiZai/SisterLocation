// FNaF-style "panorama" perspective warp: the image is stretched vertically toward the left/right
// edges, like the original game's pre-rendered rooms viewed through a curved lens.
Shader "Hidden/FNaFSL/PanoramaWarp"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
        _Warp ("Warp Strength", Float) = 0.32
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Warp;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float x = i.uv.x - 0.5;
                float s = 1.0 + _Warp * (x * x * 4.0);
                float2 uv = float2(i.uv.x, 0.5 + (i.uv.y - 0.5) / s);
                return fixed4(tex2D(_MainTex, uv).rgb, 1.0);
            }
            ENDCG
        }
    }
}
