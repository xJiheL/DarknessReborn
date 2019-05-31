Shader "Custom/Shader_Waterfall_Ripples" {
	Properties {
		_ColorFoam ("Color Foam", Color) = (1,1,1,1)
		_NoiseFoam ("Noise Foam", 2D) = "white" {}
		_SpeedNoise ("Speed Noise", vector) = (0,0,0,0)
		_HardnessVC ("Hardness Vertex Color", Range(0,10)) = 0.0
		_Contrast ("Contrast", Range(0,10)) = 0.0
		_Cutout ("Cutout", Range(0,1)) = 0.0
		_Min ("_Min", float) = 0.0
		_Max ("_Max", float) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Cutout" }
		LOD 200

		Cull Off

		CGPROGRAM
		#pragma surface surf Standard //vertex:vert addshadow
		#pragma target 3.0
		#include "UnityCG.cginc"

		sampler2D _NoiseFoam;
		
		half _HardnessVC;
		half _Contrast;
//		half _Push;
		half _Cutout;
//		float _IntensityEmiss;
//		float _Min;
//		float _Max;

		fixed4 _SpeedNoise;
		fixed4 _ColorFoam;

		half3 Contrast (half3 color, half contrast)
		{
			float3 newColor = saturate (lerp (half3 (0.3, 0.5, 0.5), color, contrast));
			newColor = GammaToLinearSpace (newColor);
			return newColor;
		}

		struct Input {
			float2 uv_NoiseFoam;
			float4 color:COLOR;
			float mask;
//			float3 viewDir;
		};

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed2 uvWaterTex = float2 (IN.uv_NoiseFoam.x + _Time.x * _SpeedNoise.x, IN.uv_NoiseFoam.y + _Time.x * _SpeedNoise.y);
			fixed4 c = tex2D (_NoiseFoam, uvWaterTex);
			
			uvWaterTex = float2 (IN.uv_NoiseFoam.x + _Time.x * _SpeedNoise.x* -1.5, IN.uv_NoiseFoam.y + _Time.x * _SpeedNoise.y*1.5);
			c += tex2D (_NoiseFoam, uvWaterTex);
			c = saturate (c);

            half mask =  c * pow (saturate(IN.color.r - _Contrast), _HardnessVC);
//			float3 finalColor = lerp (_ColorWater1.rgb, _ColorWater2.rgb, smoothstep (_Min, _Max, c.r + pow (IN.color.r, _HardnessVC)));

			o.Albedo = _ColorFoam;
			o.Emission = o.Albedo;
			o.Metallic = 0;
			o.Smoothness = 0;
			clip (mask * IN.color.a - _Cutout);
		}
		ENDCG
	}
	FallBack "Diffuse"
}

//Shader "FlatKit/Stylized Surface Wind"
//{
//    Properties
//    {
//        _Color ("Color", Color) = (1,1,1,1)
//        
//        /*--->*/
//        [Space(10)]
//        [KeywordEnum(None, Single, Steps, Curve)]_CelPrimaryMode("Cel Shading Mode", Float) = 1
//        _ColorDim ("[_CELPRIMARYMODE_SINGLE]Color Shaded", Color) = (0.85023, 0.85034, 0.85045, 0.85056)
//        _ColorDimSteps ("[_CELPRIMARYMODE_STEPS]Color Shaded", Color) = (0.85023, 0.85034, 0.85045, 0.85056)
//        _ColorDimCurve ("[_CELPRIMARYMODE_CURVE]Color Shaded", Color) = (0.85023, 0.85034, 0.85045, 0.85056)
//        _SelfShadingSize ("[_CELPRIMARYMODE_SINGLE]Self Shading Size", Range(0, 1)) = 0.5
//        _ShadowEdgeSize ("[_CELPRIMARYMODE_SINGLE]Shadow Edge Size", Range(0, 0.5)) = 0.05
//        _Flatness ("[_CELPRIMARYMODE_SINGLE]Localized Shading", Range(0, 1)) = 1.0
//        
//        [IntRange]_CelNumSteps ("[_CELPRIMARYMODE_STEPS]Number Of Steps", Range(1, 10)) = 3.0
//        _CelStepTexture ("[_CELPRIMARYMODE_STEPS][LAST_PROP_STEPS]Cel steps", 2D) = "white" {}
//        _CelCurveTexture ("[_CELPRIMARYMODE_CURVE][LAST_PROP_CURVE]Ramp", 2D) = "white" {}
//        
//        [Space(10)]
//        [Toggle(DR_CEL_EXTRA_ON)] _CelExtraEnabled("Enable Extra Cel Layer", Int) = 0
//        _ColorDimExtra ("[DR_CEL_EXTRA_ON]Color Shaded", Color) = (0.85023, 0.85034, 0.85045, 0.85056)
//        _SelfShadingSizeExtra ("[DR_CEL_EXTRA_ON]Self Shading Size", Range(0, 1)) = 0.6
//        _ShadowEdgeSizeExtra ("[DR_CEL_EXTRA_ON]Shadow Edge Size", Range(0, 0.5)) = 0.05
//        _FlatnessExtra ("[DR_CEL_EXTRA_ON]Localized Shading", Range(0, 1)) = 1.0
//        
//        [Space(10)]
//        [Toggle(DR_SPECULAR_ON)] _SpecularEnabled("Enable Specular", Int) = 0
//        [HDR] _FlatSpecularColor("[DR_SPECULAR_ON]Specular Color", Color) = (0.85023, 0.85034, 0.85045, 0.85056)
//        _FlatSpecularSize("[DR_SPECULAR_ON]Specular Size", Range(0.0, 1.0)) = 0.1
//        _FlatSpecularEdgeSmoothness("[DR_SPECULAR_ON]Specular Edge Smoothness", Range(0.0, 1.0)) = 0
//        
//        [Space(10)]
//        [Toggle(DR_RIM_ON)] _RimEnabled("Enable Rim", Int) = 0
//        [HDR] _FlatRimColor("[DR_RIM_ON]Rim Color", Color) = (0.85023, 0.85034, 0.85045, 0.85056)
//        _FlatRimLightAlign("[DR_RIM_ON]Light Align", Range(0.0, 1.0)) = 0
//        _FlatRimSize("[DR_RIM_ON]Rim Size", Range(0, 1)) = 0.5
//        _FlatRimEdgeSmoothness("[DR_RIM_ON]Rim Edge Smoothness", Range(0, 1)) = 0.5
//        
//        [Space(10)]
//        [Toggle(DR_GRADIENT_ON)] _GradientEnabled("Enable Height Gradient", Int) = 0
//        [HDR] _ColorGradient("[DR_GRADIENT_ON]Gradient Color", Color) = (0.85023, 0.85034, 0.85045, 0.85056)
//        _GradientCenterX("[DR_GRADIENT_ON]Center X", Float) = 0
//        _GradientCenterY("[DR_GRADIENT_ON]Center Y", Float) = 0
//        _GradientSize("[DR_GRADIENT_ON]Size", Float) = 10.0
//        _GradientAngle("[DR_GRADIENT_ON]Gradient Angle", Range(0, 360)) = 0
//        
//        [Space(10)]
//        [Toggle(DR_VERTEX_COLORS_ON)] _VertexColorsEnabled("Enable Vertex Colors", Int) = 0
//        
//        _LightContribution("[FOLDOUT(Advanced Lighting){1}]Light Contribution", Range(0, 1)) = 0
//        
//        [KeywordEnum(None, Multiply, Color)] _UnityShadowMode ("[FOLDOUT(Unity Built-in Shadows){4}]Mode", Float) = 0
//        _UnityShadowPower("[_UNITYSHADOWMODE_MULTIPLY]Power", Range(0, 1)) = 0.2
//        _UnityShadowColor("[_UNITYSHADOWMODE_COLOR]Color", Color) = (0.85023, 0.85034, 0.85045, 0.85056)
//        _UnityShadowSharpness("Sharpness", Range(1, 10)) = 1.0
//        
//        [Space(25)]
//        /*<---*/
//        
//        _MainTex ("Texture", 2D) = "white" {}
//        
//        /*--->*/
//        _TextureImpact("Texture Impact", Range(0, 1)) = 1.0
//        /*<---*/
//        
//        _Glossiness ("Smoothness", Range(0,1)) = 0.5
//        _Metallic ("Metallic", Range(0,1)) = 0.0
//        
//        [Space(10)]
//        [Toggle(VERTEX_WIND_ON)] _VertexWind("Enable Wind", Int) = 0
//        _MaskWind("[VERTEX_WIND_ON]Mask Wind", 2D) = "white"
//        _SpeedWind("[VERTEX_WIND_ON]Wind Speed", float) = 0
//        _AmountWind("[VERTEX_WIND_ON]Wind Amount", float) = 0
//        _DirectionWind("[VERTEX_WIND_ON]Wind Direction & Intensity", vector) = (0,0,0,0)
//    }
//    SubShader
//    {
//        Tags { "RenderType"="Wind" }
//        LOD 200
//
//        CGPROGRAM
//        // Doc: https://docs.unity3d.com/Manual/SL-SurfaceShaders.html
//        #include "DustyroomStylizedLighting.cginc"
//        #pragma surface surfObject DustyroomStylized vertex:vertObject addshadow
//        #pragma target 3.0
//        #pragma require interpolators15
//        #define Input InputObject
//
//        #pragma multi_compile _CELPRIMARYMODE_NONE _CELPRIMARYMODE_SINGLE _CELPRIMARYMODE_STEPS _CELPRIMARYMODE_CURVE
//        #pragma shader_feature DR_CEL_EXTRA_ON
//        #pragma shader_feature DR_GRADIENT_ON
//        #pragma shader_feature DR_SPECULAR_ON
//        #pragma shader_feature DR_RIM_ON
//        #pragma shader_feature DR_VERTEX_COLORS_ON
//        #pragma shader_feature VERTEX_WIND_ON
//        #pragma multi_compile _UNITYSHADOWMODE_NONE _UNITYSHADOWMODE_MULTIPLY _UNITYSHADOWMODE_COLOR
//
//        ENDCG
//    }
//    FallBack "VertexLit"
//    CustomEditor "StylizedSurfaceEditor"
//}

