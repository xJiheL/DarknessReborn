#include "UnityCG.cginc"

// Custom Unpack Normal (same as Unity with normal intensity)
fixed3 Unpack(float4 packednormal, float strenght)
{
	#if defined(SHADER_API_GLES) && defined(SHADER_API_MOBILE)
		return packednormal.xyz * 2 - 1;
	#else
		fixed3 normal;
		normal.xy = packednormal.wy * 2 - 1;
		normal.z = sqrt(1 - normal.x*normal.x - normal.y * normal.y) * strenght;
		return normalize(normal);
	#endif
}

// Parallax Functions
float2 Parallax(sampler2D _MainTex, float2 uv, float2 viewDir, float scale, float bias)
{
	float h = tex2D(_MainTex, uv).b;
	float hsb = h * scale + bias;

	return uv + viewDir * hsb;
}

float2 ParallaxClassic(float2 uv, float2 viewDir, float scale, float bias, float h)
{
	float hsb = h * scale + bias;

	return uv + viewDir * hsb;
}

// Convert cartesian coordinates to radial coordinates
float2 CartesianToRadial(float2 uv)
{
	return float2(atan2(uv.y, uv.x), length(uv));
}

// Dithering for fake transparency with c.a transparency. 
float Dithering (float4 c, sampler2D paletteTex, float2 uv, half colorCount, half paletteHeight)
{
	// fmod (half x, half y) return the remainder of x/y with the sign of x
	float rowOffset = floor ((fmod (uv.x, 2) * 0.249 + fmod (uv.x + uv.y, 2) * 0.499) * colorCount) * 16;

	float2 paletteUV = float2 (clamp (floor (c.r * 16), 0, 15) / 16 + clamp (c.b * 16, 0.5, 15.5) / 256, (clamp (c.g * 16, 0.5, 15.5) + rowOffset) / paletteHeight);

	float m = tex2D (paletteTex, paletteUV).r * c.a;

	return m;
}

float3 GetNoise (sampler2D _NoiseTex, float2 uv)
{
	float3 noise = tex2D (_NoiseTex, uv * 100);
	noise = noise * 2.0 - 0.5;

	return noise/255;
}

float3 GetBlend(float4 _Texture1, float a1, float4 _Texture2, float a2)
{
    float depth = 0.2;
    float ma = max(_Texture1.a + a1, _Texture2.a + a2) - depth;

    float b1 = max(_Texture1.a + a1 - ma, 0);
    float b2 = max(_Texture2.a + a2 - ma, 0);

    return (_Texture1.rgb * b1 + _Texture2.rgb * b2) / (b1 + b2);
}

// Overlay blend mode
float Overlay(float base, float top)
{
     if (base < 0.5){
          return 2 * base*top;
     }
     else {
          return 1 - 2 * (1 - base) *(1 - top);
     }
}

// Function for hue/saturation from photoshop
// Blend2
float3 Blend2 (float3 col, float3 c, float pos)
{
	float3 final;
	final.r = col.r * (1 - pos) + c.r * pos;
	final.g = col.g * (1 - pos) + c.g * pos;
	final.b = col.b * (1 - pos) + c.b * pos;
	return final;
}

// Blend3
float3 Blend3 (float3 col1, float3 originalCol, float3 col2, float pos)
{
	float3 final;
	if (pos < 0)
	{
		final = Blend2 (col1, originalCol, pos + 1);
	}
	else if (pos > 0)
	{
		final = Blend2 (originalCol, col2, pos);
	}
	else
	{
	 	final = originalCol;
	}

	return final;
		
}

// Inverse Matrix 4x4
//float4x4 Inverse (float4x4 inputMatrix)
//{
//	#define minor (x, y, z) 
////	determinant (float3x3 (inputMatrix.x, inputMatrix.y, inputMatrix.z))
//
//	float4x4 coFactors = float4x4 (
//		 minor (_22_23_24, _32_33_34, _42_43_44),
//		-minor (_21_23_24, _31_33_34, _41_43_44),
//		 minor (_21_22_24, _31_32_34, _41_42_44),
//		-minor (_21_22_23, _31_32_33, _41_42_43),
//
//		-minor (_12_13_14, _32_33_34, _42_43_44),
//		 minor (_11_13_14, _31_33_34, _41_43_44),
//		-minor (_11_12_14, _31_32_34, _41_42_44),
//		 minor (_11_12_13, _31_32_33, _41_42_43),
//
//		 minor (_12_13_14, _22_23_24, _42_43_44),
//		-minor (_11_13_14, _21_23_24, _41_43_44),
//		 minor (_11_12_14, _21_22_24, _41_42_44),
//		-minor (_11_12_13, _21_22_23, _41_42_43),
//
//		-minor (_12_13_14, _22_23_24, _32_33_34),
//		 minor (_11_13_14, _21_23_24, _31_33_34),
//		-minor (_11_12_14, _21_22_24, _31_32_34),
//		 minor (_11_12_13, _21_22_23, _31_32_33),
//	);
//
//	#undef minor
//	return transpose (coFactors)/determinant(inputMatrix);
//}
