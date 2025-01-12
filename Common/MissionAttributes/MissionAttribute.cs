using System;
using System.Text;
using System.Threading.Tasks;

namespace Reverie.Common.MissionAttributes
{
    public class MissionHandlerAttribute : Attribute
    {
        public int MissionID { get; }
        public int SetIndex { get; }

        public MissionHandlerAttribute(int missionId, int setIndex = -1)
        {
            MissionID = missionId;
            SetIndex = setIndex;
        }
    }
}