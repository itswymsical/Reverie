using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Reverie.Core.Interfaces;
using Reverie.Core.PrimitiveDrawing;
using Reverie.Helpers;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Content.Terraria.Items.WoodenWarden
{
    public class Bramblebow : ModItem
    {
        public override string Texture => Assets.Terraria.Items.WoodenWarden + Name;
        public override void SetDefaults()
        {
            Item.damage = 11;
            Item.width = 32;
            Item.height = 56;
            Item.useTime = Item.useAnimation = 27;
            Item.knockBack = 1.2f;
            Item.crit = 3;
            Item.value = Item.sellPrice(silver: 14);
            Item.rare = ItemRarityID.Blue;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item5;
            Item.shootSpeed = 13.75f;
            Item.useAmmo = AmmoID.Arrow;
            Item.DamageType = DamageClass.Ranged;

            Item.shoot = ModContent.ProjectileType<BrambleArrowProj>();
        }
        public override Vector2? HoldoutOffset() => new Vector2(-8, 0);

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (type == ProjectileID.WoodenArrowFriendly) type = ModContent.ProjectileType<BrambleArrowProj>();
        }
    }

    public class BrambleArrowProj : ModProjectile
    {
        public override string Texture => Assets.Terraria.Projectiles.WoodenWarden + Name;
        public int MaxStickies => 3;
        private bool isStuck = false;

        protected NPC Target => Main.npc[(int)Projectile.ai[1]];

        protected bool stickToNPC;

        protected bool stickingToNPC;

        private Vector2 offset;

        private float oldRotation;

        private List<Vector2> cache;
        private Trail trail;
        private Trail trail2;
        private Color color = new(255, 255, 255);
        private readonly Vector2 Size = new(46, 50);
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
            ProjectileID.Sets.TrailingMode[Type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;

            stickToNPC = 
                Projectile.tileCollide = 
                Projectile.arrow =
                Projectile.friendly = true;

            Projectile.DamageType = DamageClass.Ranged;

            Projectile.localNPCHitCooldown = 20;
            Projectile.timeLeft = 660;
            //Projectile.extraUpdates = 1;
            Projectile.penetrate = 3;
        }
        public override void AI()
        {
            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] >= 15f)
            {
                Projectile.ai[0] = 15f;
                Projectile.velocity.Y += 0.1f;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Projectile.velocity.Y > 16f)
            {
                Projectile.velocity.Y = 16f;
            }

            if (stickingToNPC)
            {
                if (Target.active && !Target.dontTakeDamage)
                {
                    Projectile.tileCollide = false;

                    Projectile.Center = Target.Center - offset;

                    Projectile.gfxOffY = Target.gfxOffY;
                }
                else
                {
                    Projectile.Kill();
                }
            }

            if (stickingToNPC)
                Projectile.rotation = oldRotation;

            Player player = Main.player[Projectile.owner];
            float distanceToPlayer = Vector2.Distance(Projectile.Center, player.Center);

            Projectile.spriteDirection = Projectile.direction;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!stickingToNPC && stickToNPC)
            {
                Projectile.ai[1] = target.whoAmI;

                oldRotation = Projectile.rotation;

                offset = target.Center - Projectile.Center + (Projectile.velocity * 0.75f);

                stickingToNPC = true;

                Projectile.netUpdate = true;
            }
            else
            {
                RemoveStackProjectiles();
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item50, target.position);
            isStuck = true;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.t_LivingWood);
                dust.noGravity = true;
                dust.velocity *= 1.5f;
                dust.scale *= 0.9f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.instance.LoadProjectile(Projectile.type);
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            return false;
        }

        protected void RemoveStackProjectiles()
        {
            var sticking = new Point[MaxStickies];
            int index = 0;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile currentProjectile = Main.projectile[i];

                if (i != Projectile.whoAmI
                    && currentProjectile.active
                    && currentProjectile.owner == Main.myPlayer
                    && currentProjectile.type == Projectile.type
                    && currentProjectile.ai[0] == 1f
                    && currentProjectile.ai[1] == Target.whoAmI
                )
                {
                    sticking[index++] = new Point(i, currentProjectile.timeLeft);

                    if (index >= sticking.Length)
                        break;
                }
            }

            if (index >= sticking.Length)
            {
                int oldIndex = 0;

                for (int i = 1; i < sticking.Length; i++)
                    if (sticking[i].Y < sticking[oldIndex].Y)
                        oldIndex = i;

                Main.projectile[sticking[oldIndex].X].Kill();
            }
        }
    }
}
