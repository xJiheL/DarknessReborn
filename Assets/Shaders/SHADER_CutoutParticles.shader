Shader "Custom/SHADER_CutoutParticles"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _EmissPower ("Emiss",float) = 1
        _Cutout ("Cutout",range (0,1)) = 0.5
        _Power ("Power",float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 200
        
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf Unlit fullforwardshadows alpha:fade
        #pragma target 3.0
        #include "UnityPBSLighting.cginc"
        #include "UnityCG.cginc"

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float4 color:COLOR;
        };

        fixed4 _Color;
        half _Cutout;
        half _EmissPower;
        half _Power;
        
        half4 LightingUnlit (SurfaceOutputStandard s, UnityGI gi)
        {
            half4 c;
            c.rgb = s.Albedo;
            c.rgb += s.Emission;
            c.a = s.Alpha;
            
            return c;   
        }
        
        inline void LightingUnlit_GI (SurfaceOutputStandard s, UnityGIInput data, inout UnityGI gi) {
            #if defined(UNITY_PASS_DEFERRED) && UNITY_ENABLE_REFLECTION_BUFFERS
                 gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal);
            #else
                  Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.Smoothness, data.worldViewDir, s.Normal, lerp(unity_ColorSpaceDielectricSpec.rgb, s.Albedo, s.Smoothness));
                   gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal, g);
              #endif
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            fixed4 cMov = tex2D (_MainTex, float2 (IN.uv_MainTex.x + _Time.x * -5, IN.uv_MainTex.y)*.6);
            o.Albedo = _Color * IN.color.rgb; // * IN.color.rgb
            o.Emission = c.r * pow (cMov.g,_Power) * _Color * _EmissPower * IN.color.rgb * ((1 - IN.uv_MainTex.x)/0.17);
            
//            o.Alpha = c.b;
            o.Alpha = c.r * _Color.a;
            
            clip (saturate (c.r *  pow (cMov.g,1) + ((1 - IN.uv_MainTex.x)/2)) - IN.color.a - _Cutout);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
