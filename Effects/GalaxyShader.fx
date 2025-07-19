// Main spiral texture
sampler uImage0 : register(s0);

float4 uSourceRect;
float2 uScreenResolution;
float uTime;
float uIntensity;
float2 uCenter; // spiral center position (-1 to 1 range)
float uScale; // spiral scale multiplier

struct VSInput
{
    float2 Coord : TEXCOORD0;
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
};

float4 PixelShaderFunction(VSInput input) : COLOR0
{
    float2 uv = ((input.Coord - 0.5) * 2.0 - uCenter) * uScale;
    float dist = length(uv);
    float angle = atan2(uv.y, uv.x);
    
    // Texture sampling with spiral coordinates
    float2 texCoord = float2(frac(angle * 0.159 + uTime * 0.1), frac(dist * 0.8));
    float3 current = tex2D(uImage0, texCoord).rgb;
    
    // Simple spiral pattern
    float spiral = sin(angle * 3.0 - dist * 6.0 + uTime * 2.0) * 0.5 + 0.5;
    float fade = 1.0 - dist * dist;
    
    // Central bloom and combine
    float bloom = 1.0 / (1.0 + dist * 6.0);
    float3 final = current * spiral * fade * float3(0.6, 0.3, 1.0) + bloom * float3(0.8, 0.4, 1.0);
    
    return float4(final * uIntensity, spiral * fade + bloom);
}

technique Technique1
{
    pass GalaxyPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}