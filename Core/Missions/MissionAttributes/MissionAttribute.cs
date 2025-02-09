

namespace Reverie.Core.Missions.MissionAttributes;

public class MissionHandlerAttribute(int missionId, int setIndex = -1) : Attribute
{
    public int MissionID { get; } = missionId;
    public int SetIndex { get; } = setIndex;
}