
float4x4 WorldViewProjection;
float Time;
texture NoiseTexture;

sampler2D NoiseSampler = sampler_state
{
    Texture = <NoiseTexture>;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    // Transform the position
    output.Position = mul(input.Position, WorldViewProjection);
    output.TexCoord = input.TexCoord;
    output.Color = input.Color;
    
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // Create flowing perlin noise effect
    float2 uv = input.TexCoord;
    
    // Offset the texture coords over time for animation
    float2 noiseOffset1 = float2(Time * 0.02, Time * 0.03);
    float2 noiseOffset2 = float2(Time * -0.02, Time * 0.01);
    
    // Sample noise texture with different offsets
    float4 noise1 = tex2D(NoiseSampler, uv + noiseOffset1);
    float4 noise2 = tex2D(NoiseSampler, uv * 2.0 + noiseOffset2);
    
    // Blend the noise samples
    float noiseValue = (noise1.r * 0.6 + noise2.r * 0.4);
    
    // Apply some color gradients based on the noise
    float4 gradientColor = lerp(
        float4(0.7, 0.8, 1.0, 1.0),  // Light blue
        float4(0.4, 0.5, 0.9, 1.0),  // Darker blue
        noiseValue
    );
    
    // Get the original color (cloud silhouette)
    float4 originalColor = input.Color;
    
    // Only apply the effect within the cloud silhouette (non-transparent pixels)
    float4 finalColor = originalColor.a > 0.1 ? gradientColor : originalColor;
    
    // Keep the original alpha
    finalColor.a = originalColor.a;
    
    return finalColor;
}

technique PerlinNoiseTechnique
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}