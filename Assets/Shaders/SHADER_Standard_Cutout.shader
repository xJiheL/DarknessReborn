Shader "DarknessReborn/Standard/Cutout"
{
    Properties
    {
        [Header(Base)]
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Cutout ("Cutout", Range (0,1)) = .5
        
        [Header(Emission)]
        _ColorE ("Color Emissive", Color) = (0,0,0,0)
        _IntensityEmiss ("Intensity Emissive", float) = 1
        
        [Header(Lighting)]        
        [Header(Specular)]
        _TintSpec ("Spec Color", Color) = (1,1,1,0)
        [NoScaleOffset]_SmoothTex ("Smoothness", 2D) = "white" {}
        _Smoothness ("Smoothness Intensity", range (0,1)) = 1
        _MinSpec ("Min", float) = 0
        _MaxSpec ("Max", float) = 0
        
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
        #pragma surface surf Cartoon fullforwardshadows 
        #pragma target 3.0
        #include "UnityPBSLighting.cginc"
           
        fixed4 _Color;
        sampler2D _MainTex;
        half _Cutout;
        
        half _IntensityEmiss;
        fixed4 _ColorE;

        fixed4 _TintSpec;
        sampler2D _SmoothTex;
        half _Smoothness;
        half _MinSpec;
        half _MaxSpec;       
         
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
        };
        
        struct CustomSurfaceOutput
        {
            half3 Albedo;
            half Alpha;
            half3 Normal;
            half3 Emission;
            half Smoothness;
            half RimLight;
        };

        half4 LightingCartoon (CustomSurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
        {
            s.Normal = normalize (s.Normal);           
            
            half3 halfVector = normalize (lightDir + viewDir); 
            half nDotl = max (dot(s.Normal, lightDir), 0);
            half nDoth = max (dot(s.Normal, halfVector), 0);
            
            // Shadow
            half shadow = saturate (1 - smoothstep (_MinShadow, _MaxShadow, nDotl));            

            // Spec            
            half spec =  smoothstep (_MinSpec, _MaxSpec, pow (nDoth, s.Smoothness * s.Smoothness)) * (1- shadow);
            
            // Rim
            nDoth = saturate (nDoth - _RangeTransition);
            half3 rimLight = _RimLightColor.rgb * s.RimLight * _LightColor0;
            rimLight *= nDoth;
            half3 rimShadow = _RimShadowColor.rgb * s.RimLight * _LightColor0;
            rimShadow *= (1 - nDoth);
            
            half4 c;

            c.rgb = saturate (s.Albedo * _LightColor0.rgb);
            c.rgb = lerp (c.rgb, _TintShadow.rgb * _LightColor0.rgb, shadow * _TintShadow.a);
            c.rgb = lerp (c.rgb, _TintSpec.rgb * _LightColor0.rgb, spec * _TintSpec.a);
            c.rgb += rimLight;           
            c.rgb += rimShadow;
//            c.rgb *= atten;
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
            o.Smoothness = tex2D (_SmoothTex, IN.uv_MainTex) * _Smoothness;
            clip (c.a - _Cutout);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
