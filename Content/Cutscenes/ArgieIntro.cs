using Reverie.Common.Systems;
using Reverie.Content.NPCs.Special;
using Reverie.Core.Cinematics;
using Reverie.Core.Cinematics.Camera;
using Reverie.Core.Cinematics.Music;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using Reverie.Core.Missions.Core;

namespace Reverie.Content.Cutscenes;

public class ArgieIntroCutscene : Cutscene
{
    private float fadeInDuration = 2 * 60f;
    private float cutsceneDuration = 5f;

    private NPC argieNPC;
    private Vector2 originalCameraPos;

    public override void Start()
    {
        base.Start();
        SetMusic(MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}ArgiesTheme"), MusicFadeMode.CrossFade, 7);

        argieNPC = FindArgieNPC();
        if (argieNPC != null)
        {
            originalCameraPos = argieNPC.Center;
        }
    }

    private NPC FindArgieNPC()
    {
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (npc.active && npc.ModNPC?.Type == ModContent.NPCType<Argie>())
            {
                return npc;
            }
        }
        return null;
    }

    protected override void OnCutsceneStart()
    {
        FadeAlpha = 1f;
        FadeColor = Color.Black;


        if (argieNPC != null)
        {
            // Position camera slightly away from Argie
            Vector2 startPos = argieNPC.Center + new Vector2(70, 0);
            CameraSystem.MoveCameraOut(1, startPos);
        }
    }

    protected override void OnCutsceneUpdate(GameTime gameTime)
    {
        FadeIn(fadeInDuration);

        if (argieNPC == null) return;
    }


    protected override void DrawCutsceneContent(SpriteBatch spriteBatch)
    {

    }

    public override bool IsFinished()
    {
        return ElapsedSeconds >= cutsceneDuration;
    }

    protected override void OnCutsceneEnd()
    {
        DownedSystem.argieCutscene = true;

        CameraSystem.ReturnCamera(60);
        ControlsON();

        DialogueManager.Instance.StartDialogue("Argie.Intro", 9, letterbox: true, music: MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}ArgiesTheme"));
        MissionPlayer player = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
        player.UnlockMission(MissionID.SporeSplinter);
    }
}