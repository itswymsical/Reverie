using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria.UI;

namespace Reverie.Common.Systems.Particles;

public abstract class Particle
{
    public Vector2 position;
    public Vector2 velocity;
    public float alpha;
    public float scale;
    public float lifetime;
    public float maxLifetime;
    public bool active;

    protected const float VELOCITY_DAMPING = 0.98f;

    public virtual void Initialize(Vector2 startPos)
    {
        position = startPos;
        lifetime = 0f;
        active = true;
    }

    public virtual void Update()
    {
        if (!active) return;

        lifetime++;
        UpdateBehavior();
        ApplyPhysics();

        if (lifetime >= maxLifetime)
            active = false;
    }

    protected virtual void UpdateBehavior() { }

    protected virtual void ApplyPhysics()
    {
        velocity *= VELOCITY_DAMPING;
        position += velocity;
    }

    public virtual void Reset()
    {
        active = false;
        alpha = 0f;
        lifetime = 0f;
        velocity = Vector2.Zero;
        scale = 1f;
        position = Vector2.Zero;
    }
}

public abstract class ParticleManager<T> : ModSystem where T : Particle, new()
{
    protected List<T> particlePool;
    protected Texture2D particleTexture;
    protected int maxParticles;

    protected abstract string TexturePath { get; }
    protected abstract int MaxParticles { get; }
    protected virtual BlendState ParticleBlendState => BlendState.AlphaBlend;

    public override void PostSetupContent()
    {
        maxParticles = MaxParticles;
        particlePool = new List<T>(maxParticles);

        for (int i = 0; i < maxParticles; i++)
            particlePool.Add(new T());

        var texturePath = TexturePath;
        if (!ModContent.HasAsset(texturePath))
        {
            Main.NewText($"[{GetType().Name}] Texture not found: {texturePath}", Color.Red);
            return;
        }

        particleTexture = ModContent.Request<Texture2D>(texturePath, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

        if (particleTexture == null)
        {
            Main.NewText($"[{GetType().Name}] Failed to load texture: {texturePath}", Color.Red);
        }
    }

    public override void PostUpdateEverything()
    {
        if (Main.gameMenu || Main.LocalPlayer == null) return;

        UpdateParticles();

        if (ShouldSpawnParticles())
            SpawnParticles();
    }

    protected virtual void UpdateParticles()
    {
        for (int i = 0; i < particlePool.Count; i++)
        {
            var particle = particlePool[i];
            if (particle.active)
                particle.Update();
        }
    }

    protected abstract bool ShouldSpawnParticles();
    protected abstract void SpawnParticles();

    protected T GetInactiveParticle()
    {
        return particlePool.FirstOrDefault(p => !p.active);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        if (Main.gameMenu || particleTexture == null) return;

        int invasionIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Invasion Progress Bars"));
        if (invasionIndex != -1)
        {
            layers.Insert(invasionIndex + 1, new LegacyGameInterfaceLayer(
                "Reverie: Particles",
            delegate {
                    DrawParticleSystem(Main.spriteBatch);
                    return true;
                },
                InterfaceScaleType.Game)
            );
        }
    }

    protected virtual void DrawParticleSystem(SpriteBatch spriteBatch)
    {
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, ParticleBlendState, SamplerState.PointClamp,
                         DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

        DrawParticles(spriteBatch);

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                         DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
    }

    protected virtual void DrawParticles(SpriteBatch spriteBatch)
    {
        Vector2 screenPos = Main.screenPosition;
        Rectangle screenBounds = new Rectangle((int)screenPos.X - 50, (int)screenPos.Y - 50,
                                             Main.screenWidth + 100, Main.screenHeight + 100);

        for (int i = 0; i < particlePool.Count; i++)
        {
            var particle = particlePool[i];
            if (!particle.active) continue;

            if (!screenBounds.Contains((int)particle.position.X, (int)particle.position.Y))
                continue;

            DrawParticle(spriteBatch, particle, screenPos);
        }
    }

    protected virtual void DrawParticle(SpriteBatch spriteBatch, T particle, Vector2 screenPos)
    {
        Vector2 drawPos = particle.position - screenPos;
        Color drawColor = Color.White * (particle.alpha / 255f);

        spriteBatch.Draw(particleTexture, drawPos, null, drawColor,
                       0f, Vector2.Zero, particle.scale, SpriteEffects.None, 0f);
    }

    public int ActiveParticleCount => particlePool.Count(p => p.active);

    public void ClearAllParticles()
    {
        foreach (var particle in particlePool)
            particle.Reset();
    }
}