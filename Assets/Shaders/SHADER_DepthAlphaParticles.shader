Shader "Custom/SHADER_DepthAlphaParticles"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _PowerFresnel ("Fresnel Power",float) = 1
        _RangeFresnel ("Fresnel Range", float) = 0.0
        _Threshold ("Fresnel Range", float) = 0.0
        _Range ("Fresnel Range", float) = 0.0
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
            float3 viewDir;
            float3 worldNormal;
            float4 screenPos;
            float4 color:COLOR;
        };

        half _PowerFresnel;
        half _RangeFresnel;
        half _Threshold;
        half _Range;
        fixed4 _Color;
        
        sampler2D _CameraDepthTexture;
        
        half4 LightingUnlit (SurfaceOutputStandard s, UnityGI gi)
        {
            half4 c;
            c.rgb = s.Albedo;
            c.a = s.Alpha;
            
            return c;   
        }
        
        inline void LightingUnlit_GI( SurfaceOutputStandard s, UnityGIInput data, inout UnityGI gi ) {
            #if defined(UNITY_PASS_DEFERRED) && UNITY_ENABLE_REFLECTION_BUFFERS
                 gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal);
            #else
                  Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.Smoothness, data.worldViewDir, s.Normal, lerp(unity_ColorSpaceDielectricSpec.rgb, s.Albedo, s.Smoothness));
                   gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal, g);
              #endif
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
             half fresnel = saturate (dot(IN.viewDir, IN.worldNormal) - _RangeFresnel);
             fresnel = pow (fresnel, _PowerFresnel);

			float texDepth = LinearEyeDepth (tex2Dproj (_CameraDepthTexture, UNITY_PROJ_COORD (IN.screenPos))).r;
			float inter = saturate ((abs (texDepth - IN.screenPos.w))/ _Threshold);
        
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb * IN.color.rgb;
//            o.Albedo = depth;
            o.Alpha = fresnel * IN.color.a * inter;
//            o.Alpha =1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
