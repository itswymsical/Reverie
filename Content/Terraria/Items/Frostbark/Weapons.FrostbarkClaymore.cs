using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.Audio;

namespace Reverie.Content.Terraria.Items.Frostbark
{
    public class FrostbarkClaymore : ModItem
    {
        public override string Texture => Assets.Terraria.Items.Frostbark + Name;

        public override void SetDefaults()
        {
            Item.damage = 13;
            Item.DamageType = DamageClass.Melee;
            Item.width = Item.height = 50;
            Item.useTime = 29;
            Item.useAnimation = 29;
            Item.knockBack = 1.7f;
            Item.value = Item.sellPrice(silver: 18);
            Item.rare = ItemRarityID.Blue;
            Item.useTurn = false;
            //Item.noUseGraphic = true;
            Item.channel = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item1;
            Item.shoot = ModContent.ProjectileType<FrostbarkClaymoreProj>();
            Item.shootSpeed = 2.5f;
        }
    }
    public class FrostbarkClaymoreProj : ModProjectile
    {
        public override string Texture => $"{Assets.Terraria.Items.Frostbark}FrostbarkClaymore";

        private const int SwingTime = 10;
        private const int DelayTime = 15;
        private const float SwingRadius = 50f;
        private const float SpriteRotationOffset = -MathHelper.PiOver4;

        private enum SwingState
        {
            SwingUp,
            DelayUp,
            SwingDown,
            DelayDown
        }

        // Projectile.ai[0] is used for the overall timer
        // Projectile.ai[1] is used to store the current swing state

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.ownerHitCheck = true;
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.channel)
            {
                Projectile.Kill();
                return;
            }

            player.heldProj = Projectile.whoAmI;
            player.itemTime = player.itemAnimation = 2;

            // Determine the player's facing direction based on mouse position
            int playerDirection = (Main.MouseWorld.X > player.Center.X) ? 1 : -1;
            player.direction = playerDirection;
            Projectile.direction = Projectile.spriteDirection = playerDirection;

            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter);
            Projectile.ai[0]++;
            SwingState currentState = (SwingState)Projectile.ai[1];

            switch (currentState)
            {
                case SwingState.SwingUp:
                    if (Projectile.ai[0] >= SwingTime)
                    {
                        TransitionToNextState();
                        PlaySwingSound();
                    }
                    break;
                case SwingState.DelayUp:
                    if (Projectile.ai[0] >= DelayTime)
                    {
                        TransitionToNextState();
                    }
                    break;
                case SwingState.SwingDown:
                    if (Projectile.ai[0] >= SwingTime)
                    {
                        TransitionToNextState();
                        PlaySwingSound();
                    }
                    break;
                case SwingState.DelayDown:
                    if (Projectile.ai[0] >= DelayTime)
                    {
                        TransitionToNextState();
                    }
                    break;
            }

            // Calculate the swing rotation based on the current state
            float swingRotation = CalculateSwingRotation(currentState);

            // Adjust rotation based on player direction
            if (playerDirection == -1)
                swingRotation = MathHelper.Pi - swingRotation;

            // Position the projectile
            Vector2 swingOffset = Vector2.UnitX.RotatedBy(swingRotation) * SwingRadius;
            Projectile.Center = playerCenter + swingOffset;

            // Rotate the projectile, accounting for the sprite's base rotation and player direction
            Projectile.rotation = swingRotation + SpriteRotationOffset;
            if (playerDirection == -1)
                Projectile.rotation += MathHelper.Pi;

            // Rotate the player's arm
            player.itemRotation = (swingOffset * playerDirection).ToRotation();
        }

        private void TransitionToNextState()
        {
            Projectile.ai[0] = 0; // Reset the timer
            Projectile.ai[1] = (Projectile.ai[1] + 1) % 4; // Move to the next state, looping back to 0 after 3
        }

        private float CalculateSwingRotation(SwingState state)
        {
            float progress = Projectile.ai[0] / SwingTime;
            switch (state)
            {
                case SwingState.SwingUp:
                    return MathHelper.Lerp(MathHelper.PiOver2, -MathHelper.PiOver2, progress);
                case SwingState.SwingDown:
                    return MathHelper.Lerp(-MathHelper.PiOver2, MathHelper.PiOver2, progress);
                case SwingState.DelayUp:
                    return -MathHelper.PiOver2;
                case SwingState.DelayDown:
                    return MathHelper.PiOver2;
                default:
                    return 0f;
            }
        }

        private void PlaySwingSound()
        {
            SoundEngine.PlaySound(SoundID.Item1 with { Volume = 0.5f, Pitch = -0.2f }, Projectile.Center);
        }
    }
}