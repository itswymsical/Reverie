using Reverie.Content.NPCs.Bosses.KingSlime;
using Reverie.Content.Projectiles.Sharpnut;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace Reverie.Content.Items.KingSlime;

public class GelatinousBlasterItem : ModItem
{
    public override void SetDefaults()
    {
        Item.DamageType = DamageClass.Ranged;
        Item.damage = 11;

        Item.width = 44;
        Item.height = 32;

        Item.useTime = 18;
        Item.useAnimation = 18;

        Item.useAmmo = AmmoID.Gel;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.UseSound = SoundID.DD2_BallistaTowerShot;
        Item.rare = ItemRarityID.Blue;

        Item.autoReuse = true;
        Item.noMelee = true;
        Item.channel = true;
        Item.shootSpeed = 10f;
        Item.shoot = ModContent.ProjectileType<GelBallProj>();
    }

    public override Vector2? HoldoutOffset()
    {
        return new(-8, 0);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        var muzzleOffset = Vector2.Normalize(new Vector2(velocity.X, velocity.Y)) * 25f;
        if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0))
        {
            position += muzzleOffset;
        }
        Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<GelBallProj>(), damage, knockback, player.whoAmI);
        return false;
    }
}

public class GelBallProj : ModProjectile
{
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
    }

    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.aiStyle = ProjAIStyleID.Arrow;
        Projectile.penetrate = 1;
        Projectile.alpha = 190;
        Projectile.timeLeft = 300;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        int tileX = (int)(Projectile.Center.X / 16f);
        int tileY = (int)(Projectile.Center.Y / 16f);

        for (int x = tileX - 1; x <= tileX + 1; x++)
        {
            for (int y = tileY - 1; y <= tileY + 1; y++)
            {
                if (WorldGen.InWorld(x, y))
                {
                    Tile tile = Framing.GetTileSafely(x, y);

                    if (tile.HasTile && Main.tileSolid[tile.TileType])
                    {
                        SlimedTileSystem.AddSlimedTile(x, y);
                    }
                }
            }
        }

        SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}SlimeConsume") with { Pitch = -0.3f, Volume = 0.33f }, Projectile.position);
        for (int i = 0; i < 30; i++)
        {
            Dust dust = Dust.NewDustPerfect(Projectile.position, DustID.t_Slime, -Vector2.UnitY.RotatedByRandom(MathHelper.PiOver2) * 4f, 100,
                new Color(86, 162, 255, 100), 1.4f);
            dust.noGravity = true;
        }

        return true;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        target.AddBuff(BuffID.Slow, 2 * 60);
        target.AddBuff(BuffID.Slimed, 2 * 60);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Main.instance.LoadProjectile(Projectile.type);
        var texture = TextureAssets.Projectile[Projectile.type].Value;

        Vector2 drawOrigin = new(texture.Width * 0.5f, Projectile.height * 0.5f);
        for (var k = 0; k < Projectile.oldPos.Length; k++)
        {
            var drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
            var color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
            Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
        }

        return true;
    }
}
