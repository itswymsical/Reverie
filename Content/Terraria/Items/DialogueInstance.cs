using Reverie.Common.Systems.Subworlds.Archaea;
using Reverie.Core.Dialogue;
using SubworldLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Content.Terraria.Items
{
    public class DialogueInstance : ModItem
    {
        public override string Texture => "Terraria/Images/UI/Bestiary/Icon_Locked";
        public override void SetDefaults()
        {
            Item.useTime = Item.useAnimation = 20;
            Item.value = Item.buyPrice(0);
            Item.rare = ItemRarityID.Quest;
            Item.useStyle = ItemUseStyleID.HoldUp;      
        }
        public override bool CanUseItem(Player player)
        {
            if (DialogueManager.Instance.PlayDialogueSequence(NPCDataManager.GuideData, DialogueID.GuideGivingPropsTrainingArc, true))
                return false;

            return base.CanUseItem(player);
        }
        public override bool? UseItem(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
                DialogueManager.Instance.PlayDialogueSequence(NPCDataManager.GuideData, DialogueID.GuideGivingPropsTrainingArc, true);

            return true;
        }
    }
}
