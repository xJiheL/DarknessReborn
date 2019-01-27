Shader "DarknessReborn/EnvironmentVertexPaint_3+1" {
	Properties {
//		_Cube ("Reflection Cubemap", Cube) = "_Skybox" { }
		_Metallic ("Metallic", Range (0, 1)) = 0.5

		[Header (Textures Parameters)]
		[Header (R Channel)]
		_MainColor ("Main Tint", Color) = (1,1,1,1)
		_MainTex ("Main Albedo (RGB) Gloss (A)", 2D) = "white" {}
		[noscaleoffset]_MainNormal ("Main Normal", 2D) = "bump" {}
		[noscaleoffset]_MainHeightAO ("Main HeightMap (R) AO (G) Noise (B)", 2D) = "white" {}
//		_RStrengthN ("R Strength normales", Range (1,0.05)) = 1

		[Header (G Channel)]
		_SecColor ("Second Tint", Color) = (1,1,1,1)
		_SecTex ("Secondary Albedo (RGB) Gloss (A)", 2D) = "white" {}
		[noscaleoffset]_SecNormal ("Secondary Normal", 2D) = "bump" {}
		[noscaleoffset]_SecHeightAO ("Sec HeightMap (R) AO (G)  Noise (B)", 2D) = "white" {}
		_GValue ("G Hardness", Range (0,1)) = 1
		_NoiseIntensityG ("Noise Intensity", Range (0,10)) = 1
		_HeightIntensityG ("Height intensity", Range (0,2)) = 1
//		_GStrengthN ("G Strength normales", Range (1,0.05)) = 1

		[Header (B Channel)]
		_ThirdColor ("Third Tint", Color) = (1,1,1,1)
		_ThirdTex ("Third Albedo (RGB) Gloss (A)", 2D) = "white" {}
		[noscaleoffset]_ThirdNormal ("Third Normal", 2D) = "bump" {}
		[noscaleoffset]_ThirdHeightAO ("Third HeightMap (R) AO (G)  Noise (B)", 2D) = "white" {}
		_BValue ("B Hardness", Range (0,1)) = 1
		_NoiseIntensityB ("Noise Intensity", Range (0,10)) = 1
		_HeightIntensityB ("Height intensity", Range (0,2)) = 1
//		_BStrengthN ("B Strength normales", Range (1,0.05)) = 1

		[Header (Water Parameters)]
		[Header (A Channel)]
		_ParallaxWater ("Parallax", Range (0,1)) = 1
		_AAmount ("Water amount displacement", Range (-0.5, 0.5)) = 0
		_WetColor ("Wet map color", Color) = (1,1,1,1)
		_Offset ("Wet map offset", Range (0, 1)) = 0
		_HardnessWet ("Wet map hardness", Range (0,1)) = 1
		_WaterColor ("Water Tint", Color) = (1,1,1,1)
		_AValue ("A Hardness", Range (0,1)) = 1
		_IntensityCloud ("Intensity cloud", Range (0,10)) = 1
		_IntensitySplatter ("Intensity splatter", Range (0,10)) = 1
		_HeightIntensity ("Height intensity", Range (0,1)) = 1
		_IntensityRefl ("Reflection intensity", Range (0,1)) = 1

		_Noise ("Noise Tex (R cloud, G splatter, B useless)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard addshadow vertex:vert
		#pragma target 4.0
		#include "CGINC_CustomFunctions.cginc"

		float4 _MainColor;
		float4 _SecColor;
		float4 _ThirdColor;

		float4 _WaterColor;
		float4 _WetColor;

		sampler2D _MainTex;
		sampler2D _MainNormal;
		sampler2D _MainHeightAO;

		sampler2D _SecTex;
		sampler2D _SecNormal;
		sampler2D _SecHeightAO;

		sampler2D _ThirdTex;
		sampler2D _ThirdNormal;
		sampler2D _ThirdHeightAO;

		sampler2D _WaterNormales;

		sampler2D _Noise;

		samplerCUBE _Cube;

		half _GValue;
		half _BValue;
		half _AValue;

		half _HeightIntensityG;
		half _NoiseIntensityG;
		half _HeightIntensityB;
		half _NoiseIntensityB;
		half _Metallic;

//		half _RStrengthN;
//		half _GStrengthN;
//		half _BStrengthN;

		half _NoiseTile;
		half _AAmount;
		half _IntensityCloud;
		half _IntensitySplatter;
		half _Offset;
		half _HardnessWet;
		half _HeightIntensity;
		half _IntensityRefl;
		half _ParallaxWater;
		half _ParallaxGeneral;

		struct Input {
			float2 uv_MainTex;
			float2 uv_SecTex;
			float2 uv_ThirdTex;
			float2 uv_Noise;
			float3 viewDir;
			float4 color:COLOR;
			float3 worldRefl;
			INTERNAL_DATA
		};

		void vert (inout appdata_full v)
		{
			v.vertex.xyz += v.normal * _AAmount * v.color.a;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float3 worldRefl = WorldReflectionVector (IN, o.Normal);
			fixed4 reflcol = texCUBE (_Cube, worldRefl);

			// Water part
			fixed4 noise = tex2D (_Noise, IN.uv_Noise);
			fixed cloud = (noise.r * _IntensityCloud);
			fixed splatter = (noise.g * _IntensitySplatter);

			fixed4 hMain = tex2D (_MainHeightAO, IN.uv_MainTex);
			fixed4 hSec = tex2D (_SecHeightAO, IN.uv_SecTex);
			fixed4 hThird = tex2D (_ThirdHeightAO, IN.uv_ThirdTex);

			float hParallax = lerp (hMain, hSec, IN.color.g);
			hParallax = lerp (hSec, hThird, IN.color.b);
			// Parallax Generale
			float2 offsetGeneral = ParallaxOffset (hParallax, _ParallaxGeneral, IN.viewDir);
			IN.uv_MainTex += offsetGeneral;
			IN.uv_SecTex += offsetGeneral;
			IN.uv_ThirdTex += offsetGeneral;

			// HeightMap & Occlusion
			hMain = tex2D (_MainHeightAO, IN.uv_MainTex);
			hSec = tex2D (_SecHeightAO, IN.uv_SecTex);
			hThird = tex2D (_ThirdHeightAO, IN.uv_ThirdTex);

			fixed h = hMain.r * IN.color.r;

			// GB Mask vertex color
			float gMask = clamp ((((IN.color.g - (h - 0.1) * _HeightIntensityG) + (IN.color.g - hSec.b * _NoiseIntensityG)) / _GValue), 0, 1);
			h += hSec.r * gMask;
			float bMask = clamp ((((IN.color.b - (h - 0.1) * _HeightIntensityB) + (IN.color.b - hThird.b  * _NoiseIntensityB)) / _BValue), 0, 1);
			h += hThird.r * bMask;

			hSec *= gMask;
			hThird *= bMask;

			// Water Mask
			float aMask = clamp ((IN.color.a - ((h - 0.1) * _HeightIntensity) - cloud)/((1 - _AValue) * 2), 0, 1);
			float aMaskOffset = clamp (((((IN.color.a - (h - 0.1) * _Offset) - cloud - splatter) )/((1 - _HardnessWet) * 2)), 0, 1);

			float2 offset = ParallaxOffset ((1 - aMask/0.8), _ParallaxWater, IN.viewDir);
			IN.uv_MainTex += offset * aMask;	

			fixed3 flatNormal = float3 (0,0,1);

			//Albedos
			fixed4 c = lerp (tex2D (_MainTex, IN.uv_MainTex) * _MainColor, tex2D (_SecTex, IN.uv_SecTex) * _SecColor, gMask);
			c = lerp (c, tex2D (_ThirdTex, IN.uv_ThirdTex) * _ThirdColor, bMask);

			//Normales 
			fixed3 n = lerp (UnpackNormal (tex2D (_MainNormal, IN.uv_MainTex)), UnpackNormal (tex2D (_SecNormal, IN.uv_SecTex)), gMask);
			n = lerp (n, UnpackNormal (tex2D (_ThirdNormal, IN.uv_ThirdTex)), bMask);

			//Smoothness
			fixed s = c.a;

			// Final Textures
			float3 cFinal = lerp (c, c * _WetColor, aMask);
			cFinal = lerp (cFinal, c * _WaterColor, aMask);
			float3 nFinal = lerp (n, flatNormal, IN.color.a);
			float mFinal = lerp (_Metallic, 0.9, aMaskOffset);
			float sFinal = lerp (s, 0.9, aMask);
//			sFinal = lerp (sFinal, 0.9, aMaskOffset);
			fixed occ = saturate (hMain.g + hSec.g + hThird.g);
			float3 emiss = reflcol.rgb * aMaskOffset * _IntensityRefl;

			o.Albedo = saturate (cFinal.rgb);			
//			o.Emission = saturate (emiss);
			o.Normal = nFinal;
			o.Metallic = saturate (mFinal);
			o.Occlusion = occ;
			o.Smoothness = saturate (sFinal);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
