matrix uWorldViewProjection;

texture sampleTexture;
sampler2D trailSampler = sampler_state
{
    texture = <sampleTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = clamp;
    AddressV = clamp;
};

texture noiseTexture;
sampler2D noiseSampler = sampler_state
{
    texture = <noiseTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

// Advanced trail parameters
float time;
float trailIntensity = 2.5;
float edgeSharpness = 4.0;
float glowStrength = 1.8;
float noiseScale = 3.0;
float noiseSpeed = 1.5;
float fadeDistance = 0.15;
float coreBrightness = 2.0;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;
    output.Position = mul(input.Position, uWorldViewProjection);
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    return output;
}

float invlerp(float from, float to, float value)
{
    return saturate((value - from) / (to - from));
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 coords = input.TextureCoordinates;
    float4 baseColor = input.Color;
    
    // Sample main trail texture with animation (like LightningTrail)
    float2 animatedCoords = float2(coords.x + time * noiseSpeed, coords.y);
    float4 trailTex = tex2D(trailSampler, animatedCoords);
    
    // Sample noise for energy distortion
    float2 noiseCoords = float2(coords.x * noiseScale - time * noiseSpeed, coords.y * noiseScale);
    float noise = tex2D(noiseSampler, noiseCoords).r;
    
    // Create sophisticated edge falloff with multiple layers
    float centerDistance = abs(coords.y - 0.5) * 2.0;
    float edgeFade = 1.0 - pow(centerDistance, edgeSharpness);
    
    // Fixed trail completion fade - ensure it goes to 0 at the end
    float trailFade = coords.x;
    if (coords.x < fadeDistance)
        trailFade = smoothstep(0.0, fadeDistance, coords.x);
    
    trailFade = pow(trailFade, trailIntensity);
    
    // Early exit if we're at the trail end to prevent black artifacts
    if (trailFade < 0.01)
        return float4(0, 0, 0, 0);
    
    // Create glowing core effect
    float coreIntensity = 1.0 - pow(centerDistance, 2.0);
    float4 coreGlow = baseColor * coreIntensity * coreBrightness;
    
    // Combine noise with trail for energy effect
    float energyMask = trailTex.r * (0.7 + noise * 0.3);
    
    // Build final color with layered effects
    float4 finalColor = baseColor * trailTex;
    
    // Add glowing core
    finalColor += coreGlow * energyMask * glowStrength;
    
    // Add subtle noise-based highlights
    finalColor.rgb += noise * baseColor.rgb * 0.4 * energyMask;
    
    // Apply sophisticated opacity - ensure alpha never goes negative
    float finalOpacity = saturate(edgeFade * trailFade * energyMask);
    finalColor.a = finalOpacity;
    
    return finalColor;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 MainPS();
    }
}