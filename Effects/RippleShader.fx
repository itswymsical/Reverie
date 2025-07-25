sampler uImage0 : register(s0);

float2 uCenter;
float uTime;
float uRippleStrength;
float uRippleSpeed;
float uRippleFrequency;
float uRippleDecay;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 centeredCoords = (coords - 0.5) * 2.0;
    
    float distanceFromCenter = length(centeredCoords - uCenter);
    
    float ripplePhase = distanceFromCenter * uRippleFrequency - uTime * uRippleSpeed;
    float rippleEffect = sin(ripplePhase) * uRippleStrength;
    
    rippleEffect *= exp(-distanceFromCenter * uRippleDecay);
    
    float2 radialDirection = normalize(centeredCoords - uCenter);
    float2 perpendicularDirection = float2(-radialDirection.y, radialDirection.x);
    
    float2 distortedCoords = coords + perpendicularDirection * rippleEffect * 0.01;
    
    return tex2D(uImage0, distortedCoords);
}

technique Technique1
{
    pass RipplePass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}