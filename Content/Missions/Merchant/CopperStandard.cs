using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using Terraria.DataStructures;

namespace Reverie.Content.Missions.Merchant;

public class CopperStandard : Mission
{
    public CopperStandard() : base(MissionID.CopperStandard,
      "The Copper Standard",
      "'The Merchant believes copper is the cornerstone of civilization—durable, shiny, and universally appealing. " +
        "\nHe wants to build a proper “copper-backed economy”, and needs your help collecting resources.",
      [
        [("kill enemies for copper coins", 300)],
        [("Mine copper ore", 80)],
        [("Collect or smelt copper bars", 25)],
        [("Return to the Merchant", 1)]
      ],

      [new Item(ItemID.SilverCoin, Main.rand.Next(75, 150)), 
          new Item(ItemID.SilverWatch), new Item(ItemID.SilverBar, 20)],
      isMainline: false,
      NPCID.Merchant,
      xpReward: 150)
    {
        ModContent.GetInstance<Reverie>().Logger.Info("[Copper Standard] Mission constructed");
    }

    internal enum Objectives
    {
        KillCopperCoins = 0,
        MineCopper = 1,
        SmeltCopper = 2,
        ReturnToMerchant = 3
    }

    public override void Update()
    {
        base.Update();
    }

    public override void OnMissionStart()
    {
        base.OnMissionStart();
        DialogueManager.Instance.StartDialogueByKey(NPCManager.MerchantData, DialogueKeys.Merchant.CopperStandardStart,
            lineCount: 5, zoomIn: true);
    }

    public override void OnCollected(Item item)
    {
        try
        {
            var objective = (Objectives)CurrentIndex;
            switch (objective)
            {

                case Objectives.KillCopperCoins:
                    if (item.type == ItemID.CopperCoin && 
                        item.GetSource_FromThis() is EntitySource_Loot lootSource && lootSource.Entity is NPC)
                        UpdateProgress(0, item.stack);               
                break;
            case Objectives.MineCopper:
                    if (item.type == ItemID.CopperOre && item.GetSource_FromThis() is EntitySource_TileBreak)
                        UpdateProgress(0, item.stack);
                break;
            case Objectives.SmeltCopper:
                    if (item.type == ItemID.CopperBar)
                        UpdateProgress(0, item.stack);
                break;
            }
        }
        catch (Exception e)
        {
            Instance.Logger.Error($"[Copper Standard] Error in OnCollected: {e}");
        }
    }

    public override void OnMissionComplete(bool giveRewards = true)
    {
        base.OnMissionComplete(giveRewards);
        DialogueManager.Instance.StartDialogueByKey(
            NPCManager.MerchantData, DialogueKeys.Merchant.CopperStandardComplete, lineCount: 4, zoomIn: true);
    }

    public override void OnChat(NPC npc)
    {
        try
        {
            var objective = (Objectives)CurrentIndex;
            switch (objective)
            {
                case Objectives.KillCopperCoins:
                    if (Main.rand.NextBool(2))
                        DialogueManager.Instance.StartDialogueByKey(NPCManager.MerchantData,
                            DialogueKeys.Merchant.CopperCoinsInProgress, lineCount: 2);                
                    else
                        DialogueManager.Instance.StartDialogueByKey(NPCManager.MerchantData,
                            DialogueKeys.Merchant.CopperCoinsInProgress_Alt, lineCount: 2);
                    break;
                case Objectives.MineCopper:
                    DialogueManager.Instance.StartDialogueByKey(NPCManager.MerchantData,
                            DialogueKeys.Merchant.MineCopperInProgress, lineCount: 2);
                    break;
                case Objectives.SmeltCopper:
                    DialogueManager.Instance.StartDialogueByKey(NPCManager.MerchantData,
                       DialogueKeys.Merchant.SmeltCopperInProgress, lineCount: 5);
                    break;
            }
        }
        catch (Exception e)
        {
            Instance.Logger.Error($"[Copper Standard] Error in OnChat: {e}");
        }
    }

    public override void OnItemCreated(Item item, ItemCreationContext context)
    {
        try
        {
            var objective = (Objectives)CurrentIndex;
            switch (objective)
            {

                case Objectives.KillCopperCoins:
                    if (item.type == ItemID.CopperCoin)
                        UpdateProgress(0, item.stack);
                    break;
                case Objectives.SmeltCopper:
                    if (item.type == ItemID.CopperBar)
                        UpdateProgress(0, item.stack);
                    break;
            }
        }
        catch (Exception e)
        {
            Instance.Logger.Error($"[Copper Standard] Error in OnItemCreated: {e}");
        }
    }
}
