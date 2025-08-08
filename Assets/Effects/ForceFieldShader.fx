sampler uImage0 : register(s0);

float2 uPlayerPosition;
float2 uScreenResolution;
float4 uSourceRect;
float uTime;
float uFieldRadius;
float uRippleFrequency;
float uDistortionStrength;
float uFalloffPower;
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
    // Convert screen coordinates to world position
    float2 screenPos = input.Coord * uScreenResolution;
    float2 playerOffset = screenPos - uPlayerPosition;
    float distance = length(playerOffset);
    
    // Base texture sample
    float4 color = tex2D(uImage0, input.Coord);
    
    if (distance < uFieldRadius)
    {
        float normalizedDist = distance / uFieldRadius;
        
        // Create spherical dome effect using distance
        float sphereEffect = sqrt(1.0 - normalizedDist * normalizedDist);
        
        // Animated ripples
        float ripple = sin(normalizedDist * uRippleFrequency - uTime * uPulseSpeed) * 0.3;
        
        // Pulsing effect
        float pulse = sin(uTime * uPulseSpeed * 2.0) * uPulseIntensity + 1.0;
        
        // Energy lines based on angle
        float angle = atan2(playerOffset.y, playerOffset.x);
        float energyLines = sin(angle * 8.0 + uTime * 2.0) * 0.5 + 0.5;
        energyLines *= sin(normalizedDist * 12.0 - uTime * 3.0) * 0.5 + 0.5;
        
        // Fresnel-like edge effect
        float fresnel = pow(normalizedDist, uFalloffPower);
        
        // Combine effects
        float intensity = (sphereEffect + ripple) * energyLines * pulse * fresnel;
        
        // Magnetic field colors
        float3 fieldColor = lerp(float3(0.0, 0.3, 1.0), float3(0.0, 1.0, 1.0), energyLines);
        
        // Apply the forcefield effect
        float fieldStrength = intensity * (1.0 - normalizedDist * 0.7);
        color.rgb += fieldColor * fieldStrength;
        color.a = max(color.a, fieldStrength * uBaseOpacity);
    }
    else
    {
        // Outside field radius, make transparent
        color.a = 0.0;
    }
    
    return color * input.Color;
}

technique Technique1
{
    pass ForceFieldPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}