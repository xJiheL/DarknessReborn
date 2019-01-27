Shader "VertexPainter/Shader_VertexPaint_Emission" {
	Properties {
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert
		#pragma target 3.0

		uniform float _ShowAlpha;

		struct Input {
			float4 color:COLOR;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			if (_ShowAlpha < 0.5)
			{
				o.Emission = IN.color.rgb;
			}
			else
			{
				o.Emission = IN.color.a;
			}
		}
		ENDCG
	}
	FallBack "Diffuse"
}
