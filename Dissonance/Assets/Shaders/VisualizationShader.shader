Shader "Custom/VisualizationShader" {
	Properties {
		_MainColor ("Base (RGB)", Color) = (0,0,0.5,1)
		_BumpMap ("Bumpmap", 2D) = "bump" {}
		_RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0.0)
      	_RimPower ("Rim Power", Range(0.5,8.0)) = 3.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Lambert

		struct Input {
			float2 uv_BumpMap;
			float3 viewDir;
		};

		float4 _MainColor;
		sampler2D _BumpMap;
		float4 _RimColor;
		float _RimPower;
		void surf (Input IN, inout SurfaceOutput o) {
			o.Albedo = _MainColor;
			o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));
			half rim = saturate(dot (normalize(IN.viewDir), o.Normal));
			o.Emission = _RimColor.rgb * pow (rim, _RimPower);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
