Shader "Hidden/ReplaceShader" {
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
}