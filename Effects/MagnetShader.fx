sampler uImage0 : register(s0);
float4 uSourceRect;
float2 uScreenResolution;
float uTime;
float2 uPlayerPosition;
float uFieldRadius;
float uPulseSpeed;
float uPulseIntensity;
float uBaseOpacity;

struct VSInput
{
    float2 Coord : TEXCOORD0;
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
};

float4 PixelShaderFunction(VSInput input) : COLOR0
{
    float4 color = tex2D(uImage0, input.Coord);
    float2 screenPos = input.Coord * uScreenResolution;
    
    // Distance from player center
    float dist = length(screenPos - uPlayerPosition);
    
    // Early exit if outside field radius
    if (dist > uFieldRadius)
        return color;
    
    // Normalized distance (0 at center, 1 at edge)
    float normalizedDist = dist / uFieldRadius;
    
    // Simple radial pulse (reduced complexity)
    float pulsePhase = uTime * uPulseSpeed;
    float pulse = sin(normalizedDist * 8.0 - pulsePhase * 2.0) * 0.5 + 0.5;
    
    // Distance falloff (using saturate instead of pow to avoid negative issues)
    float distanceFade = saturate(1.0 - normalizedDist);
    distanceFade = distanceFade * distanceFade; // Square for falloff instead of pow
    
    // Edge definition
    float edgeSharpness = saturate((1.0 - normalizedDist) * 5.0);
    
    // Final effect strength
    float effectStrength = pulse * distanceFade * edgeSharpness;
    effectStrength = effectStrength * uPulseIntensity + uBaseOpacity;
    
    // Simple blue force field color
    float3 fieldColor = float3(0.2, 0.6, 1.0);
    
    // Apply the effect
    color.rgb = lerp(color.rgb, color.rgb + fieldColor, effectStrength * 0.5);
    
    return color;
}

technique Technique1
{
    pass ForceFieldPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}