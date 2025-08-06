sampler uImage0 : register(s0);

float4 uSourceRect;
float2 uScreenResolution;
float uTime;
float uIntensity;
float2 uCenter; // sunburst center position (-1 to 1 range)
float uScale; // sunburst scale multiplier
float uRayCount; // number of rays (default: 12)

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
    float angle = atan2(uv.y, uv.x) + uTime * 0.3;
    
    // Simple ray pattern
    float rays = abs(sin(angle * uRayCount * 0.5));
    rays = rays * rays; // square for sharpening
    
    // Combined falloff and glow
    float falloff = 1.0 / (1.0 + dist * 3.0);
    float centerGlow = 1.0 / (1.0 + dist * dist * 6.0);
    
    // Edge fade
    float fade = 1.0 - dist;
    fade = max(fade, 0.0);
    
    float brightness = (rays * falloff + centerGlow) * fade;
    
    // Simple color blend
    float3 color = float3(1.0, 0.7 - dist * 0.3, 0.2 + centerGlow * 0.6);
    
    return float4(color * brightness * uIntensity, brightness);
}

technique Technique1
{
    pass SunburstPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}