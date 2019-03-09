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
        _TintSpec ("Tint Spec", Color) = (0,0,0,1)
        _SpecStrength ("Spec Strength", range (0,1)) = 1
        _MinSpec ("Min", float) = 0
        _MaxSpec ("Max", float) = 0
        
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
        
        fixed4 _TintSpec;
        half _MinSpec;
        half _MaxSpec;
        half _SpecStrength;        
         
        fixed4 _TintShadow;
        half _MinShadow;
        half _MaxShadow;
        half _ShadowStrength;       

        struct Input
        {
            float2 uv_MainTex;
        };
        
        //half3 viewDir
        half4 LightingCartoon (SurfaceOutput s, half3 lightDir, half atten)
        {
            s.Normal = normalize (s.Normal);
            
            half nDotl = dot(s.Normal, lightDir);
            
            half shadow = smoothstep (_MinShadow, _MaxShadow, nDotl);            
            shadow = lerp (1, shadow, _ShadowStrength);
            
            half spec = smoothstep (_MinSpec, _MaxSpec, nDotl) * 2;
            spec = lerp (0, spec, _SpecStrength);
        
            half4 c;
            
            c.rgb = saturate (s.Albedo  * _LightColor0.rgb * ( (shadow * .5 + spec) * 2));
            c.rgb += saturate(_TintShadow * s.Albedo *  (1 - shadow));
            c.rgb += saturate(_TintSpec * s.Albedo *  spec);
            
            c.rgb = saturate (c.rgb);
            
            c.a = s.Alpha;
            
            return c;
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Emission = _ColorE.rgb * _IntensityEmiss; 
//            o.Occlusion = m.g;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
