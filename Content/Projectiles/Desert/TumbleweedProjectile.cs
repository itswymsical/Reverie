namespace Reverie.Content.Projectiles.Desert;

public class TumbleweedProjectile : ModProjectile
{
    private const float BaseGravity = 0.07f;
    private const float WindResponse = 0.001f;
    private const float MaxSpeed = 8f;
    private const float GroundFriction = 0.85f;
    private const float BounceStrength = 0.81f;

    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 24;
        Projectile.aiStyle = -1;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 500;
        Projectile.tileCollide = true;
    }

    public override void AI()
    {
        ApplyPhysics();
        HandleMovement();
        UpdateVisuals();
    }

    private void ApplyPhysics()
    {
        float windSpeed = Math.Abs(Main.windSpeedCurrent);

        float gravity = Math.Max(BaseGravity - (windSpeed * 0.02f), 0.01f);
        Projectile.velocity.Y += gravity;

        Projectile.velocity.X += Main.windSpeedCurrent * WindResponse;

        if (windSpeed > 0.3f)
        {
            Projectile.velocity.X += Main.rand.NextFloat(-0.3f, 0.3f) * windSpeed;
            Projectile.velocity.Y += Main.rand.NextFloat(-0.2f, 0.1f) * windSpeed;
        }
    }

    private void HandleMovement()
    {
        if (Projectile.velocity.Length() > MaxSpeed)
            Projectile.velocity *= 0.95f;

        // Ground behavior
        if (Projectile.velocity.Y == 0f)
        {
            Projectile.velocity.X *= GroundFriction;

            // Occasional bounce
            if (Main.rand.NextBool(60))
                Projectile.velocity.Y = -Main.rand.NextFloat(2f, 5f);
        }
    }

    private void UpdateVisuals()
    {
        Projectile.rotation += Projectile.velocity.X * 0.03f;

        if (Projectile.timeLeft < 300)
            Projectile.alpha = (int)(255 * (1f - Projectile.timeLeft / 300f));
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        // bouncy collision
        if (Projectile.velocity.X != oldVelocity.X)
            Projectile.velocity.X = -oldVelocity.X * 0.3f;

        if (Projectile.velocity.Y != oldVelocity.Y)
            Projectile.velocity.Y = -oldVelocity.Y * BounceStrength;

        // bounce variation
        Projectile.velocity += new Vector2(
            Main.rand.NextFloat(-0.5f, 0.5f),
            Main.rand.NextFloat(-0.3f, 0.1f)
        );

        return false;
    }
}