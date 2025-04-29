using Reverie.Common.Systems;
using Reverie.Common.Systems.Camera;
using Reverie.Content.Cutscenes;
using Reverie.Content.Items;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using Reverie.Utilities;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;

namespace Reverie.Content.Missions;

public class AFallingStar : Mission
{
    private int reverieSFXtimer = 0;
    private int nextChimeTime = 0;
    private int potTileBreakCounter = 0;
    private bool specialItemFound = false;
    private enum Objectives
    {
        TalkToTownies = 0,
        GatherResources = 1,
        AquireItems = 2,
        ExploreBiomes = 3,
        ClearSlimes = 4,
        DefendTown = 5,
        ClearSlimeRain = 6,
        DefeatKingSlime = 7
    }

    public AFallingStar() : base(MissionID.AFallingStar,
      "Falling Star...",
      @"""Well, that's one way to make an appearance..."""
      + "\nBegin your Journey, exploring Terraria",
      [
        [("Talk to Guide", 1), ("Talk to Merchant", 1), ("Talk to Demolitionist", 1), ("Talk to Nurse", 1)],

        [("Gather Wood", 50), ("Break Pots", 20)],

        [("Harvest Ore", 30), ("Discover accessories", 2)],

        [("Explore the Underground", 1), ("Check in with the Guide", 1),
        ("Discover a Glowing Mushroom Biome", 1), ("Explore the Jungle", 1),
        ("Explore the Underground Desert", 1),  ("Explore the Tundra", 1)],

        //[("Find Stillspire Outpost", 1)],

        //TODO: Mission objectives tied to the Archiver Chronicles, Reverie, and Guide's research.

        [("Mission still WIP (for now mess around with stuff)", 10)],

        [("Defend the Town", 10)],

        [("Clear slime infestation", 100)],

        [("Defeat King Slime", 1)]
      ],

      [new Item(ItemID.RegenerationPotion),
          new Item(ItemID.IronskinPotion),
          new Item(ItemID.GoldCoin, Main.rand.Next(4, 6))],
      isMainline: true,
      NPCID.Guide,
      xpReward: 100)
    {
        Instance.Logger.Info("[A Falling Star] Mission constructed");
    }

    private readonly List<Item> CopperItems =
    [
        new Item(ItemID.CopperShortsword),
        new Item(ItemID.CopperPickaxe),
        new Item(ItemID.CopperAxe)
    ];

    public override void OnMissionStart()
    {
        base.OnMissionStart(); // This now calls RegisterEventHandlers()

        SetObjectiveVisibility((int)Objectives.ExploreBiomes, 1, false);

        CutsceneSystem.PlayCutscene(new FallingStarCutscene());
    }

    public override void OnMissionComplete(bool giveRewards = true)
    {
        base.OnMissionComplete(giveRewards);

        //MissionPlayer player = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
        //player.StartNextMission(...);
    }

    public override void Update()
    {
        base.Update();

        if (CurrentIndex < (int)Objectives.ClearSlimeRain)
        {
            Main.slimeRain = false;
            Main.slimeRainTime = 0;
        }
        else if (CurrentIndex == (int)Objectives.ClearSlimeRain)
        {
            if (!Main.slimeRain)
            {
                Main.StartSlimeRain();
            }
        }
        if (CurrentIndex > (int)Objectives.TalkToTownies)
        {
            UpdateAmbientSound();
        }
        if (CurrentIndex == (int)Objectives.ExploreBiomes)
        {
           
        }
        Main.bloodMoon = false;
    }

    #region Event Registration
    protected override void RegisterEventHandlers()
    {
        if (eventsRegistered) return;

        base.RegisterEventHandlers();

        // Register event handlers specific to this mission
        ObjectiveEventNPC.OnNPCChat += OnNPCChatHandler;
        ObjectiveEventItem.OnItemPickup += OnItemPickupHandler;
        ObjectiveEventItem.OnItemUpdate += OnItemUpdateHandler;
        ObjectiveEventNPC.OnNPCKill += OnNPCKillHandler;
        ObjectiveEventTile.OnTileBreak += OnTileBreakHandler;
        ObjectiveEventPlayer.OnBiomeEnter += OnBiomeEnterHandler;

        ModContent.GetInstance<Reverie>().Logger.Debug($"[A Falling Star] Registered event handlers");

        eventsRegistered = true;
    }

    protected override void UnregisterEventHandlers()
    {
        if (!eventsRegistered) return;

        // Unregister event handlers
        ObjectiveEventNPC.OnNPCChat -= OnNPCChatHandler;
        ObjectiveEventItem.OnItemPickup -= OnItemPickupHandler;
        ObjectiveEventItem.OnItemUpdate -= OnItemUpdateHandler;
        ObjectiveEventNPC.OnNPCKill -= OnNPCKillHandler;
        ObjectiveEventTile.OnTileBreak -= OnTileBreakHandler;
        ObjectiveEventPlayer.OnBiomeEnter -= OnBiomeEnterHandler;

        ModContent.GetInstance<Reverie>().Logger.Debug($"[A Falling Star] Unregistered event handlers");
        base.UnregisterEventHandlers();
    }

    //public override void LoadState(MissionDataContainer state)
    //{
    //    base.LoadState(state);

    //    // Restore visibility conditions based on mission state
    //    if (Progress == MissionProgress.Active)
    //    {
    //        // Example: if player has found the item but hasn't talked to guide yet
    //        if (specialItemFound)
    //        {
    //            SetObjectiveVisibility((int)Objectives.ExploreBiomes, 2, true);
    //        }
    //    }
    //}

    #endregion

    #region Event Handlers
    protected override void OnObjectiveIndexComplete(int setIndex, ObjectiveSet set)
    {
        try
        {
            var objective = (Objectives)setIndex;

            switch (objective)
            {
                //case Objectives.CheckIn:
                //    DialogueManager.Instance.StartDialogueByKey(
                //        NPCManager.GuideData,
                //        DialogueKeys.FallingStar.SlimeInfestation,
                //        lineCount: 2,
                //        zoomIn: true);
                //    break;
                //case Objectives.ClearSlimes:
                //    StartSlimeRain();
                //    break;
                //case Objectives.ClearSlimeRain:
                //    SpawnKingSlime();
                //    break;
                //case Objectives.DefeatKingSlime:
                //    DialogueManager.Instance.StartDialogueByKey(
                //        NPCManager.GuideData,
                //        DialogueKeys.FallingStar.KingSlimeDefeat,
                //        lineCount: 4,
                //        zoomIn: true);
                //    break;
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnObjectiveIndexComplete: {ex.Message}");
        }
    }

    protected override void OnObjectiveComplete(int objectiveIndex)
    {
        try
        {
            var currentObjectiveSet = (Objectives)CurrentIndex;

            if (currentObjectiveSet == Objectives.TalkToTownies && objectiveIndex == 1)
            {
                GiveStarterItems();
            }
            if (currentObjectiveSet == Objectives.TalkToTownies && objectiveIndex == 0)
            {
                GiveMirror();
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnObjectiveComplete: {ex.Message}");
        }
    }

    private void OnNPCChatHandler(NPC npc, ref string chat)
    {
        if (Progress != MissionProgress.Active) return;

        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.TalkToTownies:
                if (npc.type == NPCID.Guide)
                {
                    DialogueManager.Instance.StartDialogueByKey(
                        NPCManager.GuideData,
                        DialogueKeys.FallingStar.GatheringResources,
                        lineCount: 6,
                        zoomIn: true,
                        modifications:
                        [(line: 1, delay: 2, emote: 3),
                        (line: 2, delay: 2, emote: 0),
                        (line: 3, delay: 3, emote: 2),
                        (line: 4, delay: 3, emote: 2),
                        (line: 5, delay: 5, emote: 0),
                        (line: 6, delay: 2, emote: 0)]);
                    UpdateProgress(0);
                }
                if (npc.type == NPCID.Merchant)
                {
                    DialogueManager.Instance.StartDialogueByKey(
                        NPCManager.MerchantData,
                        DialogueKeys.Merchant.MerchantIntro,
                        lineCount: 5,
                        zoomIn: true,
                        modifications:
                       [(line: 1, delay: 3, emote: 0),
                            (line: 2, delay: 3, emote: 1),
                            (line: 3, delay: 3, emote: 0),
                            (line: 4, delay: 3, emote: 0),
                            (line: 5, delay: 3, emote: 1)]);
                    UpdateProgress(1);
                }
                if (npc.type == NPCID.Demolitionist)
                {
                    DialogueManager.Instance.StartDialogueByKey(
                        NPCManager.DemolitionistData,
                        DialogueKeys.Demolitionist.DemolitionistIntro,
                        lineCount: 4,
                        zoomIn: true,
                        modifications:
                        [(line: 1, delay: 3, emote: 0),
                            (line: 2, delay: 3, emote: 0),
                            (line: 3, delay: 3, emote: 0),
                            (line: 4, delay: 3, emote: 0)]
                        );

                    UpdateProgress(2);
                }
                if (npc.type == NPCID.Nurse)
                {
                    DialogueManager.Instance.StartDialogueByKey(
                        NPCManager.NurseData,
                        DialogueKeys.Nurse.NurseIntro,
                        lineCount: 4,
                        zoomIn: true,
                        modifications:
                        [(line: 1, delay: 3, emote: 0),
                            (line: 2, delay: 3, emote: 0),
                            (line: 3, delay: 3, emote: 0),
                            (line: 4, delay: 3, emote: 0)]
                        );

                    UpdateProgress(3);
                }
                break;
            case Objectives.ExploreBiomes:
                if (npc.type == NPCID.Guide && specialItemFound)
                {
                    DialogueManager.Instance.StartDialogueByKey(
                        NPCManager.GuideData,
                        DialogueKeys.FallingStar.GuideReadsChronicleI,
                        lineCount: 3,
                        zoomIn: true);
                    UpdateProgress(1);
                    SetObjectiveVisibility((int)Objectives.ExploreBiomes, 2, true);
                    SetObjectiveVisibility((int)Objectives.ExploreBiomes, 3, true);
                    SetObjectiveVisibility((int)Objectives.ExploreBiomes, 4, true);
                    SetObjectiveVisibility((int)Objectives.ExploreBiomes, 5, true);
                }
                break;
        }
    }

    private void OnItemPickupHandler(Item item, Player player)
    {
        if (Progress != MissionProgress.Active) return;

        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.GatherResources:
                if (item.IsWood())
                    UpdateProgress(0, item.stack);
                break;
            case Objectives.AquireItems:
                if (item.IsOre())
                    UpdateProgress(0, item.stack);
                if (item.accessory)
                    UpdateProgress(1, item.stack);
                break;
            case Objectives.ExploreBiomes:
                if (item.type == ModContent.ItemType<ArchiverChronicleI>())
                {
                    OnSpecialItemDiscovered();

                    Main.NewText("Consider wisely...", Color.White);

                    DialogueManager.Instance.StartDialogueByKey(
                        NPCManager.GuideData,
                        DialogueKeys.FallingStar.ArchiverChronicleIFound,
                        lineCount: 3,
                        zoomIn: false);
                }
                break;
        }
    }
    private void OnItemUpdateHandler(Item item, Player player)
    {
        if (Progress != MissionProgress.Active) return;

        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.AquireItems:
                if (item.accessory)
                    UpdateProgress(1, item.stack);
                break;
        }
    }
    private void OnItemEquipHandler(Item item, Player player)
    {
        if (Progress != MissionProgress.Active) return;

        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.AquireItems:
                if (item.accessory)
                    UpdateProgress(1, item.stack);
                break;
        }
    }
    private void OnNPCKillHandler(NPC npc)
    {
        if (Progress != MissionProgress.Active) return;

        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.ClearSlimes:
                if (npc.type == NPCAIStyleID.Slime)
                    UpdateProgress(0);
                break;
            case Objectives.ExploreBiomes:
                if (player.ZoneRockLayerHeight || player.ZoneDirtLayerHeight)
                    if (npc.aiStyle != NPCAIStyleID.Slime)
                        UpdateProgress(2);
                break;
            case Objectives.ClearSlimeRain:
                HandleSlimeRain(npc);
                break;
            case Objectives.DefeatKingSlime:
                if (npc.type == NPCID.KingSlime)
                    UpdateProgress(0);
                break;
        }
    }

    private void OnTileBreakHandler(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (Progress != MissionProgress.Active) return;

        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.GatherResources:
                if (type == TileID.Pots)
                {
                    potTileBreakCounter++;
                    if (potTileBreakCounter >= 4)
                    {
                        UpdateProgress(1);
                        potTileBreakCounter = 0;
                    }
                }
                break;
        }
    }

    private void OnBiomeEnterHandler(Player player, BiomeType biome)
    {
        if (Progress != MissionProgress.Active) return;

        var objective = (Objectives)CurrentIndex;

        switch (objective)
        {
            case Objectives.ExploreBiomes:
                if (biome == BiomeType.Underground)
                    UpdateProgress(0);
                if (biome == BiomeType.Glowshroom)
                    UpdateProgress(2);
                if (biome == BiomeType.Jungle)
                    UpdateProgress(3);
                if (biome == BiomeType.UndergroundDesert)
                    UpdateProgress(4);
                if (biome == BiomeType.Snow)
                    UpdateProgress(5);
                break;

            case Objectives.DefendTown:
                if (biome == BiomeType.Forest)
                    UpdateProgress(0);
                break;
        }
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Plays a periodic chime every 2-6 minutes, indicates the player's reverie is growing stronger.
    /// </summary>
    private void UpdateAmbientSound()
    {
        if (nextChimeTime == 0)
        {
            nextChimeTime = Main.rand.Next(120 * 60, 360 * 60);
        }

        reverieSFXtimer++;

        int fadeStartTime = nextChimeTime - 10 * 60;

        if (reverieSFXtimer > fadeStartTime && reverieSFXtimer < nextChimeTime)
        {
            float fadeOutProgress = (float)(reverieSFXtimer - fadeStartTime) / (10 * 60);

            Main.musicFade[Main.curMusic] = 1f - fadeOutProgress;
        }

        if (reverieSFXtimer >= nextChimeTime)
        {
            Main.musicFade[Main.curMusic] = 0f;

            SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}ReverieChime")
            {
                Volume = 7.5f,
                Pitch = 0f,
                PitchVariance = 1f,
                MaxInstances = 0
            }, Main.LocalPlayer.position);

            reverieSFXtimer = 0;
            nextChimeTime = 0;
        }

        if (reverieSFXtimer > 0 && reverieSFXtimer <= 2 * 60)
        {
            Main.musicFade[Main.curMusic] = 0f;
        }

        if (reverieSFXtimer > 2 * 60 && reverieSFXtimer < 5 * 60)
        {
            float fadeInProgress = (float)(reverieSFXtimer - 2 * 60) / (3 * 60);

            Main.musicFade[Main.curMusic] = fadeInProgress;
        }

        if (reverieSFXtimer == 5 * 60 && Main.rand.NextBool(4))
        {
            if (!DialogueManager.Instance.IsAnyActive())
            {
                DialogueManager.Instance.StartDialogueByKey(
                    NPCManager.GuideData,
                    DialogueKeys.FallingStar.ChimeResponse,
                    lineCount: 2,
                    zoomIn: false);
            }
        }
    }

    public void OnSpecialItemDiscovered()
    {
        specialItemFound = true;

        // Now show "Check in with Guide" objective
        SetObjectiveVisibility((int)Objectives.ExploreBiomes, 1, true);

        // Hide other biome objectives until player checks in with guide
        SetObjectiveVisibility((int)Objectives.ExploreBiomes, 2, false);
        SetObjectiveVisibility((int)Objectives.ExploreBiomes, 3, false);
        SetObjectiveVisibility((int)Objectives.ExploreBiomes, 4, false);
        SetObjectiveVisibility((int)Objectives.ExploreBiomes, 5, false);
    }

    private void GiveStarterItems()
    {
        foreach (var item in CopperItems)
        {
            Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), item.type, item.stack);
        }
    }

    private void GiveMirror()
    {
        Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), ItemID.MagicMirror);   
    }

    private void StartSlimeRain()
    {
        Main.StartSlimeRain(true);
        DialogueManager.Instance.StartDialogueByKey(
        NPCManager.GuideData,
        DialogueKeys.FallingStar.SlimeRain,
        lineCount: 2,
        zoomIn: false);
    }

    private void HandleSlimeRain(NPC npc)
    {
        if (npc.type == NPCAIStyleID.Slime)
        {
            UpdateProgress(0);
            if (Objective[CurrentIndex].Objectives[0].CurrentCount == 25)
            {
                DialogueManager.Instance.StartDialogueByKey(
                NPCManager.GuideData,
                DialogueKeys.FallingStar.SlimeRainCommentary,
                lineCount: 2,
                zoomIn: false);
            }
            if (Objective[CurrentIndex].Objectives[0].CurrentCount == 50)
            {
                DialogueManager.Instance.StartDialogueByKey(
                NPCManager.GuideData,
                DialogueKeys.FallingStar.SlimeRainWarning,
                lineCount: 2,
                zoomIn: false);
            }
        }
    }

    private void SpawnKingSlime()
    {
        DialogueManager.Instance.StartDialogueByKey(
        NPCManager.GuideData,
        DialogueKeys.FallingStar.KSEncounter,
        lineCount: 3,
        zoomIn: false);

        if (!NPC.AnyNPCs(NPCID.KingSlime) && Main.LocalPlayer.whoAmI == Main.myPlayer)
        {
            SoundEngine.PlaySound(SoundID.Roar, Main.LocalPlayer.position);
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.SpawnOnPlayer(Main.LocalPlayer.whoAmI, NPCID.KingSlime);
            }
            else
            {
                NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent,
                    number: Main.LocalPlayer.whoAmI,
                    number2: NPCID.KingSlime);
            }
        }
    }

    #endregion
}

public class ArchiverChronicleNPC : ModNPC
{
    public override string Texture => "Reverie/Assets/Textures/Items/ArchiverChronicle";

    public override void SetDefaults()
    {
        NPC.width = 36;
        NPC.height = 32;
        NPC.aiStyle = -1;
        NPC.lifeMax = 25;
        NPC.immortal = true;
        NPC.dontTakeDamageFromHostiles = true;
        NPC.dontTakeDamage = true;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.friendly = true;
        NPC.GivenName = "???";
    }

    private float baseY;
    private bool positionSaved = false;

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        if (spawnInfo.Player.ZoneRockLayerHeight || spawnInfo.Player.ZoneDirtLayerHeight && !DownedSystem.foundChronicleI)
        {
            var missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();

            var fallingStarMission = missionPlayer.GetMission(MissionID.AFallingStar);

            if (fallingStarMission != null &&
                fallingStarMission.Progress == MissionProgress.Active &&
                fallingStarMission.CurrentIndex == 3 && NPC.CountNPCS(Type) <= 1)
            {
                return 0.4f;
            }
        }

        return 0f;
    }

    private int soundTimer = 0;
    private bool hasPannedCamera = false;

    public override void AI()
    {
        soundTimer++;
        if (soundTimer >= 80)
        {
            SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.75f, Pitch = -0.2f }, NPC.Center);        
            soundTimer = 0;
        }

        Lighting.AddLight(NPC.Center, 0.4f, 0.4f, 0.7f);

        if (!positionSaved)
        {
            baseY = NPC.position.Y;
            positionSaved = true;
        }

        float amplitude = 7f;
        float frequency = 0.05f;

        float sineWave = (float)Math.Sin(Main.GameUpdateCount * frequency);
        NPC.position.Y = baseY + (sineWave * amplitude);

        NPC.velocity.Y = 0f;

        Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Enchanted_Gold, 0f, 0f, 150, default, 0.65f);

        if (!hasPannedCamera)
        {
            float distanceToPlayer = Vector2.Distance(Main.LocalPlayer.Center, NPC.Center);
            if (distanceToPlayer < 50 * 16)
            {
                const int CUTSCENE_DURATION = 3 * 60;
                Vector2 targetPosition = NPC.Center;

                CameraSystem.DoPanAnimation(
                    CUTSCENE_DURATION,
                    targetPosition,
                    Vector2.Zero
                );

                hasPannedCamera = true;

                SoundEngine.PlaySound(SoundID.Item29, NPC.Center);
            }
        }
    }

    public override bool CanChat() => true;

    public override string GetChat()
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            Item.NewItem(NPC.GetSource_Loot(), NPC.Center, ModContent.ItemType<ArchiverChronicleI>());      
        else
            Main.LocalPlayer.QuickSpawnItem(new EntitySource_Loot(NPC), ModContent.ItemType<ArchiverChronicleI>());
        Main.CloseNPCChatOrSign();

        Vector2 speed = Main.rand.NextVector2Circular(1.6f, 1.6f);
        Dust dust = Dust.NewDustDirect(
        NPC.position,
                  NPC.width,
                  NPC.height,
                  DustID.Enchanted_Gold,
                  speed.X,
                  speed.Y,
                  0,
                  default,
                  Main.rand.NextFloat(1f, 2f)
              );
        dust.noGravity = true;
        dust.fadeIn = 1.2f;

        DownedSystem.foundChronicleI = true;
        NPC.active = false;

        return base.GetChat();
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 drawPos = NPC.Center - screenPos;
        Vector2 origin = new(texture.Width / 2f, texture.Height / 2f);

        float mainPulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.3f + 0.7f;
        float secondPulse = (float)Math.Sin(Main.GameUpdateCount * 0.03f + 1f) * 0.3f + 0.5f;

        spriteBatch.Draw(
            texture,
            drawPos,
            null,
            Color.White * 0.3f * mainPulse,
            0f,
            origin,
            NPC.scale * (1f + mainPulse * 0.2f),
            SpriteEffects.None,
            0f
        );

        spriteBatch.Draw(
            texture,
            drawPos,
            null,
            Color.White * 0.5f * secondPulse,
            0f,
            origin,
            NPC.scale * (1f + secondPulse * 0.15f),
            SpriteEffects.None,
            0f
        );

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                         DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

        Color glowColor = new Color(100, 100, 220) * mainPulse;

        spriteBatch.Draw(
            texture,
            drawPos,
            null,
            glowColor,
            0f,
            origin,
            NPC.scale,
            SpriteEffects.None,
            0f
        );

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                         DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

        return false;
    }
}