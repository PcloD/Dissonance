// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Unlit/ShadowLayers" {
Properties {
    _MainTex ("Base (RGB)", 2D) = "white" {}
    _BackgroundTexture ("Background Texture", 2D) = "white" {}
    _ShadowTexture ("Shadow Texture", 2D) = "black" {}
}

SubShader {
    Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
    LOD 100

    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha

    Pass {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                half2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _BackgroundTexture;
            sampler2D _ShadowTexture;

            float4 _MainTex_ST;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 shadowProjCol = tex2D(_MainTex, i.texcoord);
                fixed4 bgCol = tex2D(_BackgroundTexture, i.texcoord);
                fixed4 texCol = tex2D(_ShadowTexture, i.texcoord);

                fixed4 res = lerp(float4(1), float4(0), shadowProjCol.a) * bgCol;

                //return (1-shadowProjCol.a * 50) * texCol + bgCol * (shadowProjCol.a * 50) + fixed4(0,0,0,1);
                return res.aaaa * res + (float4(1.0, 1.0, 1.0, 1.0) - res.aaaa) * fixed4(texCol.r,texCol.g,texCol.b,bgCol.a);

				//return lerp(float4(1), float4(0), shadowProjCol.a);
            }
        ENDCG
    }
}

}