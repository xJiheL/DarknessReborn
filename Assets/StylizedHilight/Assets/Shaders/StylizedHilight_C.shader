// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FoxWork/StylizedHilight_C"
{
	Properties
	{
		_BaseColor("Base Color", Color) = (0.2361592,0.6153711,0.6691177,0)
		_BrightEffect("Albedo Highlight", Range( 0.5 , 1)) = 1
		_DarkEffect("Shadow Strength", Range( 0 , 0.5)) = 0.2737041
		_ShadowRange("Shadow Range", Range( -1 , 1)) = 0.4829271
		_SpecularCurveMap("Spec curve map", 2D) = "white" {}
		_SpecularColor("Spec Color", Color) = (0,0,0,0)
		_MaskStrength("Mask Strength", Range( 0 , 1)) = 0
		_SpecularRange("Spec Range", Range( 0 , 1000)) = 193
		_SpecularHardness("Spec Hardness", Float) = 0
		_SpecularStrength("Spec Strength", Range( 0 , 1)) = 0
		_DistortionThickness("Distortion Thickness", Range( 0 , 20)) = 0
		_DistortionStrength("Distortion Strength", Range( -1 , 1)) = 0
		 _texcoord( "", 2D ) = "white" {}
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float3 worldNormal;
			INTERNAL_DATA
			float3 worldPos;
			float2 uv_texcoord;
		};

		uniform float _ShadowRange;
		uniform float _BrightEffect;
		uniform float4 _BaseColor;
		uniform float _DarkEffect;
		uniform float _SpecularStrength;
		uniform sampler2D _SpecularCurveMap;
		uniform float4 _SpecularCurveMap_ST;
		uniform float _MaskStrength;
		uniform float _SpecularHardness;
		uniform float _DistortionStrength;
		uniform float _DistortionThickness;
		uniform float _SpecularRange;
		uniform float4 _SpecularColor;
		uniform float _OutLineWidth;
		uniform float4 _OutLineColor;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			v.vertex.xyz += 0;
		}

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			o.Normal = float3(0,0,1);
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult14 = dot( ase_worldNormal , ase_worldlightDir );
			float4 ifLocalVar10 = 0;
			if( dotResult14 > _ShadowRange )
				ifLocalVar10 = ( _BrightEffect * _BaseColor );
			else if( dotResult14 < _ShadowRange )
				ifLocalVar10 = ( _BaseColor * _DarkEffect );
			float ifLocalVar115 = 0;
			if( dotResult14 > _ShadowRange )
				ifLocalVar115 = 1.0;
			else if( dotResult14 < _ShadowRange )
				ifLocalVar115 = 0.0;
			float SpeShadowMask118 = ifLocalVar115;
			float2 uv_SpecularCurveMap = i.uv_texcoord * _SpecularCurveMap_ST.xy + _SpecularCurveMap_ST.zw;
			float MaskR48_g88 = tex2D( _SpecularCurveMap, uv_SpecularCurveMap ).r;
			float Mask33_g88 = pow( ( MaskR48_g88 + _MaskStrength ) , _SpecularHardness );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3 normalizeResult17_g88 = normalize( ase_worldBitangent );
			float2 appendResult66_g88 = (float2(_DistortionThickness , 1.0));
			float2 uv_TexCoord64_g88 = i.uv_texcoord * appendResult66_g88;
			float OffsetG49_g88 = tex2D( _SpecularCurveMap, uv_TexCoord64_g88 ).g;
			float3 ase_normWorldNormal = normalize( ase_worldNormal );
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 normalizeResult4_g89 = normalize( ( ase_worldViewDir + ase_worldlightDir ) );
			float dotResult14_g88 = dot( ( normalizeResult17_g88 + ( ( _DistortionStrength * OffsetG49_g88 ) * ase_normWorldNormal ) ) , normalizeResult4_g89 );
			o.Emission = ( ifLocalVar10 + ( SpeShadowMask118 * ( _SpecularStrength * max( 0.01 , saturate( ( Mask33_g88 * pow( max( 0.01 , sqrt( ( 1.0 - pow( dotResult14_g88 , 2.0 ) ) ) ) , ( _SpecularRange * 8 ) ) ) ) ) * _SpecularColor ) ) ).rgb;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit keepalpha fullforwardshadows vertex:vertexDataFunc 

		ENDCG
	}
	Fallback "Diffuse"
}