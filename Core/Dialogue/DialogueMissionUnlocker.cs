using Reverie.Core.Missions;

namespace Reverie.Core.Dialogue;

public class DialogueMissionUnlocker : ModSystem
{
    public override void PostSetupContent()
    {
        DialogueManager.OnDialogueEnd += HandleDialogueEnd;
    }

    public override void Unload()
    {
        DialogueManager.OnDialogueEnd -= HandleDialogueEnd;
    }

    private static void HandleDialogueEnd(string dialogueKey)
    {
        var missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();

        switch (dialogueKey)
        {
            case "JourneysBegin.Crash":
                missionPlayer.UnlockMission(MissionID.JourneysBegin);
                break;
        }
    }
}