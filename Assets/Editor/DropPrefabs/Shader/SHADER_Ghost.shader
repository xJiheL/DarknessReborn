Shader "Hidden/SHADER_Ghost" {
	Properties {
		_Color ("Color", Color) = (0,1,1,1)
		_Opacity ("Opacity", Range (0,1)) = 0.5
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 200

		ZWrite On

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows alpha:fade
		#pragma target 3.0

		half _Opacity;
		fixed4 _Color;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutputStandard o) {
			o.Albedo = _Color.rgb;
			o.Alpha = _Opacity;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
