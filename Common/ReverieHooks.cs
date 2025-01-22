using Reverie.Content.Archaea.Biomes;
using Reverie.Content.Biomes;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Reverie.Assets;

namespace Reverie.Common
{
    public static class ReverieHooks
    {
        public static bool IsPickaxe(this Item item)
        {
            return item.Name.Contains("Pickaxe");
        }
        public static bool IsHealingPot(this Item item)
        {
            return item.type == ItemID.LesserHealingPotion || item.type == ItemID.HealingPotion || item.type == ItemID.GreaterHealingPotion;
        }

        private static readonly HashSet<string> MetalKeywords = new HashSet<string>
        {
            "Ore", "Bar", "Iron", "Lead", "Silver", "Gold", "Platinum", "Tungsten", "Tin", "Copper", 
            "Cobalt", "Palladium", "Mythril", "Orichalcum", "Adamantite", "Titanium"
        };
        public static bool IsAMetalItem(Item item)
        {
            foreach (string keyword in MetalKeywords)
            {
                if (item.Name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool IsWeapon(this Item item)
        {
            return item.pick <= 0 &&
                (item.DamageType == DamageClass.Magic
                || item.DamageType == DamageClass.Summon
                || item.DamageType == DamageClass.Melee
                || item.DamageType == DamageClass.Throwing);
        }
        public static bool IsWood(this Item item)
        {
            return item.type == ItemID.Wood
                || item.type == ItemID.BorealWood
                || item.type == ItemID.PalmWood
                || item.type == ItemID.RichMahogany
                || item.type == ItemID.Ebonwood
                || item.type == ItemID.Shadewood
                || item.type == ItemID.AshWood
                || item.type == ItemID.Pearlwood;
        }
        public static bool IsMirror(this Item item)
        {
            return item.type == ItemID.MagicMirror
                || item.type == ItemID.IceMirror
                || item.type == ItemID.CellPhone
                || item.type == ItemID.Shellphone;
        }
        public static bool CountsAsArmor(this Item item)
        {
            return item.Name.Contains("Leggings")
                || item.Name.Contains("Greaves")
                || item.Name.Contains("Brogues")
                || item.Name.Contains("Chainmail")
                || item.Name.Contains("Chestplate")
                || item.Name.Contains("Breastplate")
                || item.Name.Contains("Helmet")
                || item.Name.Contains("Mask")
                || item.Name.Contains("Hat")
                || item.Name.Contains("Headgear")
                || item.Name.Contains("Hauberk")
                && !item.vanity || item.OriginalDefense > 0;
        }
        public static bool ZoneCanopy(this Player player) => player.InModBiome<WoodlandCanopyBiome>();
        public static bool ZoneEmberiteCaverns(this Player player) => player.InModBiome<EmberiteCavernsBiome>();

    }

}