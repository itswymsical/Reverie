using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Content.Terraria.Projectiles
{
    public class StoneProj : ModProjectile
    {
        public override string Texture => Assets.Terraria.Projectiles.Dir + Name;
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.damage = 8;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.timeLeft = 180;
            Projectile.width = Projectile.height = 18;
            Projectile.hostile = true;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            AIType = ProjectileID.JungleSpike;
        }
    }
}
