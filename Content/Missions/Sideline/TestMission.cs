using Reverie.Core.Missions;
using Terraria.ID;
using Terraria;
using Reverie.Common.MissionAttributes;

namespace Reverie.Core.Missions.Sideline
{
    [MissionHandler(MissionID.TestMission)]
    public class TestMission : MissionObjectiveHandler
    {
        public TestMission(Mission mission) : base(mission) { }

        public override void OnItemPickup(Item item)
        {
            if (item.type == ItemID.DirtBlock)
            {
                Mission.UpdateProgress(0);
            }
        }
    }
}
