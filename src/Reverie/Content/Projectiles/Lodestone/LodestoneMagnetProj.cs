using Reverie.Core.Graphics;
using Reverie.Core.Interfaces;
using Reverie.Utilities;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using static Reverie.Reverie;

namespace Reverie.Content.Projectiles.Lodestone;

public class LodestoneMagnetProj : ModProjectile, IDrawPrimitive
{
    private const float MAGNET_RANGE = 200f;
    private const float ITEM_VACUUM_RANGE = 300f;
    private const float ITEM_VACUUM_SPEED = 8f;
    private const float ARC_ANGLE = MathHelper.Pi / 6;
    private const int MINING_SPEED = 5;
    private int miningTimer = 0;
    private readonly HashSet<int> magnetizedItems = [];

    private List<Vector2> cache;
    private Trail trail;
    private Trail trail2;
    private Color color = new(255, 255, 255);
    private readonly Vector2 Size = new(100, 50);
    public override void SetDefaults()
    {
        Projectile.width = 26;
        Projectile.height = 32;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.ownerHitCheck = true;
    }

    public override void AI()
    {
        ManageCaches();
        ManageTrail();
        var owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead || !owner.channel)
        {
            Projectile.Kill();
            return;
        }

        var aimDirection = Vector2.Normalize(Main.MouseWorld - owner.Center);
        Projectile.Center = owner.Center + aimDirection * 22f;

        if (Main.myPlayer == Projectile.owner)
        {
            Projectile.rotation = (Main.MouseWorld - Projectile.Center).ToRotation();
            Projectile.netUpdate = true;

            Projectile.direction = Main.MouseWorld.X > owner.Center.X ? 1 : -1;
        }

        owner.ChangeDir(Projectile.direction);

        if (Projectile.owner == Main.myPlayer)
        {
            MagnetizeTiles(owner);
            VacuumItems();
        }

        SetOwnerAnimation(owner);

        miningTimer++;
    }

    private void ManageCaches()
    {
        var player = Main.LocalPlayer;
        var pos = Projectile.Center + player.DirectionTo(Projectile.Center) * (Size.Length() * Main.rand.NextFloat(0.5f, 1.1f)) + Main.rand.NextVector2Unit() * Main.rand.NextFloat(1.0f, 4.0f);

        if (cache == null)
        {
            cache = [];

            for (var i = 0; i < 15; i++)
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
        var start = Projectile.Center;
        var end = Main.MouseWorld;
        var direction = Vector2.Normalize(end - start);
        var distance = Math.Min(Vector2.Distance(start, end), MAGNET_RANGE);

        var trailSegments = 15;
        var segmentLength = distance / trailSegments;

        var trailWidth = MAGNET_RANGE * (float)Math.Tan(ARC_ANGLE / 2) * 1.2f;

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

        var trailPositions = new Vector2[trailSegments];
        for (var i = 0; i < trailSegments; i++)
        {
            var progress = (float)i / (trailSegments - 1);
            trailPositions[i] = Vector2.Lerp(start, start + direction * distance, progress);
        }

        trail.Positions = trailPositions;
        trail2.Positions = trailPositions;

        trail.NextPosition = start + direction * distance;
        trail2.NextPosition = start + direction * distance;
    }

    public void DrawPrimitives()
    {
        var primitiveShader = Filters.Scene["LightningTrail"];
        if (primitiveShader != null)
        {
            var effect = primitiveShader.GetShader().Shader;
            if (effect != null)
            {
                var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
                var view = Main.GameViewMatrix.TransformationMatrix;
                var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

                effect.Parameters["time"]?.SetValue(Main.GameUpdateCount * 0.2f);
                effect.Parameters["repeats"]?.SetValue(8f);
                effect.Parameters["transformMatrix"]?.SetValue(world * view * projection);
                effect.Parameters["sampleTexture"]?.SetValue(ModContent.Request<Texture2D>($"{VFX_DIRECTORY}WaterTrail").Value);
                effect.Parameters["sampleTexture2"]?.SetValue(ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Bloom").Value);

                trail?.Render(effect);

                effect.Parameters["sampleTexture2"]?.SetValue(ModContent.Request<Texture2D>($"{VFX_DIRECTORY}WaterTrail").Value);

                trail2?.Render(effect);
            }
        }
    }

    private void MagnetizeTiles(Player owner) // quite whimsical \(~.~)/ 
    {
        var start = Projectile.Center - new Vector2(Projectile.width, Projectile.height) / 2;
        var end = Main.MouseWorld;
        var direction = Vector2.Normalize(end - start);
        var distance = Math.Min(Vector2.Distance(start, end), MAGNET_RANGE);

        var leftAngle = direction.ToRotation() - ARC_ANGLE / 2;
        var rightAngle = direction.ToRotation() + ARC_ANGLE / 2;

        for (var angle = leftAngle; angle <= rightAngle; angle += 0.1f)
        {
            var arcDirection = angle.ToRotationVector2();
            for (float i = 0; i <= distance; i += 16f)
            {
                var checkPos = start + arcDirection * i;
                var tileX = (int)(checkPos.X / 16f);
                var tileY = (int)(checkPos.Y / 16f);
                Tile tile = Main.tile[tileX, tileY];
                if (tile.HasTile && Main.tileSpelunker[tile.TileType] 
                    && (tile.TileType is not 82 or 83 or 84) || tile.TileType is TileID.Hellstone)
                {
                    if (miningTimer % MINING_SPEED is 0)
                    {
                        BreakTile(tileX, tileY, owner);
                    }
                }
            }
        }
    }

    public override void OnKill(int timeLeft)
    {
        foreach (var itemIndex in magnetizedItems)
        {
            if (itemIndex < Main.item.Length && Main.item[itemIndex].active)
            {
                var item = Main.item[itemIndex];
                item.beingGrabbed = false;

                item.velocity *= 0.5f;

                item.velocity.Y -= 1f;

                item.noGrabDelay = 0;
            }
        }

        magnetizedItems.Clear();
        base.OnKill(timeLeft);
    }

    private static void BreakTile(int x, int y, Player player)
    {

        player.PickTile(x, y, 10);
        if (Main.netMode is NetmodeID.MultiplayerClient)
        {
            NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, x, y);
        }
    }

    private void VacuumItems()
    {
        magnetizedItems.Clear();

        for (var i = 0; i < Main.maxItems; i++)
        {
            var item = Main.item[i];
            if (item.active && item.noGrabDelay is 0 && item.type is not ItemID.None && ItemUtils.IsAMetalItem(item))
            {
                var distance = Vector2.Distance(item.Center, Projectile.Center);
                if (distance <= ITEM_VACUUM_RANGE)
                {
                    item.beingGrabbed = true;
                    var directionToMagnet = Vector2.Normalize(Projectile.Center - item.Center);
                    var speed = MathHelper.Lerp(ITEM_VACUUM_SPEED, 1f, distance / ITEM_VACUUM_RANGE);
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
