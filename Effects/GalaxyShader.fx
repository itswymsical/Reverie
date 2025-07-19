// Multiple texture samplers for trail effect
sampler uImage0 : register(s0); // Main spiral texture
sampler uImage1 : register(s1); // Trail texture 1
sampler uImage2 : register(s2); // Trail texture 2  
sampler uImage3 : register(s3); // Trail texture 3

float4 uSourceRect;
float2 uScreenResolution;
float uTime;
float uIntensity;

struct VSInput
{
    float2 Coord : TEXCOORD0;
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
};

float4 PixelShaderFunction(VSInput input) : COLOR0
{
    float2 uv = (input.Coord - 0.5) * 2.0;
    float dist = length(uv);
    float angle = atan2(uv.y, uv.x);
    
    // Simplified spiral coordinates
    float2 texCoord = float2(frac(angle * 0.159 + uTime * 0.1), frac(dist * 0.8));
    
    // Sample textures with time offset
    float3 current = tex2D(uImage0, texCoord).rgb;
    float3 trail = tex2D(uImage1, frac(texCoord - uTime * 0.03)).rgb * 0.6;
    
    // Simple spiral pattern
    float spiral = sin(angle * 3.0 - dist * 6.0 + uTime * 2.0) * 0.5 + 0.5;
    
    // Combine with fade
    float fade = 1.0 - dist * dist;
    float3 spiralColor = (current + trail) * spiral * fade;
    
    // Central bloom and purple/indigo coloring
    float bloom = 1.0 / (1.0 + dist * 6.0);
    float3 final = spiralColor * float3(0.6, 0.3, 1.0) + bloom * float3(0.8, 0.4, 1.0);
    
    return float4(final * uIntensity, spiral * fade + bloom);
}

technique Technique1
{
    pass GalaxyPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}