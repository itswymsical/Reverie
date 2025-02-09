
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
        dust.alpha = 210;
        dust.fadeIn = 14f;
    }

    public override bool Update(Dust dust)
    {
        if (Main.rand.Next(0, 9) == 1) dust.alpha++;

        if (Main.rand.Next(0, 15) == 1) dust.velocity.Y += 0.0008f;

        dust.position -= dust.velocity;

        if (dust.alpha > 255)
            dust.active = false;
        
        return false;
    }
}