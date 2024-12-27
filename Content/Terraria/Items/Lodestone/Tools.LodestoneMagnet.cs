using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Effects;
using Reverie.Helpers;
using Reverie.Core.Interfaces;
using Reverie.Core.PrimitiveDrawing;

namespace Reverie.Content.Terraria.Items.Lodestone
{
    public class LodestoneMagnet : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.Minishark;
        public override void SetDefaults()
        {
            Item.useTime = Item.useAnimation = 20;
            Item.width = Item.height = 32;

            Item.autoReuse = Item.useTurn = true;

            Item.value = Item.sellPrice(gold: 1);

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item18;
            Item.rare = ItemRarityID.Green;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.shootSpeed = 7f;
            Item.shoot = ModContent.ProjectileType<LodestoneMagnetProj>();
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 muzzleOffset = Vector2.Normalize(new Vector2(velocity.X, velocity.Y)) * 25f;
            if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0))
            {
                position += muzzleOffset;
            }
            Projectile.NewProjectile(source, position, Vector2.Zero, ModContent.ProjectileType<LodestoneMagnetProj>(), damage, knockback, player.whoAmI);
            return false;
        }
        public override bool CanUseItem(Player player)
            => player.ownedProjectileCounts[ModContent.ProjectileType<LodestoneMagnetProj>()] <= 0;

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<Lodestone>(25);
            recipe.AddIngredient<MagnetizedCoil>(15);
            recipe.AddRecipeGroup(nameof(ItemID.SilverBar), 10);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
    public class LodestoneMagnetProj : ModProjectile, IDrawPrimitive
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.Minishark;
        private const float MagnetRange = 200f;
        private const float ItemVacuumRange = 300f;
        private const float ItemVacuumSpeed = 8f;
        private const float ArcAngle = MathHelper.Pi / 6;
        private const int MiningSpeed = 5;
        private int miningTimer = 0;
        private List<Vector2> magnetizedTiles = [];
        private HashSet<int> magnetizedItems = [];

        private List<Vector2> cache;
        private Trail trail;
        private Trail trail2;
        private Color color = new(255, 255, 255);
        private readonly Vector2 Size = new(100, 50);
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.ownerHitCheck = true;
        }

        public override void AI()
        {
            ManageCaches();
            ManageTrail();
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead || !owner.channel)
            {
                Projectile.Kill();
                return;
            }

            Vector2 aimDirection = Vector2.Normalize(Main.MouseWorld - owner.Center);
            Projectile.Center = owner.Center + aimDirection * 50f;

            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.rotation = (Main.MouseWorld - Projectile.Center).ToRotation();
                Projectile.netUpdate = true;

                Projectile.direction = (Main.MouseWorld.X > owner.Center.X) ? 1 : -1;
            }

            owner.ChangeDir(Projectile.direction);
            Projectile.spriteDirection = Projectile.direction;

            if (Projectile.owner == Main.myPlayer)
            {
                MagnetizeTiles(owner);
                VacuumItems();
            }

            //DrawRaycastDust();
            SetOwnerAnimation(owner);

            miningTimer++;
        }
        private void ManageCaches()
        {
            Player player = Main.LocalPlayer;
            Vector2 pos = Projectile.Center + (player.DirectionTo(Projectile.Center) * (Size.Length() * Main.rand.NextFloat(0.5f, 1.1f))) + (Main.rand.NextVector2Unit() * Main.rand.NextFloat(1.0f, 4.0f));

            if (cache == null)
            {
                cache = [];

                for (int i = 0; i < 15; i++)
                {
                    cache.Add(pos);
                }
            }

            cache.Add(pos);

            while (cache.Count > 15)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            Vector2 start = Projectile.Center;
            Vector2 end = Main.MouseWorld;
            Vector2 direction = Vector2.Normalize(end - start);
            float distance = Math.Min(Vector2.Distance(start, end), MagnetRange); // Limit the distance to the magnet's max range

            int trailSegments = 15;
            float segmentLength = distance / trailSegments;

            float trailWidth = MagnetRange * (float)Math.Tan(ArcAngle / 2) * 1.2f;

            trail ??= new Trail(Main.instance.GraphicsDevice, trailSegments, new RoundedTip(5), factor => factor * trailWidth, factor =>
            {
                if (factor.X >= 0.98f)
                    return Color.White * 0;
                return new Color(color.R, color.G, color.B) * 0.03f;
            });

            trail2 ??= new Trail(Main.instance.GraphicsDevice, trailSegments, new RoundedTip(5), factor => factor * trailWidth, factor =>
            {
                if (factor.X >= 0.98f)
                    return Color.White * 0;
                return new Color(color.R, color.G, color.B) * 0.03f;
            });

            Vector2[] trailPositions = new Vector2[trailSegments];
            for (int i = 0; i < trailSegments; i++)
            {
                float progress = (float)i / (trailSegments - 1);
                trailPositions[i] = Vector2.Lerp(start, start + direction * distance, progress); // Calculate trail positions based on the limited distance
            }

            trail.Positions = trailPositions;
            trail2.Positions = trailPositions;

            trail.NextPosition = start + direction * distance; // Set the trail's end position to the limited distance
            trail2.NextPosition = start + direction * distance;
        }

        public void DrawPrimitives()
        {
            var primitiveShader = Filters.Scene["LightningTrail"];
            if (primitiveShader != null)
            {
                Effect effect = primitiveShader.GetShader().Shader;
                if (effect != null)
                {
                    var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
                    Matrix view = Main.GameViewMatrix.TransformationMatrix;
                    var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

                    effect.Parameters["time"]?.SetValue(Main.GameUpdateCount * 0.2f);
                    effect.Parameters["repeats"]?.SetValue(8f);
                    effect.Parameters["transformMatrix"]?.SetValue(world * view * projection);
                    effect.Parameters["sampleTexture"]?.SetValue(ModContent.Request<Texture2D>("ReverieMod/Assets/VFX/WaterTrail").Value);
                    effect.Parameters["sampleTexture2"]?.SetValue(ModContent.Request<Texture2D>("ReverieMod/Assets/VFX/Bloom").Value);

                    trail?.Render(effect);

                    effect.Parameters["sampleTexture2"]?.SetValue(ModContent.Request<Texture2D>("ReverieMod/Assets/VFX/WaterTrail").Value);

                    trail2?.Render(effect);
                }
            }
        }

        private void MagnetizeTiles(Player owner)
        {
            Vector2 start = Projectile.Center;
            Vector2 end = Main.MouseWorld;
            Vector2 direction = Vector2.Normalize(end - start);
            float distance = Math.Min(Vector2.Distance(start, end), MagnetRange);

            float leftAngle = direction.ToRotation() - ArcAngle / 2;
            float rightAngle = direction.ToRotation() + ArcAngle / 2;

            for (float angle = leftAngle; angle <= rightAngle; angle += 0.1f)
            {
                Vector2 arcDirection = angle.ToRotationVector2();
                for (float i = 0; i <= distance; i += 16f)
                {
                    Vector2 checkPos = start + arcDirection * i;
                    int tileX = (int)(checkPos.X / 16f);
                    int tileY = (int)(checkPos.Y / 16f);

                    if (Main.tile[tileX, tileY].HasTile && Main.tileSpelunker[Main.tile[tileX, tileY].TileType] 
                        || Main.tile[tileX, tileY].TileType == TileID.Hellstone)
                    {
                        if (miningTimer % MiningSpeed == 0)
                        {
                            BreakTile(tileX, tileY, owner);
                        }
                    }
                }
            }
        }
  
        private void DrawRaycastDust() //debugging thing for trail cache perimeter 
        {
            Vector2 start = Projectile.Center;
            Vector2 end = Main.MouseWorld;
            Vector2 direction = Vector2.Normalize(end - start);
            float distance = Math.Min(Vector2.Distance(start, end), MagnetRange);

            float leftAngle = direction.ToRotation() - ArcAngle / 2;
            float rightAngle = direction.ToRotation() + ArcAngle / 2;

            int dustCount = 20;
            for (int i = 0; i <= dustCount; i++)
            {
                float progress = i / (float)dustCount;

                // Left arc line
                Vector2 leftDustPos = start + (leftAngle.ToRotationVector2() * distance * progress);
                Dust leftDust = Dust.NewDustPerfect(leftDustPos, DustID.Electric, Vector2.Zero, 0, Color.Blue, 0.5f);
                leftDust.noGravity = true;
                leftDust.noLight = true;

                // Right arc line
                Vector2 rightDustPos = start + (rightAngle.ToRotationVector2() * distance * progress);
                Dust rightDust = Dust.NewDustPerfect(rightDustPos, DustID.Electric, Vector2.Zero, 0, Color.Blue, 0.5f);
                rightDust.noGravity = true;
                rightDust.noLight = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            foreach (int itemIndex in magnetizedItems)
            {
                if (itemIndex < Main.item.Length && Main.item[itemIndex].active)
                {
                    Item item = Main.item[itemIndex];
                    item.beingGrabbed = false;

                    item.velocity *= 0.5f;

                    item.velocity.Y -= 1f;

                    item.noGrabDelay = 0;
                }
            }

            magnetizedItems.Clear();
            base.OnKill(timeLeft);
        }

        private void BreakTile(int x, int y, Player player)
        {
            
            player.PickTile( x, y, 10);
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, x, y);
            }
        }

        private void VacuumItems()
        {
            magnetizedItems.Clear();

            for (int i = 0; i < Main.maxItems; i++)
            {
                Item item = Main.item[i];
                if (item.active && item.noGrabDelay == 0 && item.type != ItemID.None)
                {
                    float distance = Vector2.Distance(item.Center, Projectile.Center);
                    if (distance <= ItemVacuumRange)
                    {
                        item.beingGrabbed = true;
                        Vector2 directionToMagnet = Vector2.Normalize(Projectile.Center - item.Center);
                        float speed = MathHelper.Lerp(ItemVacuumSpeed, 1f, distance / ItemVacuumRange);
                        item.velocity = directionToMagnet * speed;

                        // Counteract gravity
                        item.velocity.Y -= 0.2f;

                        magnetizedItems.Add(i);
                    }
                }
            }
        }

        private void SetOwnerAnimation(Player owner)
        {
            owner.ChangeDir(Projectile.direction);
            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;

            owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (owner.Center - Main.MouseWorld).ToRotation() + MathHelper.PiOver2);
        }
    }
}