Shader "DarknessReborn/Standard/Base Anisotropic"
{
    Properties
    {
        [Header(Base)]
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        
        [Header(Emission)]
        _ColorE ("Color Emissive", Color) = (0,0,0,0)
        _IntensityEmiss ("Intensity Emissive", float) = 1
        
        [Header(Lighting)]        
        [Header(Specular)]
        [NoScaleOffset]_SmoothTex ("Smoothness", 2D) = "white" {}
        _Smoothness ("Smoothness Intensity", range (0,1)) = 1
        _MinSpec ("Min", float) = 0
        _MaxSpec ("Max", float) = 0
        
        [Header(Anisotropic)]
        _ShiftMap ("ShiftMap", 2D) = "grey" {}
		_SpecMask ("Specular 02 filter", 2D) = "white" {}
        _ColorSpec01 ("Color Specular 01", Color) = (1,1,1,1)
		_Ani01 ("Specular 01 Glossiness", Range (1,256)) = 256
		_AniShift01 ("Specular 01 Shift", Range (-1.0,1.0)) = 0.0
		_ColorSpec02 ("Color Specular 02", Color) = (1,1,1,1)
		_Ani02 ("Specular 02 Glossiness", Range (1,256)) = 256
		_AniShift02 ("Specular 02 Shift", Range (-1.0,1.0)) = 0.0
        
        [Header(Shadow)]
        _TintShadow ("Tint Shadow", Color) = (0,0,0,1)
        _MinShadow ("Min", float) = 0
        _MaxShadow ("Max", float) = 0

        [Header(Rim Light)]
        _RimLightColor ("Rim Light Color", Color) = (1,1,1,0)
        _RimShadowColor ("Rim Shadow Color", Color) = (0,0,0,0)
		_RangeTransition ("Transition", range (-.15,1)) = 1
		_RimPower ("Rim Power", float) = 1
		_RimHardness ("Rim Hardness", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Cartoon vertex:vert fullforwardshadows 
        #pragma target 4.0
        #include "Lighting.cginc"
           
        fixed4 _Color;
        sampler2D _MainTex;
        
        half _IntensityEmiss;
        fixed4 _ColorE;

        sampler2D _SmoothTex;
        half _Smoothness;
        half _MinSpec;
        half _MaxSpec;
        
       	sampler2D _ShiftMap;
		sampler2D _SpecMask;  
        fixed4 _ColorSpec01;
		float _Ani01;
		float _AniShift01;
		fixed4 _ColorSpec02;
		float _Ani02;
		float _AniShift02;   
         
        fixed4 _RimLightColor;
        fixed4 _RimShadowColor;
        half _RangeTransition;
        half _RimPower;
        half _RimHardness;
        
        fixed4 _TintShadow;
        half _MinShadow;
        half _MaxShadow;     

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
            float2 uv_ShiftMap;
            float4 Tangent_Output;
			float3 Normal_Output;
        };
        
        struct CustomSurfaceOutput
        {
            half3 Albedo;
            half Alpha;
            half3 Normal;
            half3 Emission;
            half Smoothness;
            half RimLight;
            half3 WorldNormal;
            fixed4 Direction;
            half Shift;
        };
        
        //half3 viewDir
        half4 LightingCartoon (CustomSurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
        {
            s.Normal = normalize (s.Normal);           
            
            half3 halfVector = normalize (lightDir + viewDir); 
            half nDotl = max (0,dot(s.Normal, lightDir));
            half nDotH = max (0, dot(s.Normal, halfVector));
            
            // Shadow
            half shadow = saturate (1 - smoothstep (_MinShadow, _MaxShadow, nDotl));            

            // Spec            
            half spec =  (1 - smoothstep (_MinSpec, _MaxSpec, 1 - nDotl)) * s.Smoothness;
            
            // Rim
            nDotH = saturate (nDotH - _RangeTransition);
            half3 rimLight = _RimLightColor.rgb * s.RimLight * _LightColor0;
            rimLight *= nDotH;
            half3 rimShadow = _RimShadowColor.rgb * s.RimLight * _LightColor0;
            rimShadow *= (1 - nDotH);
            
            //Anisotropic
            float Diff = saturate ((nDotl + 0.25) / 1.25);

			//Specular var
			float3 biNormal = cross (s.WorldNormal, s.Direction) * s.Direction.w;
			float3 lightReflect = reflect (-lightDir, s.WorldNormal);
			float3 viewReflect = reflect (-viewDir, s.WorldNormal);
			float3 reflectVect = normalize (lightReflect + viewReflect);

			//Specular 01
			float3 shiftBiNormal1 = normalize(biNormal + (s.Shift + _AniShift01) * s.WorldNormal);
			float dotSpec01 = dot (shiftBiNormal1, reflectVect);
			float3 Spec01 = pow (exp(-dotSpec01 * dotSpec01), _Ani01)* _ColorSpec01.rgb;

			//Specular 02
			float3 shiftBiNormal2 = normalize(biNormal + (s.Shift + _AniShift02) * s.WorldNormal);
			float dotSpec02 = dot (shiftBiNormal2, reflectVect);
			float3 Spec02 = pow (exp(-dotSpec02 * dotSpec02), _Ani02) * _ColorSpec02.rgb;
            
            half4 c;

            c.rgb = s.Albedo;
            
            c.rgb = lerp (c.rgb, _TintShadow.rgb, shadow * _TintShadow.a);
            c.rgb += Spec01;
            c.rgb += Spec02;
            c.rgb *= _LightColor0 * atten;
            
            c.rgb += rimLight;           
            c.rgb += rimShadow;
            
//          c.rgb += s.Emission;
              
            c.a = s.Alpha;
            
            return c;
        }
        
        void vert (inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT (Input, o);
			o.Tangent_Output = v.tangent.xyzw;
			o.Normal_Output = v.normal;
		}

        void surf(Input IN, inout CustomSurfaceOutput o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            
            half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
            
            o.Albedo = c.rgb;
            o.RimLight = saturate (pow (rim, _RimPower) * _RimHardness);
            o.Emission = _ColorE.rgb * _IntensityEmiss; 
            o.Smoothness = tex2D (_SmoothTex, IN.uv_MainTex) * _Smoothness;
            o.Shift = tex2D (_ShiftMap,IN.uv_ShiftMap) - 0.5;
			o.WorldNormal = IN.Normal_Output;
			o.Direction = IN.Tangent_Output;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
