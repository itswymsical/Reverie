using Reverie.Core.Missions;
using Terraria.ID;
using Terraria;
using Reverie.Common.MissionAttributes;

namespace Reverie.Content.Terraria.Missions.Sideline
{
    [MissionHandler(MissionID.TestMission)]
    public class TestMissionHandler : MissionObjectiveHandler
    {
        public TestMissionHandler(Mission mission) : base(mission) { }

        public override void OnItemPickup(Item item)
        {
            if (item.type == ItemID.DirtBlock)
            {
                Mission.UpdateProgress(0);
            }
        }
    }
}
