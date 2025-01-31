using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Reverie.Common.Players;
using Reverie.Core;
using Reverie.Helpers;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Content.Terraria.Items.WoodenWarden
{
    public class BoulderBludgeon : ModItem
    {
        public override string Texture => Assets.Terraria.Items.WoodenWarden + Name;
        public override void SetDefaults()
        {
            Item.damage = 11;
            Item.DamageType = DamageClass.Melee;
            Item.width = Item.height = 68;
            Item.useTime = Item.useAnimation = 24;
            Item.knockBack = 1.8f;
            Item.crit = 3;
            Item.value = Item.sellPrice(silver: 11);
            Item.rare = ItemRarityID.Blue;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.DD2_MonkStaffSwing;
            Item.shootSpeed = 8f;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<BoulderBludgeonProj>();
        }
    }

    public class BoulderBludgeonProj : ModProjectile
    {
        public override string Texture => Assets.Terraria.Projectiles.WoodenWarden + Name;

        private enum AIState
        {
            Spawning,
            Swinging = 2
        }

        AIState State
        {
            get => (AIState)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }

        private bool IsMaxCharge;
        private readonly float MaxChargeTime = 27f;
        private float RotationStart => MathHelper.PiOver2 + (Projectile.direction == -1 ? MathHelper.Pi : 0);
        private float RotationOffset => Projectile.direction == 1 ? 0 : MathHelper.PiOver2;

        public override void SetDefaults()
        {
            Projectile.width = 62; 
            Projectile.height = 74;
            Projectile.penetrate = -1;

            Projectile.DamageType = DamageClass.Melee;
            Projectile.friendly =
            Projectile.netImportant =
            Projectile.ownerHitCheck = true;

            Projectile.tileCollide = IsMaxCharge = false;
        }

        private Player Owner => Main.player[Projectile.owner];

        public override bool PreAI()
        {
            if (!Owner.active || Owner.dead)
            {
                Projectile.Kill();
            }

            if (State == AIState.Swinging)
            {
                Projectile.ai[1] -= 4;

                if (Projectile.ai[1] <= 0)
                {
                    Projectile.Kill();
                }
            }
            else
            {
                if (++Projectile.ai[1] >= MaxChargeTime)
                {
                    IsMaxCharge = true;
                    Projectile.ai[1] = MaxChargeTime;
                }

                if (Main.myPlayer == Projectile.owner && !Owner.channel && Projectile.ai[1] >= MaxChargeTime / 2)
                {
                    State = AIState.Swinging;
                    Projectile.netUpdate = true;
                }
            }

            SetProjectilePosition();
            SetOwnerAnimation();

            return false;
        }

        public override bool? CanDamage() => State != AIState.Spawning;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact);
            Helper.SpawnDustCloud(Projectile.position, Projectile.width, Projectile.height, 0, 60);
        }

        private void SetOwnerAnimation()
        {
            Owner.heldProj = Projectile.whoAmI;

            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, (Projectile.ai[1] / MaxChargeTime * 4) * -Owner.direction);
        }

        private void SetProjectilePosition()
        {
            Vector2 rotatedPoint = Owner.RotatedRelativePoint(Owner.Center);
            float progress = Projectile.ai[1] / MaxChargeTime;

            float easedProgress = State == AIState.Swinging ?
                EaseFunction.EaseCircularInOut.Ease(progress) :
                EaseFunction.EaseQuadOut.Ease(progress);

            // Start with weapon held up
            float baseRotation = Owner.direction == 1 ?
                MathHelper.PiOver2 :  // When facing right, start at 90 degrees (up)
                MathHelper.PiOver2;   // When facing left, also start at 90 degrees

            // Swing arc from up to down
            float swingArc = -MathHelper.Pi; // Full 180 degree swing
            float swingRotation = baseRotation + (swingArc * easedProgress * Owner.direction);

            Projectile.rotation = swingRotation;

            // Adjust distance from player
            float distance = 38f;
            Vector2 offset = Projectile.rotation.ToRotationVector2() * distance;

            // Keep vertical offset consistent
            offset.Y += 4f;
            offset.X -= 36f * Owner.direction;
            Projectile.Center = rotatedPoint + offset;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2;
            float progress = Projectile.ai[1] / MaxChargeTime;
            float scale = 1f + MathF.Sin(-progress * MathHelper.Pi) * 0.2f;
            // Flip based on direction, but maintain vertical orientation
            SpriteEffects effects = Owner.direction == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Main.spriteBatch.Draw(
                texture,
                drawPos,
                null,
                lightColor,
                Projectile.rotation,
                origin,
                scale,
                effects,
                0f
            );
            return false;
        }
    }
}