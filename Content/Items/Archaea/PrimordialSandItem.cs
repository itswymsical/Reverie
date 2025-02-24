using Reverie.Content.Projectiles.Archaea;
using Reverie.Content.Tiles.Archaea;

namespace Reverie.Content.Items.Archaea
{
    public class PrimordialSandItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;

            ItemID.Sets.SandgunAmmoProjectileData[Type] = new(ModContent.ProjectileType<PrimordialSandBallGunProjectile>(), 10);
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<PrimordialSandTile>());
            Item.width = 12;
            Item.height = 12;
            Item.ammo = AmmoID.Sand;
            Item.notAmmo = true;
        }
    }
}
