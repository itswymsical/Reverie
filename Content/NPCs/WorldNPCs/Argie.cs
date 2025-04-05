using Reverie.Common.Players;
using Reverie.Common.Systems;
using Reverie.Core.Cinematics.Cutscenes;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using Reverie.Core.NPCs.Actors;
using System.Linq;
using Terraria.DataStructures;

namespace Reverie.Content.NPCs.WorldNPCs;

[AutoloadHead]
public class Argie : WorldNPCActor
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        Main.npcFrameCount[Type] = 1;
    }

    public override void SetDefaults()
    {
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.immortal = true;
        NPC.width = 50;
        NPC.height = 74;
        NPC.aiStyle = 7;
        NPC.damage = 10;
        NPC.defense = 15;
        NPC.lifeMax = 250;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0f;
    }
    public override void OnSpawn(IEntitySource source)
    {
        base.OnSpawn(source);
        var player = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
        var bloomCap = player.GetMission(MissionID.BLOOMCAP);

        if (bloomCap != null &&
            bloomCap.Availability != MissionAvailability.Completed &&
            bloomCap.Progress != MissionProgress.Active)
        {
            player.UnlockMission(MissionID.BLOOMCAP);
        }
    }

    public override string GetChat()
    {
        return Main.rand.Next() switch
        {
            _ => "Hi!!! I'm Argie.",
        };
    }

    public override void SetChatButtons(ref string button, ref string button2)
    {
        button = "Chat";

        button2 = "Missions";
    }
    public override void OnChatButtonClicked(bool firstButton, ref string shopName)
    {
        if (firstButton)
        {
            DialogueManager.Instance.StartDialogueByKey(
                    NPCDataManager.Default,
                    DialogueKeys.ArgieDialogue.Introduction,
                    lineCount: 4,
                    zoomIn: false, musicId: MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}ArgiesTheme"));
        }
        else
        {
            var player = Main.LocalPlayer?.GetModPlayer<MissionPlayer>();
            if (player == null) return;

            var availableMissions = player.AvailableMissions();
            if (availableMissions == null) return;

            var missionId = availableMissions
                .FirstOrDefault(m => m.Employer == Type && m.Availability == MissionAvailability.Unlocked)
                ?.ID;

            if (missionId != null)
            {
                player.StartMission((int)missionId);
            }
        }
    }
}