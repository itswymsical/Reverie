using Reverie.Common.Players;
using Reverie.Common.Systems.Subworlds.Archaea;
using Reverie.Common.UI.MissionUI;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using SubworldLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace Reverie.Content.Terraria.Items
{
    public class PlayMissionUI : ModItem
    {
        public override string Texture => "Terraria/Images/UI/Bestiary/Icon_Locked";
        public override void SetDefaults()
        {
            Item.useTime = Item.useAnimation = 20;
            Item.value = Item.buyPrice(0);
            Item.rare = ItemRarityID.Quest;
            Item.useStyle = ItemUseStyleID.HoldUp;
        }
        public override bool? UseItem(Player player)
        {
            Core.Missions.Mission mission = Main.LocalPlayer.GetModPlayer<MissionPlayer>().GetMission(MissionID.Reawakening);

            if (Main.myPlayer == player.whoAmI)
                InGameNotificationsTracker.AddNotification(new MissionCompleteNotification(mission));

            return true;
        }
    }
}