sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float4 uSourceRect;
float2 uScreenResolution;
float uTime;
float uIntensity;
float2 uCenter;
float uScale;
float uRotation;
float uArmCount;
float4 uColor;

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
    float angle = atan2(uv.y, uv.x) + uRotation + uTime * 0.2;
    
    // Galaxy spiral arms - make more pronounced
    float armAngle = angle * (uArmCount > 0 ? uArmCount : 3.0);
    float spiral = sin(armAngle - dist * 8.0 + uTime * 1.5) * 0.5 + 0.5;
    spiral = pow(spiral, 1.5); // sharpen the spiral arms
    
    // Enhanced texture coordinates for better visibility
    float2 armTexCoord = float2(frac(angle * 0.159 + uTime * 0.08), frac(dist * 0.6));
    float2 nebulaTexCoord = float2(frac(angle * 0.234 - uTime * 0.05), frac(dist * 0.4 + uTime * 0.03));
    
    // Sample both textures
    float3 armTexture = tex2D(uImage0, armTexCoord).rgb;
    float3 nebulaTexture = tex2D(uImage1, nebulaTexCoord).rgb;
    
    // Distance-based effects
    float centralGlow = 1.0 / (1.0 + dist * dist * 6.0);
    float armGlow = 1.0 / (1.0 + dist * 3.0);
    float edgeFade = 1.0 - dist;
    edgeFade = max(edgeFade, 0.0);
    
    // Base galaxy colors
    float3 innerColor = float3(1.0, 0.9, 0.4);
    float3 outerColor = float3(0.4, 0.2, 1.0);
    float3 galaxyColor = lerp(innerColor, outerColor, dist);
    
    // Strong texture application to spiral arms
    float spiralMask = spiral * armGlow * 2.0; // amplify the spiral arms
    float3 armColor = armTexture * spiralMask;
    
    // Nebula texture in the gaps between arms
    float nebulaMask = (1.0 - spiral * 0.8) * armGlow * 0.8;
    float3 nebulaColor = nebulaTexture * nebulaMask;
    
    // Combine all elements with stronger texture influence
    float3 finalColor = galaxyColor * 0.3 + armColor * 1.5 + nebulaColor * 0.7;
    finalColor += centralGlow * innerColor * 0.5;
    
    // Apply color tinting
    finalColor *= uColor.rgb;
    
    float brightness = (spiralMask + nebulaMask + centralGlow) * edgeFade;
    float alpha = brightness * uColor.a;
    
    return float4(finalColor * uIntensity, alpha);
}

technique Technique1
{
    pass GalaxyPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}