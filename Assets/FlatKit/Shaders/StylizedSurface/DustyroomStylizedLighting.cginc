#ifndef DUSTYROOM_STYLIZED_LIGHTING_INCLUDED
#define DUSTYROOM_STYLIZED_LIGHTING_INCLUDED

#include "UnityPBSLighting.cginc"

half _SelfShadingSize;
half _ShadowEdgeSize;
half _LightContribution;
half _Flatness;

half _SelfShadingSizeExtra;
half _ShadowEdgeSizeExtra;
half _LightContributionExtra;
half _FlatnessExtra;

half _UnityShadowPower;
half _UnityShadowSharpness;
half3 _UnityShadowColor;

#ifdef _CELPRIMARYMODE_SINGLE
fixed4 _ColorDim;
#endif  // _CELPRIMARYMODE_SINGLE

#ifdef _CELPRIMARYMODE_STEPS
fixed4 _ColorDimSteps;
sampler2D _CelStepTexture;
#endif  // _CELPRIMARYMODE_STEPS

#ifdef _CELPRIMARYMODE_CURVE
fixed4 _ColorDimCurve;
sampler2D _CelCurveTexture;
#endif  // _CELPRIMARYMODE_CURVE

#ifdef DR_CEL_EXTRA_ON
fixed4 _ColorDimExtra;
#endif  // DR_CEL_EXTRA_ON

#ifdef DR_SPECULAR_ON
half4 _FlatSpecularColor;
float _FlatSpecularSize;
float _FlatSpecularEdgeSmoothness;
#endif  // DR_SPECULAR_ON

#ifdef DR_RIM_ON
half4 _FlatRimColor;
float _FlatRimSize;
float _FlatRimEdgeSmoothness;
float _FlatRimLightAlign;
#endif  // DR_RIM_ON

#ifdef DR_GRADIENT_ON
half4 _ColorGradient;
half _GradientCenterX;
half _GradientCenterY;
half _GradientSize;
half _GradientAngle;
#endif  // DR_GRADIENT_ON

#ifdef VERTEX_WIND_ON
half _AmountWind;
sampler2D _MaskWind;
half _SpeedWind;
half4 _DirectionWind;
#endif  // VERTEX_WIND_ON

half _TextureImpact;
sampler2D _MainTex;

half _Glossiness;
half _Metallic;
fixed4 _Color;

struct InputObject
{
    float2 uv_MainTex;
    float3 viewDir;
    float3 lightDir;
    float3 worldPos;
    float3 worldNormal;
    
    #ifdef DR_VERTEX_COLORS_ON
    float4 color: Color;  // Vertex color
    #endif  // DR_VERTEX_COLORS_ON
};

inline half NdotLTransition(half3 normal, half3 lightDir, half selfShadingSize, half shadowEdgeSize, half flatness) {
    half NdotL = dot(normal, lightDir);
    half angleDiff = saturate((NdotL * 0.5 + 0.5) - selfShadingSize);
    half angleDiffTransition = smoothstep(0, shadowEdgeSize, angleDiff); 
    return lerp(angleDiff, angleDiffTransition, flatness);
}

inline half NdotLTransitionPrimary(half3 normal, half3 lightDir) { 
    return NdotLTransition(normal, lightDir, _SelfShadingSize, _ShadowEdgeSize, _Flatness);
}

inline half NdotLTransitionExtra(half3 normal, half3 lightDir) { 
    return NdotLTransition(normal, lightDir, _SelfShadingSizeExtra, _ShadowEdgeSizeExtra, _FlatnessExtra);
}

inline half NdotLTransitionTexture(half3 normal, half3 lightDir, sampler2D stepTex) {
    half NdotL = dot(normal, lightDir);
    half angleDiff = saturate((NdotL * 0.5 + 0.5) - _SelfShadingSize * 0.0);
    half angleDiffTransition = tex2D(stepTex, half2(angleDiff, 0.5)).r; 
    return angleDiffTransition;//lerp(angleDiff, angleDiffTransition, _Flatness);
}

inline half4 LightingCore(half3 albedo, half3 lightDir, half atten) {
    half4 c;
    half3 light = lerp(half3(1, 1, 1), _LightColor0.rgb, _LightContribution);
    half attenFinal = saturate(atten * _UnityShadowSharpness);
    
    half unityShadowPower = 1;  // Corresponds to _UNITYSHADOWMODE_NONE.
#ifdef _UNITYSHADOWMODE_MULTIPLY
    unityShadowPower = lerp(1, attenFinal, _UnityShadowPower);
#endif
    
    c.rgb = albedo * light * unityShadowPower;
    
#ifdef _UNITYSHADOWMODE_COLOR
    c.rgb = lerp(_UnityShadowColor, c.rgb, attenFinal);
#endif

    return c;
}

inline half4 LightingDustyroomStylized(SurfaceOutputStandard s, half3 lightDir, half atten) {
    half4 c;
    c.rgb = LightingCore(s.Albedo, lightDir, atten);
    c.a = s.Alpha;
    return c;
}

void vertObject(inout appdata_full v, out InputObject o) {
    UNITY_INITIALIZE_OUTPUT(InputObject, o);
    o.lightDir = WorldSpaceLightDir(v.vertex);
    
    #ifdef VERTEX_WIND_ON
        float3 _Blend = abs (mul(unity_ObjectToWorld, float4 (v.normal, 0.0)).xyz);
		float4 _VertexGlobal = mul(unity_ObjectToWorld,v.vertex);

		fixed4 Mx = tex2Dlod (_MaskWind, float4(_VertexGlobal.yz/12 + _Time.x * _SpeedWind,0.0,0.0));
		fixed4 My = tex2Dlod (_MaskWind, float4(_VertexGlobal.zx/12 + _Time.x * _SpeedWind,0.0,0.0));
		fixed4 Mz = tex2Dlod (_MaskWind, float4(_VertexGlobal.xy/12 + _Time.x * _SpeedWind,0.0,0.0));

		float mask = Mx * _Blend.x + My * _Blend.y + Mz * _Blend.z;

		_VertexGlobal.xyz += (cos (_Time.y) * cos(_Time.z) * _DirectionWind.xyz + _DirectionWind.xyz) * _AmountWind * v.color.r * mask;
		v.vertex = mul(unity_WorldToObject,_VertexGlobal);
    #endif  // VERTEX_WIND_ON
}

inline half4 SurfaceCore(half3 worldNormal, half3 worldPos, half3 lightDir, half3 viewDir) {
    fixed4 c = _Color;
    
    #ifdef _CELPRIMARYMODE_SINGLE
        half NdotLTPrimary = NdotLTransitionPrimary(worldNormal, lightDir);
        c = lerp(_ColorDim, c, NdotLTPrimary);
    #endif  // _CELPRIMARYMODE_SINGLE
    
    #ifdef _CELPRIMARYMODE_STEPS
        half NdotLTSteps = NdotLTransitionTexture(worldNormal, lightDir, _CelStepTexture);
        c = lerp(_ColorDimSteps, c, NdotLTSteps);
    #endif  // _CELPRIMARYMODE_STEPS
    
    #ifdef _CELPRIMARYMODE_CURVE
        half NdotLTCurve = NdotLTransitionTexture(worldNormal, lightDir, _CelCurveTexture);
        c = lerp(_ColorDimCurve, c, NdotLTCurve);
    #endif  // _CELPRIMARYMODE_CURVE
    
    #ifdef DR_CEL_EXTRA_ON
        half NdotLTExtra = NdotLTransitionExtra(worldNormal, lightDir);
        c = lerp(_ColorDimExtra, c, NdotLTExtra);
    #endif  // DR_CEL_EXTRA_ON
    
    #ifdef DR_GRADIENT_ON
        float angleRadians = _GradientAngle / 180.0 * 3.14159265359;
        float posGradRotated = (worldPos.x - _GradientCenterX) * sin(angleRadians) + 
                               (worldPos.y - _GradientCenterY) * cos(angleRadians);
        float gradientTop = _GradientCenterY + _GradientSize * 0.5;
        half gradientFactor = saturate((gradientTop - posGradRotated) / _GradientSize);
        c = lerp(c, _ColorGradient, gradientFactor);
    #endif  // DR_GRADIENT_ON

    #ifdef DR_RIM_ON
        float4 rim = 1.0 - dot(viewDir, worldNormal);
        half NdotL = dot(worldNormal, lightDir);
        float rimSpread = 1.0 - _FlatRimSize - NdotL * _FlatRimLightAlign;
        float rimTransition = smoothstep(rimSpread - _FlatRimEdgeSmoothness * 0.5, rimSpread + _FlatRimEdgeSmoothness * 0.5, rim);
        c = lerp(c, _FlatRimColor, rimTransition);
    #endif  // DR_RIM_ON

    #ifdef DR_SPECULAR_ON
        float3 halfVector = normalize(_WorldSpaceLightPos0 + viewDir);
        float NdotH = dot(worldNormal, halfVector) * 0.5 + 0.5;
        float specular = saturate(pow(NdotH, 100.0 * (1.0 - _FlatSpecularSize) * (1.0 - _FlatSpecularSize)));
        float specularTransition = smoothstep(0.5 - _FlatSpecularEdgeSmoothness * 0.1, 0.5 + _FlatSpecularEdgeSmoothness * 0.1, specular);
        c = lerp(c, _FlatSpecularColor, specularTransition);
    #endif  // DR_SPECULAR_ON
    
    return c;
}
    
void surfObject(InputObject IN, inout SurfaceOutputStandard o) {
    half4 c = SurfaceCore(IN.worldNormal, IN.worldPos, IN.lightDir, IN.viewDir);
    
    // TODO: Add an option for additive texture blending.
    c *= lerp(half4(1.0, 1.0, 1.0, 1.0), tex2D(_MainTex, IN.uv_MainTex), _TextureImpact);
    
    #ifdef DR_VERTEX_COLORS_ON
        c *= IN.color;
    #endif  // DR_VERTEX_COLORS_ON
    
    o.Albedo = c.rgb;
    o.Alpha = c.a;
    
    o.Metallic = _Metallic;
    o.Smoothness = _Glossiness;
}

#endif // DUSTYROOM_STYLIZED_LIGHTING_INCLUDED
