using Reverie.Core.Graphics;
using Reverie.Core.Interfaces;
using Reverie.Core.Loaders;
using Reverie.Utilities;
using System.Collections.Generic;
using System.Linq;
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

    private List<List<Vector2>> fieldLines;
    private readonly int FIELD_LINE_COUNT = 6;
    private readonly int POINTS_PER_LINE = 20;
    private float magnetStrength = 0.8f;
    private float gravityStrength = 0.2f;

    private void InitializeFieldLines()
    {
        fieldLines = new List<List<Vector2>>();
        for (int i = 0; i < FIELD_LINE_COUNT; i++)
        {
            var line = new List<Vector2>();
            // Spread lines in a semicircle pattern from projectile front
            float angle = MathHelper.Lerp(-MathHelper.PiOver2, MathHelper.PiOver2, i / (float)(FIELD_LINE_COUNT - 1));
            Vector2 dir = Projectile.rotation.ToRotationVector2().RotatedBy(angle);
            Vector2 startPos = Projectile.Center;

            for (int j = 0; j < POINTS_PER_LINE; j++)
            {
                line.Add(startPos);
            }
            fieldLines.Add(line);
        }
    }

    private void UpdateFieldLines()
    {
        if (fieldLines == null || fieldLines.Count == 0)
        {
            InitializeFieldLines();
            return;
        }

        Vector2 targetPos = Main.MouseWorld;

        for (int i = 0; i < fieldLines.Count; i++)
        {
            var line = fieldLines[i];

            // Calculate base angle for this field line
            float baseAngle = MathHelper.Lerp(-MathHelper.PiOver2, MathHelper.PiOver2, i / (float)(FIELD_LINE_COUNT - 1));

            // Start from the projectile position
            line[0] = Projectile.Center;

            // Update each point in the line 
            for (int j = 1; j < line.Count; j++)
            {
                // Previous position affects next position (continuity)
                Vector2 prevPos = line[j - 1];

                // Direction to target (magnetic pull)
                Vector2 dirToTarget = Vector2.Normalize(targetPos - prevPos);

                // Direction based on projectile facing (magnetic field direction)  
                Vector2 fieldDir = Projectile.rotation.ToRotationVector2().RotatedBy(baseAngle * (1f - (float)j / line.Count));

                // Combined direction with weights
                Vector2 direction = (fieldDir * (1f - (float)j / line.Count) +
                                   dirToTarget * ((float)j / line.Count) * magnetStrength);
                direction.Normalize();

                // Add some gravity effect for fluidity
                direction.Y += gravityStrength * ((float)j / line.Count);
                direction.Normalize();

                // Calculate new position
                float stepLength = 6f + (j * 0.5f); // Lines extend further as they progress  
                Vector2 newPos = prevPos + direction * stepLength;

                // Add some noise for a more fluid look
                newPos += Main.rand.NextVector2Circular(0.5f, 0.5f) * ((float)j / line.Count);

                line[j] = newPos;
            }
        }
    }

    private void ManageTrail()
    {
        UpdateFieldLines();

        // Use colors that suggest magnetism - blue/purple tones
        Color magnetColor1 = new Color(75, 105, 255); // Blue  
        Color magnetColor2 = new Color(180, 100, 255); // Purple

        if (trail == null || trail2 == null)
        {
            trail = new Trail(Main.instance.GraphicsDevice, POINTS_PER_LINE, new RoundedTip(8),
                factor => 25f * (1f - factor), // Thicker near the magnet, thinner at the ends
                factor => {
                    if (factor.X >= 0.98f) return Color.White * 0;
                    // Fade from blue to purple
                    return Color.Lerp(magnetColor1, magnetColor2, factor.X) * 0.5f * (1f - factor.X);
                });

            trail2 = new Trail(Main.instance.GraphicsDevice, POINTS_PER_LINE, new RoundedTip(8),
                factor => 15f * (1f - factor), // Inner trail is thinner  
                factor => {
                    if (factor.X >= 0.98f) return Color.White * 0;
                    // Brighter inner core
                    return Color.Lerp(Color.White, magnetColor1, factor.X * 0.5f) * 0.7f * (1f - factor.X);
                });
        }

        // Only update trail positions if we have valid field lines
        if (fieldLines != null && fieldLines.Count > 0)
        {
            // Alternate rendering different field lines 
            int lineToRender = (int)(Main.GameUpdateCount / 5) % fieldLines.Count;

            trail.Positions = fieldLines[lineToRender].ToArray();
            trail2.Positions = fieldLines[lineToRender].ToArray();

            // Set the next positions for smooth animation
            trail.NextPosition = fieldLines[lineToRender].Last();
            trail2.NextPosition = fieldLines[lineToRender].Last();
        }
    }

    public void DrawPrimitives()
    {
        var effect = ShaderLoader.GetShader("pixelTrail").Value;

        if (effect != null && fieldLines != null)
        {
            var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
            var view = Main.GameViewMatrix.TransformationMatrix;
            var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"]?.SetValue(Main.GameUpdateCount * 0.01f);
            effect.Parameters["repeats"]?.SetValue(5f);
            effect.Parameters["pixelation"]?.SetValue(3f); // Lower pixelation for smoother look
            effect.Parameters["resolution"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            effect.Parameters["transformMatrix"]?.SetValue(world * view * projection);
            effect.Parameters["sampleTexture"]?.SetValue(ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Star09").Value);

            // Draw all the field lines one after another
            for (int i = 0; i < fieldLines.Count; i++)
            {
                // Skip some frames for performance
                if (i % 2 == Main.GameUpdateCount % 2)
                {
                    trail.Positions = fieldLines[i].ToArray();
                    trail2.Positions = fieldLines[i].ToArray();

                    // Apply slight variations to make each line unique  
                    effect.Parameters["pixelation"]?.SetValue(3f + (i * 0.2f));
                    effect.Parameters["time"]?.SetValue(Main.GameUpdateCount * 0.05f + (i * 0.1f));

                    trail?.Render(effect);
                    trail2?.Render(effect);
                }
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
