Shader "Custom/Shader_Water" {
	Properties {
		[Header (Water)]
		_ColorWater1 ("Water tint 1", Color) = (1,1,1,1)
		_ColorWater2 ("Water tint 2", Color) = (1,1,1,1)
		_SpeedTex ("Speed Texture", vector) = (0,0,0,0)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		[noscaleoffset]_BumpMap ("Normals", 2D) = "bump" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		[Header (Foam)]
		_ColorFoam ("Foam tint", Color) = (1,1,1,1)
		_FoamIntensity ("Foam Intensity", float) = 1

		[Header (Refraction)]
		_DistAmnt ("Distortion amount", float) = 1

		[Header (Detection)]
		_Threshold ("Threshold", float) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		ZWrite Off
		GrabPass { "_Refraction" }

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows addshadow
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _Refraction;
		sampler2D _CameraDepthTexture;

		half _Glossiness;
		half _Metallic;
		half _DistAmnt;
		half _Threshold;
		half _FoamIntensity;

		fixed4 _ColorWater1;
		fixed4 _ColorWater2;
		fixed4 _Color;
		fixed4 _SpeedTex;
		float4 _ColorFoam;
		float4 _Refraction_TexelSize;

		struct Input {
			float2 uv_MainTex;
			float4 screenPos;
		};

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed2 uvWater = float2 (IN.uv_MainTex.x + _Time.x * _SpeedTex.x, IN.uv_MainTex.y + _Time.x * _SpeedTex.y);
			fixed cLerp = tex2D (_MainTex, uvWater).r;
			float4 finalColor = lerp (_ColorWater1, _ColorWater2, cLerp);

			fixed3 n = UnpackNormal (tex2D (_BumpMap, uvWater));

			// Refraction 
			float2 _offset = n * _DistAmnt * _Refraction_TexelSize.xy;
		 	float2 screenUV = _offset * IN.screenPos.z + IN.screenPos.xy;
			screenUV = screenUV / IN.screenPos.w;
			float4 refraction = tex2D (_Refraction, screenUV);

			// Depth detection
			float texDepth = LinearEyeDepth (tex2Dproj (_CameraDepthTexture, UNITY_PROJ_COORD (IN.screenPos))).r;
			float inter = saturate ((abs (texDepth - IN.screenPos.w))/ _Threshold);
			float interFoam = 1 - saturate ((abs (texDepth - IN.screenPos.w))/ _FoamIntensity);

			o.Albedo = lerp (refraction.rgb, finalColor.rgb, inter);
			o.Albedo = lerp (o.Albedo, _ColorFoam, interFoam);
			o.Metallic = _Metallic;
//			o.Normal = n;
			o.Smoothness = _Glossiness;
			o.Metallic = _Metallic;
		}
		ENDCG
	}
	FallBack "VertexLit"
}
