using Reverie.Core.Missions;
using Terraria;
using Terraria.DataStructures;

namespace Reverie.Common.MissionAttributes
{
    public abstract class MissionObjectiveHandler
    {
        public readonly Mission Mission;

        public MissionObjectiveHandler(Mission mission)
        {
            Mission = mission;
        }
        public virtual void OnObjectiveComplete(int objectiveIndex) { }
        public virtual void OnItemPickup(Item item) { }
        public virtual void OnItemCreated(Item item, ItemCreationContext context) { }
        public virtual void OnNPCKill(NPC npc) { }
        public virtual void OnNPCChat(NPC npc) { }
        public virtual void OnNPCSpawn(NPC npc) { }
        public virtual void OnNPCLoot(NPC npc) { }
        public virtual void OnNPCHit(NPC npc, int damage) { }

    }
}