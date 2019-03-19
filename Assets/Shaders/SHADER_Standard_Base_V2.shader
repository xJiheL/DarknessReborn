Shader "DarknessReborn/Standard/Base_V2"
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
//        _SpecTex ("Spec Mask", 2D) = "white" {}
        _ColorS ("Spec Color", Color) = (1,1,1,0)
        [NoScaleOffset]_SmoothTex ("Smoothness", 2D) = "white" {}
        _Specular ("Specular", range (0,1)) = 1
        _Smoothness ("Smoothness", range (0,1)) = 1
        _SpecStrength ("Spec Strength", range (0,1)) = 1
        _MinSpec ("Min", float) = 0
        _MaxSpec ("Max", float) = 0
//        _SpecStretchH ("_SpecStretchH", float) = 0
//        _SpecStretchV ("_SpecStretchV", float) = 0

        [Header(Rim Light)]
        _RimLightColor ("Rim Light Color", Color) = (1,1,1,0)
        _RimShadowColor ("Rim Shadow Color", Color) = (0,0,0,0)
		_RangeTransition ("Transition", range (-.15,1)) = 1
		_RimPower ("Rim Power", float) = 1
		_RimHardness ("Rim Hardness", float) = 1
        
        [Header(Shadow)]
        _TintShadow ("Tint Shadow", Color) = (0,0,0,1)
        _ShadowStrength ("Shadow Strength", range (0,1)) = 1
        _MinShadow ("Min", float) = 0
        _MaxShadow ("Max", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Cartoon fullforwardshadows 
        #pragma target 3.0
        #include "UnityPBSLighting.cginc"
           
        fixed4 _Color;
        sampler2D _MainTex;
        sampler2D _Metallic;
        
        half _IntensityEmiss;
        fixed4 _ColorE;

//        sampler2D _SpecTex;
        fixed4 _ColorS;
        sampler2D _SmoothTex;
        half _Specular;
        half _Smoothness;
        half _MinSpec;
        half _MaxSpec;
        half _SpecStrength;        
//        half _SpecStretchH;        
//        half _SpecStretchV;       
         
        fixed4 _RimLightColor;
        fixed4 _RimShadowColor;
        half _RangeTransition;
        half _RimPower;
        half _RimHardness;
        
        fixed4 _TintShadow;
        half _MinShadow;
        half _MaxShadow;
        half _ShadowStrength;       

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
        };
        
        struct CustomSurfaceOutput
        {
            half3 Albedo;
            half Alpha;
            half3 Normal;
            half3 Emission;
            half Smoothness;
            half Specular;
            half RimLight;
        };
        
        //half3 viewDir
        half4 LightingCartoon (CustomSurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
        {
            s.Normal = normalize (s.Normal);
            
            
            
            // Shadow
//            half shadow = smoothstep (_MinShadow, _MaxShadow, nDotl);            
//            shadow = lerp (1, shadow, _ShadowStrength);

            // Spec
            half3 halfVector = normalize (lightDir + viewDir); 
            half nDotl = max (0,dot(s.Normal, lightDir));
            float nDotH = max (0, dot(s.Normal, halfVector));
            
//            half3 spec = saturate (_ColorS.rgb * _LightColor0.rgb * clamp (pow (nDotH, s.Specular * s.Specular * 5000),0.01,1) * s.Specular) ;
            half3 spec = pow (nDotH, s.Specular * 128) * s.Smoothness * _ColorS.rgb;
            float3 lightinModel = (nDotl * s.Albedo + spec) * _LightColor0.rgb;;
//            half spec = _ColorS.rgb * _LightColor0.rgb * hDotn;
            //half spec = nh;
//            spec = smoothstep (_MinSpec, _MaxSpec, spec) * 2;
//            spec = lerp (0, spec, _SpecStrength);        
            
            // Rim light
//            nDotH = saturate (nDotH - _RangeTransition);
//            half3 rimLight = _RimLightColor.rgb * s.RimLight * _LightColor0;
//            rimLight *= nDotH;
//            half3 rimShadow = _RimShadowColor.rgb * s.RimLight * _LightColor0;
//            rimShadow *= (1 - nDotH);
            
            half4 c;
            
//            c.rgb = saturate (s.Albedo * _LightColor0.rgb * ((shadow * .5) * 2)) + spec;
//            c.rgb += saturate(_TintShadow * s.Albedo *  (1 - shadow));           
//            c.rgb += rimLight;           
//            c.rgb += rimShadow;           
//            c.rgb = saturate (c.rgb);
            //c.rgb = (s.Albedo * _LightColor0.rgb * diff + spec) * (atten *2);
            c.rgb = lightinModel * (atten * 2);

            c.rgb += s.Emission;
            
            c.a = s.Alpha;
            
            return c;
        }

        void surf(Input IN, inout CustomSurfaceOutput o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            
            half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
            
            o.Albedo = c.rgb;
            o.RimLight = saturate (pow (rim, _RimPower) * _RimHardness);
            o.Emission = _ColorE.rgb * _IntensityEmiss; 
//            o.Smoothness = tex2D (_SmoothTex, IN.uv_MainTex) * _Smoothness;
            o.Specular = _Specular;
            o.Smoothness = _Smoothness;
//            o.Occlusion = m.g;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
