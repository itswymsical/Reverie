using Microsoft.Xna.Framework;
using Reverie.Common.Players;
using Reverie.Content.Terraria.Items;
using Reverie.Core.Skills;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Common.Tiles
{
    public partial class ReverieGlobalTile : GlobalTile
    {
        public bool harvestApplied;

        public override void Drop(int i, int j, int type)
        {
            Player player = Main.player[Main.myPlayer];
            var skillPlayer = player.GetModPlayer<SkillPlayer>();
            int itemType = GetItemTypeFromTileType(type);
            bool isFortuneUnlocked = skillPlayer.GetSkillStack(SkillList.IDs.Fortune) > 0;

            // Only apply fortune effect if the tile was NOT placed by a fortune player
            if (isFortuneUnlocked && TileID.Sets.Ore[type])
            {
                Skill fortuneSkill = SkillList.GetSkillById(SkillList.IDs.Fortune);
                float fortuneChance = fortuneSkill.GetEffectForStack(skillPlayer.GetSkillStack(SkillList.IDs.Fortune));

                if (Main.rand.NextFloat() < fortuneChance)
                {
                    int extraItems = Main.rand.Next(1, 3);
                    Item item = new();
                    item.SetDefaults(itemType);
                    AdvancedPopupRequest popUp = new()
                    {
                        Text = $"{item.Name} (+{extraItems})",
                        Color = new Color(204, 181, 72),
                        DurationInFrames = 60
                    };
                    harvestApplied = true;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, itemType, extraItems);
                    }
                    else
                    {
                        player.QuickSpawnItem(new EntitySource_TileBreak(i, j), itemType, extraItems);
                    }
                    PopupText.NewText(popUp, player.position);
                }
            }
            else
            {
                harvestApplied = false;
            }
        }

        public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            Player player = Main.LocalPlayer;
            var skillPlayer = player.GetModPlayer<SkillPlayer>();
            bool isFortuneUnlocked = skillPlayer.GetSkillStack(SkillList.IDs.Fortune) > 0;

            if (isFortuneUnlocked && harvestApplied && TileID.Sets.Ore[type])
            {
                harvestApplied = false;
                for (int num = 0; num < 60; num += 2)
                {
                    Dust.NewDust(new Vector2(i * 16, j * 16), 8, 8, DustID.SpelunkerGlowstickSparkle, Main.rand.NextFloat((float)num * 0.05f), -4f, Scale: 1f);
                }
                SoundEngine.PlaySound(SoundID.CoinPickup, new Vector2(i * 16, j * 16));
            }
            if (type == TileID.Trees && Main.rand.NextBool(28))
            {
                Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 0, 0, ModContent.ItemType<Apple>());
                
            }
        }
        private static int GetItemTypeFromTileType(int tileType)
        {
            // Match tile types to their corresponding item types
            return tileType switch
            {
                TileID.Copper => ItemID.CopperOre,
                TileID.Iron => ItemID.IronOre,
                TileID.Silver => ItemID.SilverOre,
                TileID.Gold => ItemID.GoldOre,
                TileID.Tin => ItemID.TinOre,
                TileID.Lead => ItemID.LeadOre,
                TileID.Tungsten => ItemID.TungstenOre,
                TileID.Platinum => ItemID.PlatinumOre,
                TileID.Demonite => ItemID.DemoniteOre,
                TileID.Crimtane => ItemID.CrimtaneOre,
                TileID.Meteorite => ItemID.Meteorite,
                TileID.Hellstone => ItemID.Hellstone,
                TileID.Amethyst => ItemID.Amethyst,
                TileID.Sapphire => ItemID.Sapphire,
                TileID.Ruby => ItemID.Ruby,
                TileID.Topaz => ItemID.Topaz,
                TileID.Emerald => ItemID.Emerald,
                TileID.Diamond => ItemID.Diamond,
                TileID.Crystals => ItemID.CrystalShard,
                TileID.Cobalt => ItemID.CobaltOre,
                TileID.Mythril => ItemID.MythrilOre,
                TileID.Adamantite => ItemID.AdamantiteOre,
                TileID.Palladium => ItemID.PalladiumOre,
                TileID.Orichalcum => ItemID.OrichalcumOre,
                TileID.Titanium => ItemID.TitaniumOre,
                TileID.Chlorophyte => ItemID.ChlorophyteOre,
                // Add other tile to item type mappings as needed
                _ => 0,
            };
        }
    }
}
