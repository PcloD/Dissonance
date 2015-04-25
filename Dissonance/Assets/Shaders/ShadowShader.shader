Shader "Custom/ReplaceShader" {

	Properties {
            _Color ("Shadow Strength", Color) = (1,1,1,1)
        }

    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        Pass {
            Blend SrcAlpha One
            ZWrite Off


            CGPROGRAM


            #pragma vertex vert
            #pragma fragment frag


            float4 vert(float4 v:POSITION) : SV_POSITION {
                return mul (UNITY_MATRIX_MVP, v);
            }

			float4 _Color;
            fixed4 frag() : SV_Target {
                return fixed4(0,0,0,0.5);
            }
            ENDCG
        }
    }

    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Transparent"}
        Pass {
            Blend SrcAlpha One
            ZWrite Off

            CGPROGRAM


            #pragma vertex vert
            #pragma fragment frag


            float4 vert(float4 v:POSITION) : SV_POSITION {
                return mul (UNITY_MATRIX_MVP, v);
            }

      float4 _Color;
            fixed4 frag() : SV_Target {
                return fixed4(0,0,0,1);
            }
            ENDCG
        }
    }
}
