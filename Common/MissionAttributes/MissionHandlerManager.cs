using Reverie.Core.Missions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;

namespace Reverie.Common.MissionAttributes
{
    public class MissionHandlerManager
    {
        private readonly Dictionary<int, MissionObjectiveHandler> handlers = [];
        private static MissionHandlerManager instance;

        public static MissionHandlerManager Instance => instance ??= new MissionHandlerManager();
        public void Reset()
        {
            handlers.Clear();
        }

        public void RegisterMissionHandler(Mission mission)
        {
            Main.NewText($"Attempting to register handler for mission ID: {mission.ID}"); // Debug

            if (handlers.ContainsKey(mission.ID))
            {
                Main.NewText("Handler already exists for this mission"); // Debug
                return;
            }

            var handlerType = Assembly.GetExecutingAssembly()
                .GetTypes()
                .FirstOrDefault(t =>
                    t.GetCustomAttribute<MissionHandlerAttribute>()?.MissionID == mission.ID);

            Main.NewText($"Found handler type: {handlerType?.Name ?? "none"}"); // Debug

            if (handlerType != null)
            {
                var handler = (MissionObjectiveHandler)Activator.CreateInstance(handlerType, mission);
                handlers[mission.ID] = handler;
                Main.NewText("Handler registered successfully"); // Debug
            }
            else
            {
                Main.NewText("No handler type found for this mission"); // Debug
            }
        }

        public void OnObjectiveComplete(Mission mission, int objectiveIndex)
        {
            if (handlers.TryGetValue(mission.ID, out var handler))
            {
                handler.OnObjectiveComplete(objectiveIndex);
            }
        }

        // Item methods
        public void OnItemCreated(Item item, ItemCreationContext context)
        {
            var activeHandlers = GetActiveHandlers().ToList();
            foreach (var handler in activeHandlers)
            {
                handler.OnItemCreated(item, context);
            }
        }

        public void OnItemPickup(Item item)
        {
            var activeHandlers = GetActiveHandlers().ToList();
            foreach (var handler in activeHandlers)
            {
                handler.OnItemPickup(item);
            }
        }

        // NPC methods
        public void OnNPCKill(NPC npc)
        {
            foreach (var handler in GetActiveHandlers())
            {
                handler.OnNPCKill(npc);
            }
        }

        public void OnNPCChat(NPC npc)
        {
            // Create a safe copy of active handlers
            var activeHandlers = GetActiveHandlers().ToList();
            foreach (var handler in activeHandlers)
            {
                handler.OnNPCChat(npc);
            }
        }

        public void OnNPCSpawn(NPC npc)
        {
            var activeHandlers = GetActiveHandlers().ToList();
            foreach (var handler in activeHandlers)
            {
                handler.OnNPCSpawn(npc);
            }
        }

        public void OnNPCLoot(NPC npc)
        {
            var activeHandlers = GetActiveHandlers().ToList();
            foreach (var handler in activeHandlers)
            {
                handler.OnNPCLoot(npc);
            }
        }

        public void OnNPCHit(NPC npc, int damage)
        {
            var activeHandlers = GetActiveHandlers().ToList();
            foreach (var handler in activeHandlers)
            {
                handler.OnNPCHit(npc, damage);
            }
        }
        private IEnumerable<MissionObjectiveHandler> GetActiveHandlers()
        {
            // Create a safe copy of active handlers before enumeration
            return handlers.Values
                .Where(h => h.Mission.Progress == MissionProgress.Active)
                .ToList(); // Materialize the list before returning
        }
    }
}