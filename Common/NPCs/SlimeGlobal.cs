using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using static System.Net.Mime.MediaTypeNames;

namespace Reverie.Common.NPCs;

public class SlimeGlobal : GlobalNPC
{
    public override bool InstancePerEntity => true;
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCAIStyleID.Slime;

    #region Constants, Fields, Enums
    private Vector2 squishScale = Vector2.One;

    private bool isGrowing = false;
    private float growthTimer = 0f;
    private const float GROWTH_DURATION = 45f;
    internal enum SlimeState
    {
        Idle,
        PrepareSlamAttack,
        SlamAttack,
    }
    #endregion

    public override void SetDefaults(NPC npc)
    {
        if (npc.type != NPCAIStyleID.Slime) return;
        npc.HitSound = new SoundStyle($"{SFX_DIRECTORY}SlimeHit") with { Volume = 0.46f, PitchVariance = 0.3f, MaxInstances = 8 };
        npc.DeathSound = new SoundStyle($"{SFX_DIRECTORY}SlimeKilled") with { Volume = 0.76f, PitchVariance = 0.2f, MaxInstances = 8 };
    }

    public override void AI(NPC npc)
    {
        base.AI(npc);

        if (npc.aiStyle == NPCAIStyleID.Slime)
        {
            if (isGrowing)
            {
                growthTimer++;
                float progress = growthTimer / GROWTH_DURATION;

                npc.scale = MathHelper.Lerp(0.2f, 1.0f, progress);
                npc.alpha = (int)MathHelper.Lerp(125, 0, progress);

                if (Main.rand.NextBool(8))
                {
                    int dust = Dust.NewDust(npc.position, npc.width, npc.height,
                        DustID.t_Slime, 0f, 0f, 150, new Color(78, 136, 255, 150), 0.8f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 0.3f;
                }

                if (growthTimer >= GROWTH_DURATION)
                {
                    isGrowing = false;
                    npc.scale = 1.0f;
                    npc.alpha = 0;
                    npc.damage = 7;
                    npc.dontTakeDamage = false;
                    SoundEngine.PlaySound(SoundID.NPCHit1, npc.Center);
                }
            }

            if (!isGrowing || growthTimer > GROWTH_DURATION * 0.8f)
            {
                HandleSquishScale(npc);
            }
        }

    }
    
    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        base.OnSpawn(npc, source);

        if (npc.aiStyle == NPCAIStyleID.Slime && source is EntitySource_Parent parent &&
            parent.Entity is NPC parentNPC && parentNPC.type == NPCID.KingSlime)
        {
            npc.scale = 0.2f;
            npc.alpha = 125;
            npc.damage = 0;
            npc.dontTakeDamage = true;
            isGrowing = true;
            growthTimer = 0f;

            for (int i = 0; i < 8; i++)
            {
                Vector2 velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(0.5f, 2f);
                int dust = Dust.NewDust(npc.Center, 4, 4, DustID.t_Slime,
                    velocity.X, velocity.Y, 150, new Color(78, 136, 255, 150), 1f);
                Main.dust[dust].noGravity = true;
            }
        }
    }

    #region Visuals
    private void HandleSquishScale(NPC npc)
    {
        const float MAX_STRETCH = 1.5f;
        const float MIN_SQUISH = 0.7f;

        float yVelocityFactor = MathHelper.Clamp(npc.velocity.Y / 16f, -1f, 1f);

        if (npc.velocity.Y == 0f && npc.ai[0] >= -30f)
        {
            // Progressively squish horizontally and stretch vertically as it prepares to jump
            float preparationProgress = Math.Min(1f, (npc.ai[0] + 30f) / 30f);
            squishScale.X = MathHelper.Lerp(1f, 1.3f, preparationProgress);
            squishScale.Y = MathHelper.Lerp(1f, 0.92f, preparationProgress);
            return;
        }

        if (npc.velocity.Y != 0)
        {
            if (npc.velocity.Y < 0)
            {
                squishScale.Y = MathHelper.Lerp(1f, MAX_STRETCH, -yVelocityFactor);
                squishScale.X = MathHelper.Lerp(1f, MIN_SQUISH, -yVelocityFactor);
            }
            else
            {
                squishScale.Y = MathHelper.Lerp(1f, MIN_SQUISH, yVelocityFactor);
                squishScale.X = MathHelper.Lerp(1f, MAX_STRETCH, yVelocityFactor);
            }
        }
        else
        {
            squishScale = Vector2.Lerp(squishScale, Vector2.One, 0.15f);
        }
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (npc.type != NPCAIStyleID.Slime) return base.PreDraw(npc, spriteBatch, screenPos, drawColor);

        Texture2D texture = TextureAssets.Npc[npc.type].Value;

        int frameCount = 2;
        int frameHeight = texture.Height / frameCount;
        int frameWidth = texture.Width;

        if (npc.ai[0] == -999f)
        {
            npc.frame.Y = 0;
            npc.frameCounter = 0.0;
        }

        Rectangle sourceRectangle = new(0, npc.frame.Y, frameWidth, frameHeight);

        Color finalColor = npc.color;
        finalColor.R = (byte)((finalColor.R * drawColor.R) / 255);
        finalColor.G = (byte)((finalColor.G * drawColor.G) / 255);
        finalColor.B = (byte)((finalColor.B * drawColor.B) / 255);
        finalColor.A = npc.color.A;

        Vector2 drawPos = npc.Center - Main.screenPosition;
        SpriteEffects spriteEffects = npc.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        // Calculate rotation based on X velocity when falling
        float spriteRotation = 0f;
        if (npc.velocity.Y > 0f)
        {
            // Scale rotation with X velocity, cap at ±π/2 (90 degrees)
            spriteRotation = MathHelper.Clamp(npc.velocity.X * 0.04f, -MathHelper.PiOver2, MathHelper.PiOver2);
        }

        Main.EntitySpriteDraw(
            texture,
            drawPos,
            sourceRectangle,
            finalColor,
            spriteRotation,
            new Vector2(frameWidth / 2, frameHeight / 2),
            squishScale * npc.scale,
            spriteEffects
        );

        return false;
    }
    #endregion
}