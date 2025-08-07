// Magnetic field vertex + pixel shader combo
sampler uImage0 : register(s0);
float4x4 uTransform;
float4 uSourceRect;
float2 uScreenResolution;
float uTime;
float2 uPlayerPosition;
float uFieldRadius;
float uDistortionStrength;
float uWaveFrequency;

struct VSInput
{
    float2 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
    float2 WorldPos : TEXCOORD1;
};

VSOutput VertexShaderFunction(VSInput input)
{
    VSOutput output;
    
    // Convert to world space
    float2 worldPos = input.Position;
    output.WorldPos = worldPos;
    
    // Calculate distance from magnetic center
    float2 toCenter = worldPos - uPlayerPosition;
    float distance = length(toCenter);
    
    // Apply magnetic distortion to vertices within range
    if (distance < uFieldRadius && distance > 0.0)
    {
        float normalizedDist = distance / uFieldRadius;
        
        // Create ripple waves that move outward
        float wavePhase = uTime * 3.0 - distance * 0.02;
        float wave = sin(wavePhase) * cos(wavePhase * 0.5);
        
        // Magnetic attraction/repulsion force
        float forceStrength = (1.0 - normalizedDist) * uDistortionStrength;
        float2 forceDirection = normalize(toCenter);
        
        // Combine wave and magnetic force
        float2 displacement = forceDirection * wave * forceStrength * 5.0;
        
        // Apply spiral distortion (like magnetic field lines)
        float angle = atan2(toCenter.y, toCenter.x);
        float spiral = sin(angle * 4.0 + uTime * 2.0) * forceStrength * 2.0;
        float2 perpendicular = float2(-forceDirection.y, forceDirection.x);
        displacement += perpendicular * spiral;
        
        worldPos += displacement;
    }
    
    // Transform to screen space
    output.Position = mul(float4(worldPos, 0, 1), uTransform);
    output.TexCoord = input.TexCoord;
    output.Color = input.Color;
    
    return output;
}

float4 PixelShaderFunction(VSOutput input) : COLOR0
{
    float4 color = tex2D(uImage0, input.TexCoord) * input.Color;
    
    // Add magnetic field glow based on distance
    float2 toCenter = input.WorldPos - uPlayerPosition;
    float distance = length(toCenter);
    
    if (distance < uFieldRadius)
    {
        float normalizedDist = distance / uFieldRadius;
        float fieldGlow = (1.0 - normalizedDist) * 0.3;
        
        // Magnetic field color
        float3 magneticTint = float3(0.2, 0.6, 1.0);
        color.rgb += magneticTint * fieldGlow;
    }
    
    return color;
}

technique Technique1
{
    pass MagneticDistortion
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}