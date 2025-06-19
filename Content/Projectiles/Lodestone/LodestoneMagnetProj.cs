using ReLogic.Utilities;
using Reverie.Core.Graphics;
using Reverie.Core.Interfaces;
using Reverie.Core.Loaders;
using Reverie.Utilities;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.Map;


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
    private Color vacuumColor1 = new(150, 150, 150);
    private Color currentTargetColor = new(255, 100, 150); // Dynamic target color that changes based on tile
    private readonly Vector2 Size = new(100, 50);

    // Sound tracking
    private SlotId startupSoundId = SlotId.Invalid;
    private SlotId loopSoundId = SlotId.Invalid;
    private bool hasPlayedStartup = false;
    private bool hasStartedLoop = false;
    private int soundTimer = 0;

    // Sine wave parameters
    private readonly int SINE_POINTS = 30;
    private float sineAmplitude = 25f;
    private float sineFrequency = 0.8f;
    private float animationSpeed = 0.15f;
    private List<Vector2> sineTrailPoints;

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
        HandleSounds();
        UpdateTargetColor();

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

    private static void BreakTile(int x, int y, Player player)
    {

        player.PickTile(x, y, 10);
        if (Main.netMode is NetmodeID.MultiplayerClient)
        {
            NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, x, y);
        }
    }

    #region Helper Methods
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

    private void HandleSounds()
    {
        if (Projectile.owner != Main.myPlayer) return;

        soundTimer++;

        if (!hasPlayedStartup)
        {
            var startupStyle = new SoundStyle($"{SFX_DIRECTORY}OreVacuum_Start")
            {
                Volume = 0.8f,
                MaxInstances = 1,
                SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest
            };

            startupSoundId = SoundEngine.PlaySound(startupStyle, Projectile.Center);
            hasPlayedStartup = true;
        }

        int startupDurationTicks = 77;

        if (hasPlayedStartup && !hasStartedLoop && soundTimer >= startupDurationTicks)
        {
            var loopStyle = new SoundStyle($"{SFX_DIRECTORY}OreVacuum_Loop")
            {
                Volume = 0.6f,
                IsLooped = true,
                MaxInstances = 1,
                SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest
            };

            loopSoundId = SoundEngine.PlaySound(loopStyle, Projectile.Center);
            hasStartedLoop = true;
        }

        if (hasStartedLoop && SoundEngine.TryGetActiveSound(loopSoundId, out var activeSound))
        {
            if (activeSound != null)
            {
                activeSound.Position = Projectile.Center;
            }
        }
    }

    public override void OnKill(int timeLeft)
    {
        if (Projectile.owner == Main.myPlayer)
        {
            if (SoundEngine.TryGetActiveSound(loopSoundId, out var loopSound))
            {
                loopSound?.Stop();
            }

            var endStyle = new SoundStyle($"{SFX_DIRECTORY}OreVacuum_End")
            {
                Volume = 0.7f,
                MaxInstances = 1
            };

            SoundEngine.PlaySound(endStyle, Projectile.Center);
        }

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

    private void SetOwnerAnimation(Player owner)
    {
        owner.ChangeDir(Projectile.direction);
        owner.heldProj = Projectile.whoAmI;
        owner.itemTime = 2;
        owner.itemAnimation = 2;

        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (owner.Center - Main.MouseWorld).ToRotation() + MathHelper.PiOver2);
    }
    #endregion

    #region Color Helper Methods
    private void UpdateTargetColor()
    {
        Vector2 mousePos = Main.MouseWorld;
        int tileX = (int)(mousePos.X / 16f);
        int tileY = (int)(mousePos.Y / 16f);

        if (tileX >= 0 && tileX < Main.maxTilesX && tileY >= 0 && tileY < Main.maxTilesY)
        {
            Tile tile = Main.tile[tileX, tileY];
            Color targetColor = new Color(135, 206, 235);

            if (tile != null && tile.HasTile)
            {
                targetColor = GetTileMapColor(tile.TileType, tileX, tileY);
                targetColor = EnhanceColor(targetColor);
            }
            else if (tile != null && tile.WallType > 0)
            {
                targetColor = GetWallMapColor(tile.WallType);
                targetColor = EnhanceColor(targetColor);
            }

            float lerpSpeed = 0.1f;
            currentTargetColor = Color.Lerp(currentTargetColor, targetColor, lerpSpeed);
        }
    }

    private Color GetTileMapColor(int tileType, int x, int y)
    {
        try
        {
            MapTile mapTile = Main.Map[x, y];

            Color mapColor = MapHelper.GetMapTileXnaColor(ref mapTile);

            if (mapColor != Color.Black && mapColor != Color.Transparent &&
                (mapColor.R > 0 || mapColor.G > 0 || mapColor.B > 0))
            {
                return mapColor;
            }

            ModTile modTile = TileLoader.GetTile(tileType);
            if (modTile != null)
            {
                return GetFallbackTileColor(tileType);
            }
        }
        catch
        {

        }

        return GetFallbackTileColor(tileType);
    }

    private Color GetFallbackTileColor(int tileType)
    {
        // Provide nice fallback colors for common tiles
        return tileType switch
        {
            TileID.Dirt => new Color(139, 91, 71),
            TileID.Stone => new Color(128, 128, 128),
            TileID.Grass => new Color(71, 133, 71),
            TileID.Iron => new Color(205, 127, 50),
            TileID.Copper => new Color(184, 115, 51),
            TileID.Gold => new Color(255, 215, 0),
            TileID.Silver => new Color(192, 192, 192),
            TileID.Sand => new Color(255, 218, 143),
            TileID.WoodBlock => new Color(150, 111, 51),
            TileID.CorruptGrass => new Color(104, 86, 164),
            TileID.CrimsonGrass => new Color(200, 45, 85),
            TileID.HallowedGrass => new Color(120, 185, 225),
            TileID.Tin => new Color(145, 145, 105),
            TileID.Lead => new Color(85, 89, 118),
            TileID.Tungsten => new Color(132, 220, 85),
            TileID.Platinum => new Color(154, 192, 220),
            TileID.Demonite => new Color(123, 97, 163),
            TileID.Crimtane => new Color(200, 45, 85),
            TileID.Meteorite => new Color(95, 61, 53),
            TileID.Obsidian => new Color(49, 40, 58),
            TileID.Hellstone => new Color(200, 89, 89),
            TileID.Cobalt => new Color(43, 109, 167),
            TileID.Mythril => new Color(89, 140, 89),
            TileID.Adamantite => new Color(214, 109, 109),
            TileID.Chlorophyte => new Color(128, 200, 89),
            _ => new Color(100, 149, 237) // air
        };
    }

    private Color GetWallMapColor(int wallType)
    {
        try
        {
            return wallType switch
            {
                WallID.DirtUnsafe => new Color(139, 91, 71),
                WallID.Stone => new Color(100, 100, 100),
                WallID.Wood => new Color(120, 91, 51),
                WallID.Grass => new Color(51, 113, 51),
                WallID.EbonstoneUnsafe => new Color(84, 66, 144),
                WallID.CrimstoneUnsafe => new Color(180, 25, 65),
                WallID.PearlstoneBrickUnsafe => new Color(100, 165, 205),
                _ => new Color(64, 64, 64)
            };
        }
        catch
        {

        }

        return new Color(64, 64, 64);
    }

    private Color EnhanceColor(Color originalColor)
    {
        Vector3 hsv = Main.rgbToHsl(originalColor);

        hsv.Y = Math.Max(hsv.Y, 0.6f);
        hsv.Z = Math.Max(hsv.Z, 0.7f);

        Color enhanced = Main.hslToRgb(hsv);
        enhanced.A = 255;

        return enhanced;
    }
    #endregion

    #region Rendering
    private void UpdateSineTrail()
    {
        if (sineTrailPoints == null)
        {
            sineTrailPoints = new List<Vector2>();
        }

        sineTrailPoints.Clear();

        Vector2 startPos = Projectile.Center;
        Vector2 endPos = Main.MouseWorld;
        Vector2 direction = Vector2.Normalize(endPos - startPos);
        Vector2 perpendicular = new Vector2(-direction.Y, direction.X); // Perpendicular for sine wave oscillation

        float totalDistance = Vector2.Distance(startPos, endPos);
        totalDistance = Math.Min(totalDistance, MAGNET_RANGE); // Clamp to max range

        float timeOffset = Main.GameUpdateCount * animationSpeed;

        for (int i = 0; i < SINE_POINTS; i++)
        {
            float progress = (float)i / (SINE_POINTS - 1);

            // Base position along the line from projectile to mouse
            Vector2 basePos = Vector2.Lerp(startPos, startPos + direction * totalDistance, progress);

            // Calculate sine wave offset
            float sineInput = progress * sineFrequency * MathHelper.TwoPi + timeOffset;
            float sineValue = (float)Math.Sin(sineInput);

            // Create vacuum effect: amplitude INCREASES as we get closer to the target
            float vacuumAmplitude = sineAmplitude * (0.2f + progress * 1.2f); // Start small, get bigger toward end

            // Add animated pulsing effect
            float pulse = 1f + 0.3f * (float)Math.Sin(timeOffset * 2f);
            vacuumAmplitude *= pulse;

            // Apply sine wave perpendicular to the main direction
            Vector2 offset = perpendicular * sineValue * vacuumAmplitude;

            Vector2 finalPos = basePos + offset;
            sineTrailPoints.Add(finalPos);
        }
    }

    private void ManageCaches()
    {
        cache = [Projectile.Center];
        UpdateSineTrail();

        while (cache.Count > 20)
        {
            cache.RemoveAt(0);
        }
    }

    private void ManageTrail()
    {
        if (trail == null)
        {
            trail = new Trail(Main.instance.GraphicsDevice, SINE_POINTS, new RoundedTip(8),
                factor => {
                    // Thickness increases from start to end for vacuum effect
                    float thickness = MathHelper.Lerp(4f, 46f, factor); // Start thin (4px), end thick (25px)
                    return thickness;
                },
                factor => {
                    // Dynamic color based on position along trail and target tile
                    float progress = factor.X; // 0 = start, 1 = end
                    if (factor.X >= 0.98f) return Color.Transparent;
                    // Color lerp from vacuum base color to target tile color
                    Color lerpedColor = Color.Lerp(vacuumColor1, currentTargetColor, progress);

                    // Add some animated intensity
                    float intensity = 0.8f + 0.2f * (float)Math.Sin(Main.GameUpdateCount * 0.2f + progress * 3f);

                    return lerpedColor * intensity;
                }
            );
        }

        if (sineTrailPoints != null && sineTrailPoints.Count > 0)
        {
            trail.Positions = sineTrailPoints.ToArray();
            trail.NextPosition = sineTrailPoints[^1];
        }
    }

    public void DrawPrimitives()
    {
        var effect = ShaderLoader.GetShader("pixelTrail").Value;

        if (effect != null && trail != null)
        {
            var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
            var view = Main.GameViewMatrix.TransformationMatrix;
            var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"]?.SetValue(Main.GameUpdateCount * 0.9f);
            effect.Parameters["repeats"]?.SetValue(3f);
            effect.Parameters["pixelation"]?.SetValue(2f);
            effect.Parameters["resolution"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            effect.Parameters["transformMatrix"]?.SetValue(world * view * projection);
            effect.Parameters["sampleTexture"]?.SetValue(ModContent.Request<Texture2D>($"{VFX_DIRECTORY}RibbonTrail").Value);

            trail?.Render(effect);
        }
    }
    #endregion
}