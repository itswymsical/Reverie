﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.DataStructures;
using Reverie.Utilities;

namespace Reverie.Core.Missions.MissionAttributes;

public class MissionHandlerManager
{
    private readonly object handlerLock = new();
    private readonly Dictionary<int, ObjectiveHandler> handlers = [];
    private static MissionHandlerManager instance;
    public static MissionHandlerManager Instance => instance ??= new MissionHandlerManager();

    public void RegisterMissionHandler(Mission mission)
    {
        if (mission == null)
            return;

        lock (handlerLock)
        {
            try
            {
                ModContent.GetInstance<Reverie>().Logger.Debug($"Handler count before registration: {handlers.Count}");

                if (handlers.ContainsKey(mission.ID))
                {
                    ModContent.GetInstance<Reverie>().Logger.Debug($"Handler already exists for mission {mission.ID}");
                    return;
                }

                var handlerType = Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .FirstOrDefault(t =>
                        t.GetCustomAttribute<MissionHandlerAttribute>()?.MissionID == mission.ID);

                if (handlerType != null)
                {
                    var handler = (ObjectiveHandler)Activator.CreateInstance(handlerType, mission);
                    handlers[mission.ID] = handler;
                    ModContent.GetInstance<Reverie>().Logger.Info($"Registered handler for mission {mission.Name}");
                }
                ModContent.GetInstance<Reverie>().Logger.Debug($"Handler count after registration: {handlers.Count}");
                ModContent.GetInstance<Reverie>().Logger.Debug($"Current handlers: {string.Join(", ", handlers.Keys)}");

            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Failed to register handler: {ex.Message}");
            }
        }
    }

    public void Reset()
    {
        lock (handlerLock)
        {
            handlers.Clear();
            ModContent.GetInstance<Reverie>().Logger.Info("All mission handlers reset");
        }
    }

    private IEnumerable<ObjectiveHandler> ActiveHandlers
    {
        get
        {
            lock (handlerLock)
            {
                return handlers.Values
                    .Where(h => h.Mission.Progress == MissionProgress.Active)
                    .ToList();
            }
        }
    }

    public void OnObjectiveComplete(Mission mission, int objectiveIndex)
    {
        lock (handlerLock)
        {
            try
            {
                if (handlers.TryGetValue(mission.ID, out var handler))
                {
                    handler.OnObjectiveComplete(objectiveIndex);
                }
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnObjectiveComplete: {ex.Message}");
            }
        }
    }

    #region Event Handlers
    public void OnItemCreated(Item item, ItemCreationContext context)
    {
        lock (handlerLock)
        {
            try
            {
                foreach (var handler in ActiveHandlers)
                {
                    handler.OnItemCreated(item, context);
                }
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnItemCreated: {ex.Message}");
            }
        }
    }

    public void OnItemPickup(Item item)
    {
        lock (handlerLock)
        {
            try
            {
                foreach (var handler in ActiveHandlers)
                {
                    handler.OnItemPickup(item);
                }
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnItemPickup: {ex.Message}");
            }
        }
    }

    public void OnNPCKill(NPC npc)
    {
        lock (handlerLock)
        {
            try
            {
                foreach (var handler in ActiveHandlers)
                {
                    handler.OnNPCKill(npc);
                }
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnNPCKill: {ex.Message}");
            }
        }
    }

    public void OnNPCChat(NPC npc)
    {
        lock (handlerLock)
        {
            try
            {
                foreach (var handler in ActiveHandlers)
                {
                    handler.OnNPCChat(npc);
                }
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnNPCChat: {ex.Message}");
            }
        }
    }

    public void OnNPCSpawn(NPC npc)
    {
        lock (handlerLock)
        {
            try
            {
                foreach (var handler in ActiveHandlers)
                {
                    handler.OnNPCSpawn(npc);
                }
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnNPCSpawn: {ex.Message}");
            }
        }
    }

    public void OnNPCLoot(NPC npc)
    {
        lock (handlerLock)
        {
            try
            {
                foreach (var handler in ActiveHandlers)
                {
                    handler.OnNPCLoot(npc);
                }
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnNPCLoot: {ex.Message}");
            }
        }
    }

    public void OnNPCHit(NPC npc, int damage)
    {
        lock (handlerLock)
        {
            try
            {
                var activeHandlers = ActiveHandlers.ToList();
                ModContent.GetInstance<Reverie>().Logger.Debug($"Processing NPC hit with {activeHandlers.Count} active handlers");

                foreach (var handler in activeHandlers)
                {
                    handler.OnNPCHit(npc, damage);
                }
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnNPCHit: {ex.Message}");
            }
        }
    }

    public void OnBiomeEnter(Player player, BiomeType biome)
    {
        lock (handlerLock)
        {
            try
            {
                foreach (var handler in ActiveHandlers)
                {
                    handler.OnBiomeEnter(player, biome);
                    ModContent.GetInstance<Reverie>().Logger.Error($"OnBiomeEnter: {biome}");
                }
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnBiomeEnter: {ex.Message}");
            }
        }
    }
    #endregion
}