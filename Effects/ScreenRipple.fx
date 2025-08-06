sampler uImage0 : register(s0);

float2 MousePosition;
float2 ProjectilePosition;
float RippleStrength;
float RippleRadius;
float RippleDistortion;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 mouseToProjectile = ProjectilePosition - MousePosition;
    float distanceToLine = distance(coords, ProjectilePosition);
    float2 direction = normalize(mouseToProjectile);
    float2 perpendicularDirection = float2(-direction.y, direction.x);
    
    float rippleEffect = sin(distanceToLine * RippleDistortion - RippleRadius) * RippleStrength;
    coords += perpendicularDirection * rippleEffect;
    
    return tex2D(uImage0, coords);
}

technique Technique1
{
    pass RipplePass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}