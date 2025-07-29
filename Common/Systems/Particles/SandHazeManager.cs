using Reverie.Content.Particles;
using Reverie.Content.Tiles.Archaea;
using System.Linq;
using Terraria.GameContent;

namespace Reverie.Common.Systems.Particles;

public class SandHazeManager : ParticleManager<SandHazeParticle>
{
    private static SandHazeManager _instance;
    public static SandHazeManager Instance => _instance;

    private SandHazeConfig config;
    private int frameCounter = 0;

    protected override string TexturePath => $"{TEXTURE_DIRECTORY}Particles/SandHaze";
    protected override int MaxParticles => 500;

    protected override BlendState ParticleBlendState => new BlendState
    {
        ColorBlendFunction = BlendFunction.Add,
        ColorSourceBlend = Blend.SourceAlpha,
        ColorDestinationBlend = Blend.DestinationAlpha, // Additive blending for brightness
        AlphaBlendFunction = BlendFunction.Add,
        AlphaSourceBlend = Blend.Zero,
        AlphaDestinationBlend = Blend.One
    };

    public override void PostSetupContent()
    {
        base.PostSetupContent();
        _instance = this;
        config = ModContent.GetInstance<SandHazeConfig>();

        if (particleTexture == null)
        {
            Main.NewText("[SandHaze] Texture is null!", Color.Red);
        }
        else
        {
            Main.NewText($"[SandHaze] Texture loaded: {particleTexture.Width}x{particleTexture.Height}", Color.Green);

            if (particleTexture.Width < 500 || particleTexture.Height < 115)
            {
                Main.NewText($"[SandHaze] Texture too small for frames! Expected 500x115, got {particleTexture.Width}x{particleTexture.Height}", Color.Orange);
            }
        }
    }

    protected override void DrawParticleSystem(SpriteBatch spriteBatch)
    {
        var blendState = new BlendState
        {
            ColorBlendFunction = BlendFunction.Add,
            ColorSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.DestinationAlpha, // Additive blending for brightness
            AlphaBlendFunction = BlendFunction.Add,
            AlphaSourceBlend = Blend.Zero,
            AlphaDestinationBlend = Blend.One
        };

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, blendState, SamplerState.PointClamp,
                         DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

        DrawSandHazeParticles(spriteBatch);

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                         DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
    }

    private void DrawSandHazeParticles(SpriteBatch spriteBatch)
    {
        if (particleTexture == null) return;

        var screenPos = Main.screenPosition;
        var screenBounds = new Rectangle((int)screenPos.X - 50, (int)screenPos.Y - 50,
                                         Main.screenWidth + 100, Main.screenHeight + 100);

        for (var i = 0; i < particlePool.Count; i++)
        {
            var particle = particlePool[i];
            if (!particle.active) continue;

            if (!screenBounds.Contains((int)particle.position.X, (int)particle.position.Y))
                continue;

            var drawPos = particle.position - screenPos;

            // Sample world lighting at particle position
            var tileX = (int)(particle.position.X / 16f);
            var tileY = (int)(particle.position.Y / 16f);
            var worldLight = Lighting.GetColor(tileX, tileY);

            // Blend sandy color with world lighting
            var sandColor = new Color(255, 245, 200); // Warm sandy yellow
            var litSandColor = new Color(
                (int)(sandColor.R * worldLight.R / 255f),
                (int)(sandColor.G * worldLight.G / 255f),
                (int)(sandColor.B * worldLight.B / 255f)
            );

            var drawColor = litSandColor * (particle.alpha / 255f);

            // Use the particle's frame data for proper sprite sheet rendering
            spriteBatch.Draw(particleTexture, drawPos, particle.frame, drawColor,
                           0f, Vector2.Zero, particle.scale, SpriteEffects.None, 0f);
        }
    }

    protected override bool ShouldSpawnParticles()
    {
        if (config == null)
            config = ModContent.GetInstance<SandHazeConfig>();

        var player = Main.LocalPlayer;
        var inSandArea = player.ZoneDesert || SubworldLibrary.SubworldSystem.IsActive<Subworlds.Archaea.ArchaeaSub>();

        return inSandArea && config.EffectiveEnableSandHaze;
    }

    protected override void SpawnParticles()
    {
        var player = Main.LocalPlayer;
        SpawnSandHaze(player);
    }

    private void SpawnSandHaze(Player player)
    {
        var startX = (int)(player.position.X / 16) - config.EffectiveHorizontalRange;
        var endX = (int)(player.position.X / 16) + config.EffectiveHorizontalRange;
        var startY = (int)(player.position.Y / 16) - config.EffectiveVerticalRange;
        var endY = (int)(player.position.Y / 16) + config.EffectiveVerticalRange;

        var spawnedThisFrame = 0;

        for (var x = startX; x < endX; x++)
        {
            for (var y = startY; y < endY; y++)
            {
                if (x < 0 || x >= Main.maxTilesX || y < 2 || y >= Main.maxTilesY)
                    continue;

                if (IsSandTile(Main.tile[x, y]) && HasAirAbove(x, y) && Main.rand.NextBool(config.EffectiveDustSpawnChance))
                {
                    SpawnSandDust(x, y, player);
                    spawnedThisFrame++;
                }
            }
        }
    }

    private bool IsSandTile(Tile tile) => tile.HasTile && (
        tile.TileType == TileID.Sand ||
        tile.TileType == TileID.Ebonsand ||
        tile.TileType == TileID.Crimsand ||
        tile.TileType == TileID.Pearlsand ||
        tile.TileType == ModContent.TileType<PrimordialSandTile>()
    );

    private bool HasAirAbove(int x, int y) =>
        !Main.tile[x, y - 1].HasTile &&
        !Main.tile[x, y - 2].HasTile; // Check 2 tiles up for good clearance

    private void SpawnSandDust(int tileX, int tileY, Player player)
    {
        var particle = GetInactiveParticle();
        if (particle == null)
        {
            if (frameCounter % 60 == 0)
                Main.NewText("[SandHaze] No inactive particles available!", Color.Orange);
            return;
        }

        // Spawn particles on the tile surface with horizontal spread to avoid clustering
        var dustPosition = new Vector2(tileX * 16, (tileY - 1) * 16); // Just above the tile

        // Add horizontal spread to distribute particles better
        float horizontalSpread = Main.rand.NextFloat(-24f, 40f); // Spread across ~4 tiles
        dustPosition.X += horizontalSpread;
        dustPosition.Y += Main.rand.NextFloat(0f, 8f); // Small vertical variation

        particle.Initialize(dustPosition);
        particle.velocity.X -= Main.windSpeedCurrent * config.EffectiveWindVelocityFactor;

        // Add some initial horizontal drift to spread particles out
        particle.velocity.X += Main.rand.NextFloat(-0.02f, 0.02f);

        if (player.ZoneSandstorm)
        {
            float sandstormVel = config.EffectiveSandstormUpwardVelocity;
            particle.velocity.Y += sandstormVel;
            // Track sandstorm influence for quicker fade
            particle.yVelInfluence += Math.Abs(sandstormVel) * 1.5f;
        }
    }
}