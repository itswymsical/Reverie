using Reverie.Content.Items;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions;

namespace Reverie.Content.Missions.Argie;

public class BloomcapHunt : Mission
{
    public BloomcapHunt() : base(MissionID.BloomcapHunt,
      "BloomcapHunt Hunt",
      "'Argie wants a handful to decorate her stump.'",
      [
        [("Collect Bloomcaps", 8)],
        [("Return to Argie", 1)]
      ],

      [new Item(ItemID.SilverCoin, Main.rand.Next(50, 75)), new Item(ItemID.HealingPotion, 3), new Item(ItemID.PlatinumBar, 8)],
      isMainline: false,
      ModContent.NPCType<NPCs.WorldNPCs.Argie>(),
      xpReward: 100)
    {
        ModContent.GetInstance<Reverie>().Logger.Info("[BloomcapHunt Hunt] Mission constructed");
    }

    internal enum Objectives
    {
        CollectBloomcaps = 0,
        Return = 1,
    }
    public override void OnMissionStart()
    {
        base.OnMissionStart();

        DialogueManager.Instance.StartDialogueByKey(
            NPCManager.Default,
            DialogueKeys.ArgieDialogue.BloomcapIntro,
            lineCount: 5,
            zoomIn: true);
    }

    public override void OnMissionComplete(bool giveRewards = true)
    {
        base.OnMissionComplete(giveRewards);

        DialogueManager.Instance.StartDialogueByKey(
            NPCManager.Default,
            DialogueKeys.ArgieDialogue.BloomcapComplete,
            lineCount: 4,
            zoomIn: true);
    }
    protected override void OnObjectiveComplete(int objectiveIndex)
    {
        base.OnObjectiveComplete(objectiveIndex);
        if (objectiveIndex == 0)
        {
            DialogueManager.Instance.StartDialogueByKey(
                NPCManager.Default,
                DialogueKeys.ArgieDialogue.BloomcapCollectedAll,
                lineCount: 2,
                zoomIn: true);
        }
    }
    public override void OnCollected(Item item)
    {
        try
        {
            if ((Objectives)CurrentIndex == Objectives.CollectBloomcaps 
                && item.type == ModContent.ItemType<BloomcapItem>())
            {
                UpdateProgress(0, item.stack);
                if (Objective[CurrentIndex].Objectives[0].CurrentCount == 1)
                {
                    DialogueManager.Instance.StartDialogueByKey(
                        NPCManager.Default,
                        DialogueKeys.ArgieDialogue.BloomcapCollected,
                        lineCount: 2,
                        zoomIn: true);
                }

                if (Objective[CurrentIndex].Objectives[0].CurrentCount == 4)
                {
                    DialogueManager.Instance.StartDialogueByKey(
                        NPCManager.Default,
                        DialogueKeys.ArgieDialogue.BloomcapCollectedHalf,
                        lineCount: 1,
                        zoomIn: true);
                }
            }
        }
        catch (Exception ex)
        {
            Instance.Logger.Error($"Error in OnObjectiveComplete: {ex.Message}");
        }
    }

    public override void OnChat(NPC npc)
    {
        try
        {
            if ((Objectives)CurrentIndex == Objectives.Return 
                && npc.type == ModContent.NPCType<NPCs.WorldNPCs.Argie>())
            {
                UpdateProgress(0);
            }
        }
        catch (Exception ex)
        {
            Instance.Logger.Error($"Error in OnChat: {ex.Message}");
        }
    }
}