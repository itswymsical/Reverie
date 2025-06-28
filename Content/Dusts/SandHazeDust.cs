
namespace Reverie.Content.Dusts;

public class SandHazeDust : ModDust
{
    public override void OnSpawn(Dust dust)
    {
        dust.velocity *= 0.05f;
        dust.velocity.Y *= 0.5f;
        dust.noGravity = true;
        dust.noLight = true;
        dust.scale *= 0.37f;
        dust.frame = new((Main.rand.Next(0, 1) == 0) ? 0 : 250, 0, 250, 115);
        dust.alpha = 255;
        dust.fadeIn = 0f;
    }

    public override bool Update(Dust dust)
    {
        dust.fadeIn++;
        float fadeInDuration = 35f;
        if (dust.fadeIn < fadeInDuration)
        {
            float progress = dust.fadeIn / fadeInDuration;
            float easedProgress = progress * progress * progress;
            dust.alpha = (int)(255f - easedProgress * 45f);
        }
        else
        {
            if (Main.rand.Next(0, 6) == 1) dust.alpha++;
        }

        // Velocity-dependent player collision
        Player nearestPlayer = Main.player[Main.myPlayer];
        if (nearestPlayer.active && !nearestPlayer.dead)
        {
            float playerSpeed = nearestPlayer.velocity.Length();
            float minSpeed = 4f; // Minimum speed needed to affect dust

            if (playerSpeed > minSpeed)
            {
                Vector2 dustCenter = dust.position;
                Vector2 playerCenter = nearestPlayer.Center;
                float distance = Vector2.Distance(dustCenter, playerCenter);

                float collisionRange = 80f;

                if (distance < collisionRange && distance > 0)
                {
                    Vector2 pushDirection = Vector2.Normalize(dustCenter - playerCenter);
                    float pushStrength = (collisionRange - distance) / collisionRange;

                    // Scale effect strength by player velocity
                    float velocityMultiplier = MathHelper.Clamp((playerSpeed - minSpeed) / 8f, 0f, 1f);
                    pushStrength *= velocityMultiplier;

                    // Push dust away from player
                    Vector2 pushForce = pushDirection * pushStrength * 0.3f;
                    dust.velocity += pushForce;

                    // Add player's velocity influence
                    dust.velocity += nearestPlayer.velocity * 0.03f * velocityMultiplier;
                }
            }
        }

        // Apply velocity damping
        dust.velocity *= 0.98f;

        if (Main.rand.Next(0, 15) == 1) dust.velocity.Y += 0.0008f;
        dust.position -= dust.velocity;

        if (dust.alpha > 255)
            dust.active = false;

        return false;
    }
}