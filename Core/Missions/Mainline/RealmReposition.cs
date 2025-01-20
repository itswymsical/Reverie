using Reverie.Core.Missions;
using Terraria.ID;
using Terraria;
using Reverie.Common.MissionAttributes;
using Terraria.ModLoader;
using Reverie.Content.Terraria.Items.Mission;
using Terraria.DataStructures;

namespace Reverie.Core.Missions.Mainline
{
    [MissionHandler(MissionID.Translocator)]
    public class RealmReposition : MissionObjectiveHandler
    {
        public RealmReposition(Mission mission) : base(mission)
        {
            Main.NewText("RealmRepositionHandler constructed"); // Debug
        }

        public override void OnItemCreated(Item item, ItemCreationContext context)
        {
            if (Mission.CurrentSetIndex == 0)
            {
                if (item.type == ModContent.ItemType<RealmCrystal>())
                {
                    Mission.UpdateProgress(0);
                }

                if (item.type == ModContent.ItemType<CoilArray>())
                {
                    Mission.UpdateProgress(1);
                }

                if (item.type == ModContent.ItemType<DimensionalTuningFork>())
                {
                    Mission.UpdateProgress(2);
                }
            }
        }

        public override void OnNPCChat(NPC npc)
        {
            if (Mission.CurrentSetIndex == 1)
            {
                if (npc.type == NPCID.Guide)
                {
                    Mission.UpdateProgress(0);
                }
            }
        }
    }
}
