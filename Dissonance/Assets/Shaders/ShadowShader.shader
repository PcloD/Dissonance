/*Shader "Hidden/ReplaceShader" {
   SubShader {
     Tags { "RenderType"="Opaque" }
     Pass {
       Tags { "LightMode"="ForwardBase" }
       Fog { Mode Off }
       Color (0, 0, 0, 0)
     }
     Pass {
       Tags { "LightMode"="ForwardAdd" }
       Fog { Mode Off }
       Color (0, 0, 0, 0)
     }
   }
   SubShader {
     Tags { "RenderType"="Transparent" }
     Pass {
       Tags { "Queue"="Transparency"}
       Tags { "LightMode"="ForwardBase" }
       Fog { Mode Off }
       Color (0, 0, 0, 0)
     }
     Pass {
       Tags { "Queue"="Transparency" }
       Tags { "LightMode"="ForwardAdd" }
       Fog { Mode Off }
       Color (0, 0, 0, 0)
     }
   }
}*/

Shader "Custom/ReplaceShader" {

	Properties {
            _Color ("Shadow Strength", Color) = (1,1,1,1)
        }
        
    SubShader {
        Pass {
        
            CGPROGRAM

			
            #pragma vertex vert
            #pragma fragment frag

			
            float4 vert(float4 v:POSITION) : SV_POSITION {
                return mul (UNITY_MATRIX_MVP, v);
            }
				
			float4 _Color;
            fixed4 frag() : SV_Target {
                return _Color;
            }
            ENDCG
        }
    }
}
