using Reverie.Common.Global;
using Reverie.Common.Players;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Reverie.Content.Terraria.Items
{
    public abstract class ShovelItem : ModItem
    {
        public int ShovelRange = 5;
        private int x;
        private int y;

        protected override bool CloneNewInstances => false;

        public void DiggingPower(int digPower)
        {
            Item.GetGlobalItem<ReverieGlobalItem>().digPower = digPower;
            Item.pick = digPower;
        }

        public override void SetStaticDefaults() => ItemID.Sets.CanGetPrefixes[Type] = true;

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            TooltipLine message = new(Mod, "ReverieMod:Shovel Info", "Stronger on soft tiles");
            tooltips.Add(message);
            if (Main.SmartCursorWanted)
            {
                TooltipLine message1 = new(Mod, "ReverieMod:Smart Cursor Info", "Smart cursor disables craters");
                tooltips.Add(message1);
            }
            if (Item.pick <= 0)
                return;

            foreach (TooltipLine line in tooltips.Where(line => line.Name == "PickPower"))
                line.Text = $"{Item.GetGlobalItem<ReverieGlobalItem>().digPower}% digging power";
        }


        public override bool? UseItem(Player player)
        {
            if (Main.SmartCursorWanted)
            {
                x = Main.SmartCursorX;
                y = Main.SmartCursorY;
            }
            else
            {
                x = (int)Main.MouseWorld.X;
                y = (int)Main.MouseWorld.Y;
            }

            if (player.Distance(Main.MouseWorld) < 16 * ShovelRange)
            {

                player.GetModPlayer<ShovelPlayer>().DigBlocks(x, y);
            }
            return true;
        }

        public override int ChoosePrefix(UnifiedRandom rand) => rand.Next(new int[]
        {
            PrefixID.Agile,
            PrefixID.Quick,
            PrefixID.Light,

            PrefixID.Slow,
            PrefixID.Sluggish,
            PrefixID.Lazy,

            PrefixID.Bulky,
            PrefixID.Heavy,

            PrefixID.Damaged,
            PrefixID.Broken,

            PrefixID.Unhappy,
            PrefixID.Nimble,
            PrefixID.Dull
        });
    }
}