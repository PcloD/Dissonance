// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Unlit/ShadowLayers" {
Properties {
    _ShadowProjection ("Base (RGB)", 2D) = "white" {}
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

            sampler2D _ShadowProjection;
            sampler2D _BackgroundTexture;
            sampler2D _ShadowTexture;

            float4 _ShadowProjection_ST;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _ShadowProjection);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 shadowProjCol = tex2D(_ShadowProjection, i.texcoord);
                fixed4 bgCol = tex2D(_BackgroundTexture, i.texcoord);
                fixed4 texCol = tex2D(_ShadowTexture, i.texcoord);

                // if (i.texcoord.x > 0.3) {
                // 	return shadowProjCol + bgCol;
                // }

				
				return lerp(float4(1), float4(0), shadowProjCol.a);//(1-shadowProjCol.a * 50) * texCol + bgCol * (shadowProjCol.a * 50) + fixed4(0,0,0,1);
                //if (shadowProjCol.a < 0.01) {
                // 	return texCol;
                //	return fixed4(texCol.r,texCol.g,texCol.b,shadowProjCol.a) + texCol; // green
                //}
                // return (texCol*(1-shadowProjCol.a))+bgCol*shadowProjCol.a;
                //return bgCol;//fixed4(1,0,0,1-shadowProjCol.a); //red

                // return fixed4(shadowProjCol.r + texCol.r * bgCol.r,
                // 	          shadowProjCol.g + texCol.g * bgCol.g,
                // 	          shadowProjCol.b + texCol.b * bgCol.b,
                // 	          bgCol.a);
            }
        ENDCG
    }
}

}