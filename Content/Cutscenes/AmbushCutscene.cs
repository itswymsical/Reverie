using Reverie.Content.NPCs.Enemies.Assailants;
using Reverie.Core.Cinematics;
using Reverie.Core.Cinematics.Camera;
using Reverie.Core.Cinematics.Music;
using Reverie.Core.Dialogue;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;

namespace Reverie.Content.Cutscenes;

public class AmbushCutscene : Cutscene
{
    #region Constants
    private const float SCENE1_DURATION = 1.0f;  // Sudden Stop
    private const float SCENE2_DURATION = 1.0f;  // Assailants Emerge

    private const float SPAWN_RADIUS = 400f;
    private const float CLOSE_RADIUS = 200f;
    private const int ASSAILANT_COUNT = 4;
    #endregion

    #region State
    private enum CutscenePhase
    {
        SuddenStop,
        Emergence,
        CircleCloses,
        GuideFreakout,
        CombatStart,
        End
    }

    private CutscenePhase currentPhase = CutscenePhase.SuddenStop;
    private float phaseTimer = 0f;
    #endregion

    #region Positions
    private Vector2 playerPos;
    private Vector2 cameraFocusPos;
    private NPC guide;
    private Vector2 guideStartPos;
    private Vector2 guideHidePos;
    private List<NPC> assailants = new();
    private List<Vector2> spawnPositions = new();
    private List<Vector2> closePositions = new();
    #endregion

    #region Visual State
    private bool hasPlayedWarningSound = false;
    private bool hasSpawnedAssailants = false;
    private float redVignetteAlpha = 0f;
    #endregion

    public override void Start()
    {
        playerPos = Main.LocalPlayer.Center;
        cameraFocusPos = playerPos;

        // Guide position
        guide = GetNPC(NPCID.Guide);
        if (guide != null)
        {
            guideStartPos = guide.Center;
            guideHidePos = playerPos + new Vector2(-100, 0); // Hide behind player
        }

        // Calculate spawn/close positions (circle around player)
        GetMobPositions();

        EnableLetterbox = true;
        base.Start();

        SetMusic(
            MusicLoader.GetMusicSlot($"{CUTSCENE_MUSIC_DIRECTORY}Tension"),
            MusicFadeMode.FadeIn,
            0.7f
        );
    }

    protected override void OnCutsceneStart()
    {
        // Lock player
        ControlsOFF();
        FreezePlayer();

        // Slow down camera zoom
        CameraSystem.LockCamera(cameraFocusPos);

        // Start with slight fade for dramatic effect
        FadeAlpha = 0.3f;
        FadeColor = Color.Black;
    }

    protected override void OnCutsceneUpdate(GameTime gameTime)
    {
        phaseTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        switch (currentPhase)
        {
            case CutscenePhase.SuddenStop:
                UpdateSuddenStop();
                break;
            case CutscenePhase.Emergence:
                UpdateEmergence();
                break;
            case CutscenePhase.End:
                UpdateEnd();
                break;
        }
    }

    #region Phase Updates
    private void UpdateSuddenStop()
    {
        // Camera pulls back slightly to show more area
        float progress = phaseTimer / SCENE1_DURATION;
        float zoom = MathHelper.Lerp(1f, 0.85f, EaseFunction.EaseQuadOut.Ease(progress));

        Vector2 pullBackPos = playerPos + new Vector2(0, -50); // Slight upward shift
        CameraSystem.LockCamera(Vector2.Lerp(playerPos, pullBackPos, progress));

        // Fade out the black overlay
        FadeAlpha = 0.3f * (1f - progress);

        // Warning sound
        if (!hasPlayedWarningSound && phaseTimer > 0.5f)
        {
            SoundEngine.PlaySound(SoundID.Roar with { Volume = 0.4f, Pitch = -0.3f });
            hasPlayedWarningSound = true;
        }

        if (phaseTimer >= SCENE1_DURATION)
        {
            currentPhase = CutscenePhase.Emergence;
            phaseTimer = 0f;
        }
    }

    private void UpdateEmergence()
    {
        // Spawn assailants if not already spawned
        if (!hasSpawnedAssailants)
        {
            SpawnAssailants();
            hasSpawnedAssailants = true;
        }

        // Assailants walk from spawn positions (slowly at first)
        float progress = phaseTimer / SCENE2_DURATION;
        float moveProgress = EaseFunction.EaseQuadIn.Ease(progress);

        for (int i = 0; i < assailants.Count; i++)
        {
            if (assailants[i] != null && assailants[i].active)
            {
                Vector2 currentPos = Vector2.Lerp(spawnPositions[i], closePositions[i], moveProgress * 0.3f);
                assailants[i].Center = currentPos;

                // Face toward player
                assailants[i].spriteDirection = assailants[i].Center.X < playerPos.X ? 1 : -1;
            }
        }

        // Red vignette intensifies
        redVignetteAlpha = progress * 0.4f;

        if (phaseTimer >= SCENE2_DURATION)
        {
            currentPhase = CutscenePhase.End;
            phaseTimer = 0f;
        }
    }

    private void UpdateEnd()
    {
        // Just wait for IsFinished to trigger
    }
    #endregion

    #region Helper Methods
    private void GetMobPositions()
    {
        spawnPositions.Clear();
        closePositions.Clear();

        for (int i = 0; i < 3; i++)
        {
            float angle = (MathHelper.TwoPi / 3) * i;

            // Spawn positions (far circle)
            Vector2 spawnOffset = new Vector2(
                (float)Math.Cos(angle) * SPAWN_RADIUS,
                (float)Math.Sin(angle) * SPAWN_RADIUS
            );
            spawnPositions.Add(playerPos + spawnOffset);

            // Close positions (near circle)
            Vector2 closeOffset = new Vector2(
                (float)Math.Cos(angle) * CLOSE_RADIUS,
                (float)Math.Sin(angle) * CLOSE_RADIUS
            );
            closePositions.Add(playerPos + closeOffset);
        }
    }

    private void SpawnAssailants()
    {
        assailants.Clear();

        for (int i = 0; i < spawnPositions.Count; i++)
        {
            int npcIndex = NPC.NewNPC(
                new EntitySource_Misc("Cutscene_Ambush"),
                (int)spawnPositions[i].X,
                (int)spawnPositions[i].Y,
                ModContent.NPCType<EchoKnight>()
            );

            if (npcIndex >= 0 && npcIndex < Main.maxNPCs)
            {
                NPC assailant = Main.npc[npcIndex];
                assailant.ai[0] = 1; // Set AI state to "cutscene mode" (you'd need to handle this in the NPC)
                assailant.velocity = Vector2.Zero;
                assailants.Add(assailant);
            }
        }

        SoundEngine.PlaySound(SoundID.DD2_BetsyFlyingCircleAttack with { Volume = 0.5f, Pitch = -0.5f }); // Ominous sound
    }
    #endregion

    protected override void DrawCutsceneContent(SpriteBatch spriteBatch)
    {
        // Draw red vignette for danger
        if (redVignetteAlpha > 0f)
        {
            DrawRedVignette(spriteBatch);
        }
    }

    private void DrawRedVignette(SpriteBatch spriteBatch)
    {
        Texture2D vignetteTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Vignette").Value;

        spriteBatch.End();
        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.Additive,
            SamplerState.LinearClamp,
            DepthStencilState.None,
            Main.Rasterizer,
            null,
            Main.UIScaleMatrix
        );

        spriteBatch.Draw(
            vignetteTexture,
            new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
            Color.Red * redVignetteAlpha
        );

        spriteBatch.End();
        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.LinearClamp,
            DepthStencilState.None,
            Main.Rasterizer,
            null,
            Main.UIScaleMatrix
        );
    }

    public override bool IsFinished()
    {
        return currentPhase == CutscenePhase.End;
    }

    public override void End()
    {
        // Clean up
        CameraSystem.UnlockCamera();
        ControlsON();

        // Unfreeze player
        var player = Main.LocalPlayer;
        player.velocity = Vector2.Zero;

        // Enable assailant AI (let them attack normally)
        foreach (var assailant in assailants)
        {
            if (assailant != null && assailant.active)
            {
                assailant.ai[0] = 0; // Reset to normal AI
            }
        }

        // Restore music to normal combat
        SetMusic(
            MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}Combat"),
            MusicFadeMode.FadeIn,
            1f
        );

        base.End();
    }
}