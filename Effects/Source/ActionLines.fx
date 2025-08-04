float time;
float2 center;
float intensity;
float speed;
float lineCount;
float lineWidth;
float fadeDistance;
float2 resolution;
matrix transformMatrix;

texture sampleTexture;
sampler2D samplerTex = sampler_state
{
    texture = <sampleTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

struct VertexShaderInput
{
    float4 Position : POSITION;
    float2 TexCoords : TEXCOORD0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION;
    float2 TexCoords : TEXCOORD0;
    float4 Color : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    output.Color = input.Color;
    output.TexCoords = input.TexCoords;
    output.Position = mul(input.Position, transformMatrix);
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 originalColor = tex2D(samplerTex, input.TexCoords);
    float2 uv = input.TexCoords;
    
    // Distance and angle from center
    float2 direction = uv - center;
    float distance = length(direction);
    float angle = atan2(direction.y, direction.x);
    
    // PS 3.0 dynamic branching - early exit for distant pixels
    if (distance > fadeDistance)
    {
        return originalColor;
    }
    
    // Normalize angle to 0-1 range
    float normalizedAngle = (angle + 3.14159) / (2.0 * 3.14159);
    
    // Create radial segments
    float segmentIndex = floor(normalizedAngle * lineCount);
    float segmentProgress = frac(normalizedAngle * lineCount);
    
    // Main line pattern
    float linePattern = abs(segmentProgress - 0.5) * 2.0;
    linePattern = 1.0 - smoothstep(0.0, lineWidth, linePattern);
    
    // Add randomness per line using segment index
    float randomOffset = sin(segmentIndex * 7.13 + time * speed) * 0.3;
    float randomIntensity = 0.5 + 0.5 * sin(segmentIndex * 3.7 + time * speed * 0.7);
    
    // Distance fade
    float distanceFade = 1.0 - smoothstep(0.1, fadeDistance, distance);
    
    // Animate lines moving outward
    float animatedDistance = distance - (time * speed * 0.1);
    float pulsePattern = sin(animatedDistance * 15.0 + segmentIndex) * 0.5 + 0.5;
    
    // Combine all factors
    float lineStrength = linePattern * distanceFade * randomIntensity * pulsePattern;
    
    // Angle variation for organic feel
    float angleVariation = sin(normalizedAngle * 4.0 + time) * 0.2 + 0.8;
    lineStrength *= angleVariation;
    
    // Speed line color
    float4 lineColor = float4(1.0, 0.95, 0.9, 1.0);
    
    // Blend with original
    float finalIntensity = lineStrength * intensity;
    float4 result = lerp(originalColor, lineColor, finalIntensity);
    
    // Boost contrast near center
    if (distance < 0.3)
    {
        result.rgb = pow(result.rgb, 0.8);
    }
    
    return result * input.Color;
}

technique Technique1
{
    pass SpeedLinesPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}