Shader "VertexPainter/Shader_Projector_Brush" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
	}

	SubShader {
		Tags { "RenderType"="Transparent" "IgnoreProjector"="False" "Queue"="Transparent"}
		LOD 200

		ZWrite Off
		ZTest Always
		ColorMask RGB
		Blend DstColor One
		Offset -1, -1
		
		CGPROGRAM
		#pragma surface surf BlinnPhong alpha:fade vertex:vert
		#pragma target 3.0

		uniform sampler2D _MainTex;

		fixed4 _Color;

		uniform float _Falloff;

		uniform float4x4 unity_Projector;

		struct Input {
			float2 uv_MainTex;
			float4 posProj:TEXCOORD0;
		};

		void vert (inout appdata_full v, out Input o){
			UNITY_INITIALIZE_OUTPUT (Input, o);
			o.posProj = mul (unity_Projector, v.vertex);
		}

		void surf (Input IN, inout SurfaceOutput o) {
			float2 uvProj = IN.posProj/IN.posProj.w;

			float4 projCol = (tex2D (_MainTex, uvProj) * _Color) * _Falloff;

			o.Emission = projCol.rgb;
			o.Alpha = (projCol.a/10);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
