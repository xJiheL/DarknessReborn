// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FoxWork/StylizedHilightWithBlack_C"
{
	Properties
	{
		_BaseColor("基础颜色", Color) = (0.2361592,0.6153711,0.6691177,0)
		_BlackStrenth("BlackStrenth", Range( 0 , 2)) = 0
		_BlackRange("高光黑色范围", Range( 0 , 1)) = 1
		_BrightEffect("亮部程度", Range( 0.5 , 1)) = 1
		_DarkEffect("暗部程度", Range( 0 , 0.5)) = 0.2737041
		_ShadowRange("阴影范围", Range( -1 , 1)) = 0.4829271
		_OutLineColor("轮廓线颜色", Color) = (0,0,0,0)
		_OutLineWidth("轮廓线宽度", Range( 0 , 0.1)) = 0.071
		_SpecularCurveMap("高光遮罩图", 2D) = "white" {}
		_SpecularColor("高光颜色", Color) = (0,0,0,0)
		_MaskStrength("遮罩强度", Range( 0 , 1)) = 0
		_SpecularRange("高光范围", Range( 0 , 1000)) = 193
		_SpecularHardness("高光硬度", Float) = 0
		_SpecularStrength("高光强度", Range( 0 , 1)) = 0
		_DistortionThickness("扭曲密度", Range( 0 , 20)) = 0
		_DistortionStrength("扭曲强度", Range( -1 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ }
		Cull Front
		CGPROGRAM
		#pragma target 3.0
		#pragma surface outlineSurf Outline nofog  keepalpha noshadow noambient novertexlights nolightmap nodynlightmap nodirlightmap nometa noforwardadd vertex:outlineVertexDataFunc 
		void outlineVertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float outlineVar = _OutLineWidth;
			v.vertex.xyz += ( v.normal * outlineVar );
		}
		inline half4 LightingOutline( SurfaceOutput s, half3 lightDir, half atten ) { return half4 ( 0,0,0, s.Alpha); }
		void outlineSurf( Input i, inout SurfaceOutput o )
		{
			o.Emission = _OutLineColor.rgb;
		}
		ENDCG
		

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
		uniform float _BlackStrenth;
		uniform float _DistortionStrength;
		uniform sampler2D _SpecularCurveMap;
		uniform float _DistortionThickness;
		uniform float _BlackRange;
		uniform float _SpecularRange;
		uniform float4 _SpecularCurveMap_ST;
		uniform float _MaskStrength;
		uniform float _SpecularHardness;
		uniform float _SpecularStrength;
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
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3 normalizeResult17_g138 = normalize( ase_worldBitangent );
			float2 appendResult66_g138 = (float2(_DistortionThickness , 1.0));
			float2 uv_TexCoord64_g138 = i.uv_texcoord * appendResult66_g138;
			float OffsetG49_g138 = tex2D( _SpecularCurveMap, uv_TexCoord64_g138 ).g;
			float3 ase_normWorldNormal = normalize( ase_worldNormal );
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 normalizeResult4_g139 = normalize( ( ase_worldViewDir + ase_worldlightDir ) );
			float dotResult14_g138 = dot( ( normalizeResult17_g138 + ( ( _DistortionStrength * OffsetG49_g138 ) * ase_normWorldNormal ) ) , normalizeResult4_g139 );
			float temp_output_18_0_g138 = sqrt( ( 1.0 - pow( dotResult14_g138 , 2.0 ) ) );
			float temp_output_25_0_g138 = _SpecularRange;
			float org_Spe81_g138 = pow( temp_output_18_0_g138 , ( _BlackRange * temp_output_25_0_g138 ) );
			float2 uv_SpecularCurveMap = i.uv_texcoord * _SpecularCurveMap_ST.xy + _SpecularCurveMap_ST.zw;
			float MaskR48_g138 = tex2D( _SpecularCurveMap, uv_SpecularCurveMap ).r;
			float Mask33_g138 = pow( ( MaskR48_g138 + _MaskStrength ) , _SpecularHardness );
			float temp_output_26_0_g138 = _SpecularStrength;
			float temp_output_44_0_g138 = ( temp_output_26_0_g138 * max( 0.01 , saturate( ( Mask33_g138 * pow( max( 0.01 , temp_output_18_0_g138 ) , ( temp_output_25_0_g138 * 8 ) ) ) ) ) );
			float4 temp_cast_0 = (( SpeShadowMask118 * ( _BlackStrenth * ( ( max( 0.01 , saturate( ( org_Spe81_g138 * Mask33_g138 ) ) ) * temp_output_26_0_g138 ) - temp_output_44_0_g138 ) ) )).xxxx;
			o.Emission = ( ( ifLocalVar10 - temp_cast_0 ) + ( SpeShadowMask118 * ( temp_output_44_0_g138 * _SpecularColor ) ) ).rgb;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit keepalpha fullforwardshadows vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=15401
28;41;1209;680;705.5132;218.0156;1.3;True;False
Node;AmplifyShaderEditor.CommentaryNode;22;-709.1155,-939.5024;Float;False;1083.002;829.0842;Comment;13;19;18;21;17;20;12;13;14;16;10;115;116;117;漫反射;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldNormalVector;12;-609.715,-889.5024;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;13;-659.1155,-737.4025;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;116;-64.82989,-788.2589;Float;False;Constant;_Float0;Float 0;14;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;85;-689.2903,-31.87988;Float;False;946.6144;957.1633;Specular;9;102;96;37;66;83;63;67;60;120;高光;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;117;-57.0299,-710.2595;Float;False;Constant;_Float1;Float 1;14;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-555.8331,-584.9641;Float;False;Property;_ShadowRange;阴影范围;6;0;Create;False;0;0;False;0;0.4829271;0.25;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;14;-362.7139,-816.7028;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-550.2023,-225.4199;Float;False;Property;_DarkEffect;暗部程度;5;0;Create;False;0;0;False;0;0.2737041;0.5;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;63;-606.9257,634.572;Float;False;Property;_SpecularStrength;高光强度;14;0;Create;False;0;0;False;0;0;0.6;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-524.2012,-480.2194;Float;False;Property;_BrightEffect;亮部程度;4;0;Create;False;0;0;False;0;1;1;0.5;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;83;-630.1052,18.12013;Float;False;Property;_SpecularColor;高光颜色;10;0;Create;False;0;0;False;0;0,0,0,0;1,0.9375253,0.4338235,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;102;-612.7385,717.0874;Float;False;Property;_DistortionThickness;扭曲密度;15;0;Create;False;0;0;False;0;0;5;0;20;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;17;-533.302,-402.4197;Float;False;Property;_BaseColor;基础颜色;0;0;Create;False;0;0;False;0;0.2361592,0.6153711,0.6691177,0;0.9294118,0.572549,0.2156863,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;37;-616.129,389.813;Float;False;Property;_SpecularRange;高光范围;12;0;Create;False;0;0;False;0;193;213;0;1000;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;66;-645.4818,464.4651;Float;False;Property;_MaskStrength;遮罩强度;11;0;Create;False;0;0;False;0;0;0.733;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;60;-639.2902,194.2349;Float;True;Property;_SpecularCurveMap;高光遮罩图;9;0;Create;False;0;0;False;0;None;90fa2e89f4a33e74ea98eaf9505e978b;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;67;-590.3817,560.265;Float;False;Property;_SpecularHardness;高光硬度;13;0;Create;False;0;0;False;0;0;4.81;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;96;-573.0099,832.6613;Float;False;Property;_DistortionStrength;扭曲强度;16;0;Create;False;0;0;False;0;0;0.44;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;115;165.2702,-836.3594;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-190.1006,-300.82;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;150;-182.5298,279.8855;Float;False;StylizedAnisotropyHairSpecularWithBlack_C;1;;138;63bc5170c26321644a6da58324bcbc4c;0;8;47;COLOR;1,1,1,0;False;30;SAMPLER2D;;False;25;FLOAT;246.44;False;43;FLOAT;0.5;False;42;FLOAT;241.82;False;26;FLOAT;1;False;69;FLOAT;1;False;57;FLOAT;1;False;2;FLOAT;86;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;-158.9005,-469.8195;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;120;-18.51147,106.2971;Float;False;118;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;118;350.7702,-844.7593;Float;False;SpeShadowMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;10;142.5175,-593.7111;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;7;-51.12437,985.2183;Float;False;642.0007;341.0004;Comment;3;4;5;3;轮廓线;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;147;291.587,25.08453;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;4;16.87554,1035.218;Float;False;Property;_OutLineColor;轮廓线颜色;7;0;Create;False;0;0;False;0;0,0,0,0;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;5;-1.124396,1211.218;Float;False;Property;_OutLineWidth;轮廓线宽度;8;0;Create;False;0;0;False;0;0.071;0.004;0;0.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;137;430.3109,-226.0968;Float;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;119;315.8716,147.7805;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;38;555.7495,-76.24702;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OutlineNode;3;340.8764,1083.218;Float;False;0;True;None;0;0;Front;3;0;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;9;832.955,-12.24629;Float;False;True;2;Float;ASEMaterialInspector;0;0;Unlit;FoxWork/StylizedHilightWithBlack_C;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;-1;False;-1;-1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;14;0;12;0
WireConnection;14;1;13;0
WireConnection;115;0;14;0
WireConnection;115;1;16;0
WireConnection;115;2;116;0
WireConnection;115;4;117;0
WireConnection;21;0;17;0
WireConnection;21;1;20;0
WireConnection;150;47;83;0
WireConnection;150;30;60;0
WireConnection;150;25;37;0
WireConnection;150;43;66;0
WireConnection;150;42;67;0
WireConnection;150;26;63;0
WireConnection;150;69;102;0
WireConnection;150;57;96;0
WireConnection;19;0;18;0
WireConnection;19;1;17;0
WireConnection;118;0;115;0
WireConnection;10;0;14;0
WireConnection;10;1;16;0
WireConnection;10;2;19;0
WireConnection;10;4;21;0
WireConnection;147;0;120;0
WireConnection;147;1;150;86
WireConnection;137;0;10;0
WireConnection;137;1;147;0
WireConnection;119;0;120;0
WireConnection;119;1;150;0
WireConnection;38;0;137;0
WireConnection;38;1;119;0
WireConnection;3;0;4;0
WireConnection;3;1;5;0
WireConnection;9;2;38;0
WireConnection;9;11;3;0
ASEEND*/
//CHKSM=7B541E6DCA633BAF5F116B05513DDDBF424AF4D1