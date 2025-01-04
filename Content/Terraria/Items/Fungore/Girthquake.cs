using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Reverie.Core.Mechanics;
using Reverie.Core.PrimitiveDrawing;

using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Reverie.Core;

namespace Reverie.Content.Terraria.Items.Fungore
{
    public class Girthquake : ModItem
    {
        public override string Texture => Assets.Terraria.Items.Fungore + Name;

        public override void SetDefaults()
        {
            Item.damage = 12;
            Item.DamageType = DamageClass.Melee;
            Item.width = 36;
            Item.height = 44;
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.reuseDelay = 20;
            Item.channel = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6.5f;
            Item.crit = 9;
            Item.shootSpeed = 14f;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<GirthquakeProj>();
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.autoReuse = false;
            Item.value = Item.sellPrice(0, 1, 50, 0);
            Item.rare = ItemRarityID.Blue;
        }
    }
    internal class GirthquakeProj : ModProjectile
    {
        enum State : int
        {
            Down = 0,
            Up = 1,
            Reset = 2
        }

        private State currentAttack = State.Down;
        private bool initialized = false;
        private int attackDuration = 0;
        private float startRotation = 0f;
        private float endRotation = 0f;
        private bool facingRight;
        private float rotVel = 0f;
        private int growCounter = 0;
        private List<float> oldRotation = new();
        private List<Vector2> oldPosition = new();
        private List<NPC> struckNPCs = new();

        public override string Texture => Assets.Terraria.Projectiles.Fungore + Name;
        Player Owner => Main.player[Projectile.owner];
        private bool FirstTickOfSwing => Projectile.ai[0] == 0;

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.Size = new Vector2(100, 100);
            Projectile.penetrate = -1;
            Projectile.ownerHitCheck = true;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;
            Projectile.Center = Main.GetPlayerArmPosition(Projectile);
            Owner.heldProj = Projectile.whoAmI;

            if (FirstTickOfSwing)
            {
                struckNPCs = new List<NPC>();

                facingRight = Owner.DirectionTo(Main.MouseWorld).X > 0;
                float rot = Owner.DirectionTo(Main.MouseWorld).ToRotation();

                if (!initialized)
                {
                    initialized = true;
                    endRotation = rot - 1f * Owner.direction;
                    oldRotation = new List<float>();
                    oldPosition = new List<Vector2>();
                }
                else
                {
                    currentAttack = (State)((int)currentAttack + 1);
                    if (currentAttack == State.Reset)
                        currentAttack = State.Down;
                }

                startRotation = endRotation;

                switch (currentAttack)
                {
                    case State.Down:
                        endRotation = rot + 2f * Owner.direction;
                        attackDuration = 120;
                        break;

                    case State.Up:
                        endRotation = rot - 2f * Owner.direction;
                        attackDuration = 120;
                        break;
                }

                Projectile.ai[0] += 30f / attackDuration;
            }

            if (Projectile.ai[0] < 1)
            {
                Projectile.timeLeft = 50;
                Projectile.ai[0] += 1f / attackDuration;
                rotVel = Math.Abs(EaseProgress(Projectile.ai[0]) - EaseProgress(Projectile.ai[0] - 1f / attackDuration)) * 2;
            }
            else
            {
                rotVel = 0f;

                if (Main.mouseLeft)
                {
                    Projectile.ai[0] = 0;
                    return;
                }
            }

            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            float progress = EaseProgress(Projectile.ai[0]);
            Projectile.scale = MathHelper.Min(MathHelper.Min(growCounter++ / 30f, 1 + rotVel * 4), 1.3f);
            Projectile.rotation = MathHelper.Lerp(startRotation, endRotation, progress);

            Owner.ChangeDir(facingRight ? 1 : -1);
            UpdatePlayerRotation();

            oldRotation.Add(Projectile.rotation);
            oldPosition.Add(Projectile.Center);

            if (oldRotation.Count > 16)
                oldRotation.RemoveAt(0);
            if (oldPosition.Count > 16)
                oldPosition.RemoveAt(0);
        }

        private void UpdatePlayerRotation()
        {
            float wrappedRotation = MathHelper.WrapAngle(Projectile.rotation);

            if (facingRight)
                Owner.itemRotation = MathHelper.Clamp(wrappedRotation, -1.57f, 1.57f);
            else if (wrappedRotation > 0)
                Owner.itemRotation = MathHelper.Clamp(wrappedRotation, 1.57f, 4.71f);
            else
                Owner.itemRotation = MathHelper.Clamp(wrappedRotation, -1.57f, -4.71f);

            Owner.itemRotation = MathHelper.WrapAngle(Owner.itemRotation - (facingRight ? 0 : MathHelper.Pi));
            Owner.itemAnimation = Owner.itemTime = 5;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (rotVel < 0.005f)
                return false;

            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                Projectile.Center, Projectile.Center + 42 * Projectile.rotation.ToRotationVector2(), 20, ref collisionPoint);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            struckNPCs.Add(target);
            CameraSystem.Shake += 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            var origin = new Vector2(0, tex.Height);
            Vector2 scaleVec = Vector2.One;

            for (int k = 16; k > 0; k--)
            {
                float progress = 1 - (float)((16 - k) / (float)16);
                Color color = lightColor * EaseFunction.EaseQuarticOut.Ease(progress) * 0.1f;

                if (k > 0 && k < oldRotation.Count)
                    Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, color,
                        oldRotation[k] + 0.78f, origin, Projectile.scale * scaleVec, SpriteEffects.None, 0f);
            }

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor,
                Projectile.rotation + 0.78f, origin, Projectile.scale * scaleVec, SpriteEffects.None, 0f);
            return false;
        }

        private float EaseProgress(float input) => EaseFunction.EaseCircularInOut.Ease(input);
    }
}
