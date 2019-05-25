Shader "Custom/Shader_Waterfall_Foam" {
	Properties {
		_IntensityEmiss ("Intensity Emissive", Range(0,1)) = 0.0

		[Header (Foam)]
		_ColorFoam ("Color Foam", Color) = (1,1,1,1)
		_NoiseFoam ("Noise Foam", 2D) = "white" {}
		_SpeedNoise ("Speed Noise", vector) = (0,0,0,0)
		_HardnessVC ("Hardness Vertex Color", Range(0,10)) = 0.0
		_Contrast ("Contrast", Range(0,10)) = 0.0
		_Push ("Push Vertex", Range(0,1)) = 0.0
		_Cutout ("Cutout", Range(0,1)) = 0.0
		_TileX ("Tile X", float) = 1
		_TileY ("Tile Y", float) = 1
		_RangeFoam ("RangeFoam", float) = 1
	}
	SubShader {
		Tags { "RenderType"="Cutout" }
		LOD 200

		Cull Off

		CGPROGRAM
		#pragma surface surf Standard vertex:vert addshadow
		#pragma target 3.0

		sampler2D _NoiseFoam;

		half _Metallic;
		half _HardnessVC;
		half _Contrast;
		half _Push;
		half _Cutout;
		float _IntensityEmiss;
		float  _TileX;
		float  _TileY;
		float  _RangeFoam;

		fixed4 _ColorFoam;
		fixed4 _SpeedNoise;

		half3 Contrast (half3 color, half contrast)
		{
			float3 newColor = saturate (lerp (half3 (0.3, 0.5, 0.5), color, contrast));
			newColor = GammaToLinearSpace (newColor);
			return newColor;
		}

		struct Input {
			float2 uv_NoiseFoam;
			float4 color:COLOR;
		};

		void vert (inout appdata_full v)
		{
			fixed2 uvNoise = float2 (v.texcoord.x * _TileX + _Time.x * _SpeedNoise.x, v.texcoord.y * _TileY + _Time.x * _SpeedNoise.y);
			fixed noise = saturate (Contrast ((saturate ((1 - v.color.r)-_RangeFoam) * _HardnessVC) * tex2Dlod (_NoiseFoam, float4 (uvNoise, 0, 0)).rgb, _Contrast).r);

			v.vertex.xyz += v.normal * noise * _Push;
			v.color.g = noise;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
//			o.Albedo = _ColorFoam.rgb;
			o.Emission = _IntensityEmiss;
//			o.Emission = IN.color.g;
			o.Metallic = _Metallic;
			o.Smoothness = 0.8;
			clip ((1 - IN.color.g) * IN.color.r - _Cutout);
//			clip (0 - _Cutout);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
