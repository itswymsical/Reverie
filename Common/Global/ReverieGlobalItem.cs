using Microsoft.Xna.Framework;
using Reverie.Common.Extensions;
using Reverie.Common.Systems;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Common.Global
{
    public partial class ReverieGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;
        protected override bool CloneNewInstances => true;

        public bool Shovel;
        public int digPower;
        public int radius = 2;

        public static int GetDigPower(int shovel)
        {
            Item i = ModContent.GetModItem(shovel).Item;
            return i.GetGlobalItem<ReverieGlobalItem>().digPower;
        }

        public static int GetShovelRadius(int shovel)
        {
            Item i = ModContent.GetModItem(shovel).Item;
            return i.GetGlobalItem<ReverieGlobalItem>().radius;
        }

        public override void SetDefaults(Item entity)
        {
            if (entity.type == ItemID.Acorn)
            {
                base.SetDefaults(entity);

                entity.DamageType = DamageClass.Ranged;
                entity.damage = 6;
                entity.knockBack = 1f;
                entity.shootSpeed = 7f;
                entity.ammo = entity.type;
            }
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(item, tooltips);

            if (ReverieHooks.IsMirror(item))
            {
                TooltipLine line = new(Mod, "MirrorRightClick", $"Right-click to open Magic Mirror [i:{ItemID.FragmentStardust}]")
                {
                    OverrideColor = new Color(150, 150, 255) // Gold color
                };
                tooltips.Add(line);
            }
        }
        public override bool AltFunctionUse(Item item, Player player)
        {
            if (ReverieHooks.IsMirror(item))
            {
                ReverieUISystem.Instance.mirrorUI.OpenUI();
            }
            return base.AltFunctionUse(item, player);
        }
    }
}