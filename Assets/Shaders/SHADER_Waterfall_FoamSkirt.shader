Shader "Custom/SHADER_Waterfall_FoamSkirt"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _SpeedTex ("Speed & Direction", vector) = (0,0,0,0)
        _IntensityEmiss ("Intensity Emiss", float) = 1
        _HardnessVC ("Hardness Vertex Color", float) = 1.0
        _RangeVC ("Range Vertex Color", float) = 1.0
        _Cutout ("Cutout", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        #include "UnityCG.cginc"

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float4 color:COLOR;
        }; 
        
        half _Cutout;
        half _IntensityEmiss;
        half _HardnessVC;
        half _RangeVC;
        
        fixed4 _Color;
        fixed4 _SpeedTex;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            half2 uvPanner = IN.uv_MainTex;
            uvPanner.x += _Time.x * _SpeedTex.x;
            uvPanner.y += _Time.x * _SpeedTex.y;
            fixed4 c = tex2D (_MainTex, uvPanner) ;
            
            o.Emission = _Color * _IntensityEmiss;
            
            IN.color.r = pow (saturate (IN.color.r - _RangeVC), _HardnessVC);
            
            clip (c.r * IN.color.r - _Cutout);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
