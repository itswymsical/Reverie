using Reverie.Common.Global;
using Reverie.Content.Archaea.Biomes;
using Reverie.Content.Biomes;
using Reverie.Helpers;
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
            return item.pick > 0 || item.GetGlobalItem<ReverieGlobalItem>().Shovel;
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
            return (item.bodySlot != -1 || item.headSlot != -1 || item.legSlot != -1) && !item.vanity;
        }
        public static bool ZoneCanopy(this Player player) => player.InModBiome<WoodlandCanopyBiome>();
        public static bool ZoneEmberiteCaverns(this Player player) => player.InModBiome<EmberiteCavernsBiome>();

    }

}