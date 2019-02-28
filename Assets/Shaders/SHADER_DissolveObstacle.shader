﻿// basic and fast version
// TODO : need improvment to share the dissolve mask
Shader "Custom/SHADER_DissolveObstacle" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		[Header (Dissolve)]
		_DissolveTex ("Mask Dissolve", 2D) = "white" {}
		_Cutout ("Cutout",  Range(0,1)) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard vertex:vert fullforwardshadows
		#pragma target 3.0

		struct Input {
			float2 uv_MainTex;
			float4 screenPos;
			float viewDist;
		};

		sampler2D _MainTex;
		sampler2D _DissolveTex;

		half _Glossiness;
		half _Metallic;
		half _Cutout;

		fixed4 _Color;
		
		void vert (inout appdata_full v, out Input o)
		{
		    UNITY_INITIALIZE_OUTPUT (Input, o);
		    half3 viewDirW = WorldSpaceViewDir (v.vertex);
		    o.viewDist = length (viewDirW);
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

			half2 uv = IN.screenPos.xy / IN.screenPos.w;
			fixed m = tex2D(_DissolveTex, uv).r;

			o.Albedo = c.rgb;
			//o.Albedo = saturate (IN.viewDist - .5);
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			clip(IN.viewDist - 10);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
