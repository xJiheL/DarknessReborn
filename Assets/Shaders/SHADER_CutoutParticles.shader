Shader "Custom/SHADER_CutoutParticles"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Cutout ("Cutout",range (0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 200
        
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf Unlit fullforwardshadows 
        #pragma target 3.0
        #include "UnityPBSLighting.cginc"

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float4 color:COLOR;
        };

        fixed4 _Color;
        half _Cutout;
        
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
            o.Albedo = _Color * IN.color.rgb;
            
            half gradient1 = (1 - IN.uv_MainTex.x) * IN.color.a;
            half gradient2 = IN.uv_MainTex.x * IN.color.a;
            half gradient = lerp (gradient1, gradient1, IN.color.r);
            clip (c.r * gradient - _Cutout);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
