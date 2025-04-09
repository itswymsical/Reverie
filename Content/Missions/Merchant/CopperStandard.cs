using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using Terraria.DataStructures;

namespace Reverie.Content.Missions.Merchant;

public class CopperStandard : Mission
{
    public CopperStandard() : base(MissionID.CopperStandard,
      "The Copper Standard",
      @"""Copper's the cornerstone of civilization-durable! Plus, it's shiny, and appealing.""",
      [
        [("Collect copper coins", 800)],
        [("Mine copper ore", 80)],
        [("Collect or smelt copper bars", 25)],
        [("Return to the Merchant", 1)]
      ],

      rewards: [new Item(ItemID.SilverCoin, Main.rand.Next(75, 150)),
          new Item(ItemID.SilverWatch), new Item(ItemID.SilverBar, 20)],
      isMainline: false,
      providerNPC: NPCID.Merchant,
      xpReward: 150)
    {
        ModContent.GetInstance<Reverie>().Logger.Info("[Copper Standard] Mission constructed");
    }

    internal enum Objectives
    {
        CopperCoins = 0,
        MineCopper = 1,
        SmeltCopper = 2,
        ReturnToMerchant = 3
    }

    #region Event Registration

    protected override void RegisterEventHandlers()
    {
        if (eventsRegistered) return;

        base.RegisterEventHandlers();

        // Register only the events this mission cares about
        ObjectiveEventItem.OnItemPickup += OnItemPickupHandler;
        ObjectiveEventItem.OnItemCreated += OnItemCreatedHandler;
        ObjectiveEventNPC.OnNPCChat += OnNPCChatHandler;

        ModContent.GetInstance<Reverie>().Logger.Debug($"[Copper Standard] Registered event handlers");
        eventsRegistered = true;
    }

    protected override void UnregisterEventHandlers()
    {
        if (!eventsRegistered) return;

        // Unregister all event handlers
        ObjectiveEventItem.OnItemPickup -= OnItemPickupHandler;
        ObjectiveEventItem.OnItemCreated -= OnItemCreatedHandler;
        ObjectiveEventNPC.OnNPCChat -= OnNPCChatHandler;

        ModContent.GetInstance<Reverie>().Logger.Debug($"[Copper Standard] Unregistered event handlers");
        base.UnregisterEventHandlers();
    }

    #endregion

    #region Event Handlers

    private void OnItemPickupHandler(Item item, Player player)
    {
        if (Progress != MissionProgress.Active) return;

        try
        {
            var objective = (Objectives)CurrentIndex;
            switch (objective)
            {
                case Objectives.CopperCoins:
                    if (item.type == ItemID.CopperCoin)
                        UpdateProgress(0, item.stack);
                    break;
                case Objectives.MineCopper:
                    if (item.type == ItemID.CopperOre)
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
            Instance.Logger.Error($"[Copper Standard] Error in OnItemPickupHandler: {e}");
        }
    }

    private void OnItemCreatedHandler(Item item, ItemCreationContext context)
    {
        if (Progress != MissionProgress.Active) return;

        try
        {
            var objective = (Objectives)CurrentIndex;
            switch (objective)
            {
                case Objectives.CopperCoins:
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
            Instance.Logger.Error($"[Copper Standard] Error in OnItemCreatedHandler: {e}");
        }
    }

    private void OnNPCChatHandler(NPC npc, ref string chat)
    {
        if (Progress != MissionProgress.Active) return;

        try
        {
            var objective = (Objectives)CurrentIndex;
            if (npc.type == ProviderNPC)
            {
                switch (objective)
                {
                    case Objectives.CopperCoins:
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
                    case Objectives.ReturnToMerchant:
                        UpdateProgress(0);
                        break;
                }
            }
        }
        catch (Exception e)
        {
            Instance.Logger.Error($"[Copper Standard] Error in OnNPCChatHandler: {e}");
        }
    }
    #endregion

    public override void Update()
    {
        base.Update();
    }

    public override void OnMissionStart()
    {
        base.OnMissionStart(); // This now calls RegisterEventHandlers()
        DialogueManager.Instance.StartDialogueByKey(NPCManager.MerchantData, DialogueKeys.Merchant.CopperStandardStart,
            lineCount: 5, zoomIn: true);
    }

    public override void OnMissionComplete(bool giveRewards = true)
    {
        base.OnMissionComplete(giveRewards); // This now calls UnregisterEventHandlers()
        DialogueManager.Instance.StartDialogueByKey(
            NPCManager.MerchantData, DialogueKeys.Merchant.CopperStandardComplete, lineCount: 4, zoomIn: true);
    }
}