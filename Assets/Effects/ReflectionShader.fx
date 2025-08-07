sampler uImage0 : register(s0);

float4 uSourceRect;
float2 uScreenResolution;
float uTime;
float uIntensity;
float4 uColor;
float uReflectionHeight; // where the reflection starts (0.5 = middle, 0.7 = lower)
float uWaveStrength; // distortion strength
float uFadeStrength; // how much the reflection fades

struct VSInput
{
    float2 Coord : TEXCOORD0;
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
};

float4 PixelShaderFunction(VSInput input) : COLOR0
{
    float2 uv = input.Coord;
    
    // Check if we're in the reflection area
    if (uv.y > uReflectionHeight)
    {
        // Calculate reflection coordinates
        float reflectionY = uReflectionHeight - (uv.y - uReflectionHeight);
        float2 reflectionUV = float2(uv.x, reflectionY);
        
        // Add wave distortion
        float wave1 = sin(uv.x * 12.0 + uTime * 2.0) * 0.01;
        float wave2 = sin(uv.x * 8.0 - uTime * 1.5) * 0.005;
        float wave3 = sin(uv.x * 20.0 + uTime * 3.0) * 0.003;
        float totalWave = (wave1 + wave2 + wave3) * uWaveStrength;
        
        reflectionUV.x += totalWave;
        
        // Sample the reflected image
        float4 reflectedColor = tex2D(uImage0, reflectionUV);
        
        // Calculate fade based on distance from reflection line
        float fadeDistance = (uv.y - uReflectionHeight) / (1.0 - uReflectionHeight);
        float fade = 1.0 - pow(fadeDistance, uFadeStrength);
        
        // Apply water tint and transparency
        reflectedColor.rgb *= uColor.rgb;
        reflectedColor.a *= fade * uIntensity * uColor.a;
        
        return reflectedColor;
    }
    else
    {
        // Normal rendering for top half
        return tex2D(uImage0, uv);
    }
}

technique Technique1
{
    pass ReflectionPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}