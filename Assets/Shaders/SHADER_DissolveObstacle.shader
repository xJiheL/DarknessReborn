// basic and fast version
// TODO : need improvment to share the dissolve mask
Shader "DarknessReborn/DissolveObstacle" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}

		[Header (Dissolve)]
		_DissolveTex ("Mask Dissolve", 2D) = "white" {}
		_Cutout ("Cutout",  Range(0,1)) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		Cull Front
		CGPROGRAM
		#pragma surface surf Standard vertex:vert alpha:fade addshadow
		#pragma target 4.0

		struct Input {
			float2 uv_MainTex;
			float viewDist;
			float4 screenPos;
		};
		
		sampler2D _DissolveTex;
		
		void vert (inout appdata_full v, out Input o)
		{
		    UNITY_INITIALIZE_OUTPUT (Input, o);
		    half3 viewDirW = WorldSpaceViewDir (v.vertex);
		    o.viewDist = length (viewDirW);
		}
		
		void surf (Input IN, inout SurfaceOutputStandard o) {
			half2 uv = IN.screenPos.xy / IN.screenPos.w;
			fixed m = 1 - saturate (pow(tex2D(_DissolveTex, uv).r,1));
			
			o.Emission = float3 (0,0,0);
            o.Alpha = saturate (1 - (1 - (IN.viewDist - 7)) * m);
//            clip((IN.viewDist - 8) * m);
		}
		ENDCG
		
		Tags { "RenderType"="Transparent" "Queue"="Transparent"}
		LOD 200

        Cull Back
        
		CGPROGRAM
		#pragma surface surf Cartoon vertex:vert alpha:fade 
		#pragma target 4.0

		struct Input {
			float2 uv_MainTex;
			float4 screenPos;
			float viewDist;
			float3 viewDir;
		};
		
		struct CustomSurfaceOutput
        {
            half3 Albedo;
            half Alpha;
            half3 Normal;
            half3 Emission;
            half Smoothness;
            half RimLight;
        };

		sampler2D _MainTex;
		sampler2D _DissolveTex;
		
		half _Cutout;

		fixed4 _Color;
        
        half _IntensityEmiss;
        fixed4 _ColorE;

        fixed4 _TintSpec;
        sampler2D _SmoothTex;
        half _Smoothness;
        half _MinSpec;
        half _MaxSpec;       
         
        fixed4 _RimLightColor;
        fixed4 _RimShadowColor;
        half _RangeTransition;
        half _RimPower;
        half _RimHardness;
        
        fixed4 _TintShadow;
        half _MinShadow;
        half _MaxShadow;  
		
        half4 LightingCartoon (CustomSurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
        {
            s.Normal = normalize (s.Normal);           
            
            half3 halfVector = normalize (lightDir + viewDir); 
            half nDotl = max (dot(s.Normal, lightDir), 0);
            half nDoth = max (dot(s.Normal, halfVector), 0);
            
            // Shadow
            half shadow = saturate (1 - smoothstep (_MinShadow, _MaxShadow, nDotl));            

            // Spec            
            half spec =  smoothstep (_MinSpec, _MaxSpec, pow (nDoth, s.Smoothness * s.Smoothness)) * (1- shadow);
            
            // Rim
            nDoth = saturate (nDoth - _RangeTransition);
            half3 rimLight = _RimLightColor.rgb * s.RimLight * _LightColor0;
            rimLight *= nDoth;
            half3 rimShadow = _RimShadowColor.rgb * s.RimLight * _LightColor0;
            rimShadow *= (1 - nDoth);
            
            half4 c;

            c.rgb = saturate (s.Albedo * _LightColor0.rgb);
            c.rgb = lerp (c.rgb, _TintShadow.rgb * _LightColor0.rgb, shadow * _TintShadow.a);
            c.rgb = lerp (c.rgb, _TintSpec.rgb * _LightColor0.rgb, spec * _TintSpec.a);
            c.rgb += rimLight;           
            c.rgb += rimShadow;
//            c.rgb *= atten;
            c.rgb += s.Emission;
              
            c.a = s.Alpha;
            
            return c;
        }
		
		void vert (inout appdata_full v, out Input o)
		{
		    UNITY_INITIALIZE_OUTPUT (Input, o);
		    half3 viewDirW = WorldSpaceViewDir (v.vertex);
		    o.viewDist = length (viewDirW);
		}

		void surf (Input IN, inout CustomSurfaceOutput o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

			half2 uv = (((IN.screenPos.xy / IN.screenPos.w) - .5) * .5)+.5;
			fixed m = 1 - saturate ((tex2D(_DissolveTex, uv).r - .1) * 2);
            
            half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
            
            o.Albedo = c.rgb;
            o.RimLight = saturate (pow (rim, _RimPower) * _RimHardness);
            o.Emission = _ColorE.rgb * _IntensityEmiss; 
            o.Smoothness = tex2D (_SmoothTex, IN.uv_MainTex) * _Smoothness * 32;
			o.Alpha = saturate (1 - (1 - (IN.viewDist - 8)) * m);
//			clip((IN.viewDist - 11) * m);
		}
		ENDCG
	}
	FallBack "Transparent/VertexLit"
}
