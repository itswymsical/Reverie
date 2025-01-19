using Reverie.Core.Missions;
using Terraria.ID;
using Terraria;
using Reverie.Common.MissionAttributes;

namespace Reverie.Content.Terraria.Missions.Sideline
{
    [MissionHandler(MissionID.DirtiestBlock)]
    public class DirtiestBlockHandler : MissionObjectiveHandler
    {
        public DirtiestBlockHandler(Mission mission) : base(mission) { }

        public override void OnItemPickup(Item item)
        {
            if (item.type == ItemID.DirtiestBlock)
            {
                Mission.UpdateProgress(0);
            }
        }
    }
}
