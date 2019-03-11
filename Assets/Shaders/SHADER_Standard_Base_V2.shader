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
        [NoScaleOffset]_SpecularTex ("Spec", 2D) = "white" {}
        _SpecStrength ("Spec Strength", range (0,1)) = 1
        _MinSpec ("Min", float) = 0
        _MaxSpec ("Max", float) = 0
//        _SpecStretchH ("_SpecStretchH", float) = 0
//        _SpecStretchV ("_SpecStretchV", float) = 0

        [Header(Rim Light)]
        _RimColor ("Rim Color", Color) = (1,1,1,0.0)
		_RimPower ("Rim Power", float) = 1
        
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
        sampler2D _SpecularTex;
        half _MinSpec;
        half _MaxSpec;
        half _SpecStrength;        
//        half _SpecStretchH;        
//        half _SpecStretchV;       
         
        fixed4 _RimColor;
        half _RimPower;
        
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
            half Specular;
            half RimLight;
        };
        
        //half3 viewDir
        half4 LightingCartoon (CustomSurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
        {
            s.Normal = normalize (s.Normal);
            
            half nDotl = dot(s.Normal, lightDir);
            
            // Shadow
            half shadow = smoothstep (_MinShadow, _MaxShadow, nDotl);            
            shadow = lerp (1, shadow, _ShadowStrength);

            // Spec
            half3 h = normalize (lightDir + viewDir);
            half diff = max (0, dot(s.Normal, lightDir));
            float nh = max (0, dot(s.Normal, h));
            half spec = pow (nh, 48);
            spec = smoothstep (_MinSpec, _MaxSpec, spec) * 2;
            spec = lerp (0, spec, _SpecStrength);
            spec *= s.Specular;
            
            
            // Rim light
            half3 rim = _RimColor.rgb * s.RimLight * _LightColor0;
            rim *= nh;
            
            half4 c;
            
            c.rgb = saturate (s.Albedo  * _LightColor0.rgb * ( (shadow * .5 + spec) * 2));
            c.rgb += saturate(_TintShadow * s.Albedo *  (1 - shadow));           
            c.rgb += rim;           
            c.rgb = saturate (c.rgb);

            c.rgb += s.Emission;
            
            c.a = s.Alpha;
            
            return c;
        }

        void surf(Input IN, inout CustomSurfaceOutput o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            
            half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
            
            o.Albedo = c.rgb;
            o.RimLight = pow (rim, _RimPower);
            o.Emission = _ColorE.rgb * _IntensityEmiss; 
            o.Specular = tex2D (_SpecularTex, IN.uv_MainTex);
//            o.Occlusion = m.g;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
