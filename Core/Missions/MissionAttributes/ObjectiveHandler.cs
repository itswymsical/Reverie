using Reverie.Core.Missions;
using Reverie.Utilities;
using Terraria.DataStructures;


namespace Reverie.Core.Missions.MissionAttributes;

public abstract class ObjectiveHandler
{
    protected readonly object handlerLock = new();
    public Mission Mission { get; }

    protected ObjectiveHandler(Mission mission)
    {
        Mission = mission ?? throw new ArgumentNullException(nameof(mission));
        ModContent.GetInstance<Reverie>().Logger.Debug($"Created handler for mission: {mission.Name}");
    }

    #region Virtual Event Handlers
    public virtual void OnObjectiveComplete(int objectiveIndex)
    {
        lock (handlerLock)
        {
            try
            {
                ModContent.GetInstance<Reverie>().Logger.Debug($"Objective {objectiveIndex} completed in mission {Mission.Name}");
                HandleObjectiveComplete(objectiveIndex);
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnObjectiveComplete for mission {Mission.Name}: {ex.Message}");
            }
        }
    }

    protected virtual void HandleObjectiveComplete(int objectiveIndex) { }

    public virtual void OnItemPickup(Item item)
    {
        lock (handlerLock)
        {
            try
            {
                HandleItemPickup(item);
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnItemPickup for mission {Mission.Name}: {ex.Message}");
            }
        }
    }

    protected virtual void HandleItemPickup(Item item) { }

    public virtual void OnItemCreated(Item item, ItemCreationContext context)
    {
        lock (handlerLock)
        {
            try
            {
                HandleItemCreated(item, context);
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnItemCreated for mission {Mission.Name}: {ex.Message}");
            }
        }
    }

    protected virtual void HandleItemCreated(Item item, ItemCreationContext context) { }

    public virtual void OnNPCKill(NPC npc)
    {
        lock (handlerLock)
        {
            try
            {
                HandleNPCKill(npc);
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnNPCKill for mission {Mission.Name}: {ex.Message}");
            }
        }
    }

    protected virtual void HandleNPCKill(NPC npc) { }

    public virtual void OnNPCChat(NPC npc)
    {
        lock (handlerLock)
        {
            try
            {
                HandleNPCChat(npc);
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnNPCChat for mission {Mission.Name}: {ex.Message}");
            }
        }
    }

    protected virtual void HandleNPCChat(NPC npc) { }

    public virtual void OnNPCSpawn(NPC npc)
    {
        lock (handlerLock)
        {
            try
            {
                HandleNPCSpawn(npc);
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnNPCSpawn for mission {Mission.Name}: {ex.Message}");
            }
        }
    }

    protected virtual void HandleNPCSpawn(NPC npc) { }

    public virtual void OnNPCLoot(NPC npc)
    {
        lock (handlerLock)
        {
            try
            {
                HandleNPCLoot(npc);
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnNPCLoot for mission {Mission.Name}: {ex.Message}");
            }
        }
    }

    protected virtual void HandleNPCLoot(NPC npc) { }

    public virtual void OnNPCHit(NPC npc, int damage)
    {
        lock (handlerLock)
        {
            try
            {
                ModContent.GetInstance<Reverie>().Logger.Debug($"NPC Hit detected in mission {Mission.Name}: Type={npc.type}, Damage={damage}");
                HandleNPCHit(npc, damage);
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnNPCHit for mission {Mission.Name}: {ex.Message}");
            }
        }
    }

    protected virtual void HandleNPCHit(NPC npc, int damage) { }

    public virtual void OnBiomeEnter(Player player, BiomeType biome)
    {
        lock (handlerLock)
        {
            try
            {
                HandleBiomeEnter(player, biome);
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnBiomeEnter for mission {Mission.Name}: {ex.Message}");
            }
        }
    }

    protected virtual void HandleBiomeEnter(Player player, BiomeType biome) { }


    #endregion
}