using Microsoft.Xna.Framework;
using Reverie.Common.MissionAttributes;
using Reverie.Content.Missions.Items;
using Reverie.Content.Missions.NPCs;
using Reverie.Core.Missions;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Reverie.Common.Players.MissionPlayer;

namespace Reverie.Content.Missions.Sideline
{

    [MissionHandler(MissionID.LostBrother)]
    public class LostBrotherMission(Mission mission) : MissionObjectiveHandler(mission)
    {
        private bool hasSpawnedMerchantsBrother = false;

        public override void OnItemPickup(Item item)
        {
            lock (handlerLock)
            {
                try
                {
                    if (item.type == ModContent.ItemType<OldTradeReceipt>() && Mission.CurrentSetIndex == 0)
                    {
                        Mission.UpdateProgress(0);
                    }
                }
                catch (Exception ex)
                {
                    ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnBiomeEnter: {ex.Message}");
                }
            }
        }

        public override void OnNPCChat(NPC npc)
        {
            lock (handlerLock)
            {
                try
                {
                    if (npc.type == NPCID.Merchant)
                    {
                        if (Mission.CurrentSetIndex == 1 || Mission.CurrentSetIndex == 4)
                        {
                            Mission.UpdateProgress(0);
                        }
                    }
                    else if (npc.type == ModContent.NPCType<MerchantsBrother>() && Mission.CurrentSetIndex == 3)
                    {
                        Mission.UpdateProgress(0);
                    }
                }
                catch (Exception ex)
                {
                    ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnBiomeEnter: {ex.Message}");
                }
            } 
        }

        public override void OnBiomeEnter(Player player, BiomeState biomeState)
        {
            if (Mission.CurrentSetIndex != 2) return;

            ModContent.GetInstance<Reverie>().Logger.Debug("Checking evil biome entry");

            if (biomeState.ZoneCorrupt || biomeState.ZoneCrimson)
            {
                Mission.UpdateProgress(0);

                if (!hasSpawnedMerchantsBrother)
                {
                    SpawnZombieMerchant(player);
                    hasSpawnedMerchantsBrother = true;
                    ModContent.GetInstance<Reverie>().Logger.Debug("Spawned Merchant's Brother");
                }
            }
        }

        private static void SpawnZombieMerchant(Player player)
        {
            Vector2 spawnPosition = player.position + new Vector2(Main.rand.Next(-500, 500), -600);
            NPC.NewNPC(default,
                (int)spawnPosition.X,
                (int)spawnPosition.Y,
                ModContent.NPCType<MerchantsBrother>()
            );
        }
    }
}
