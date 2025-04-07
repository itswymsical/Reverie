using Reverie.Core.Missions;
using System.Collections.Generic;
using System.Linq;
using Terraria.UI;

namespace Reverie.Common.UI.Missions;

public class MissionNotificationManager : ModSystem
{
    private static MissionNotificationManager _instance;
    public static MissionNotificationManager Instance => _instance;

    // To track which NPCs have missions available and notifications created
    private Dictionary<int, bool> npcMissionNotifications = new Dictionary<int, bool>();

    // Store references to created notifications
    private List<NPCMissionNotification> activeNotifications = new List<NPCMissionNotification>();

    public override void Load()
    {
        _instance = this;
    }

    public override void Unload()
    {
        _instance = null;
    }

    public override void OnWorldLoad()
    {
        ClearAllNotifications();
    }

    public override void OnWorldUnload()
    {
        ClearAllNotifications();
    }

    /// <summary>
    /// Resets all notification tracking - call on player enter world
    /// </summary>
    public void Reset()
    {
        npcMissionNotifications.Clear();
        activeNotifications.Clear();
    }

    /// <summary>
    /// Updates notifications for NPCs with available missions
    /// Call this from your main player update method
    /// </summary>
    public void UpdateMissionNotifications()
    {
        var missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();

        // Check for NPCs with available missions
        for (int i = 0; i < Main.npc.Length; i++)
        {
            NPC npc = Main.npc[i];
            if (!npc.active || !(npc.isLikeATownNPC || npc.townNPC))
                continue;

            bool hasAvailableMission = missionPlayer.NPCHasAvailableMission(npc.type);
            bool hasActiveMission = missionPlayer.ActiveMissions().Any(m => npc.type == m.Employer);

            // If NPC has available mission and no active mission
            if (hasAvailableMission && !hasActiveMission)
            {
                // Check if we already created a notification for this NPC
                if (!npcMissionNotifications.ContainsKey(i) || !npcMissionNotifications[i])
                {
                    // Get the available mission
                    var mission = missionPlayer.AvailableMissions().FirstOrDefault(m => npc.type == m.Employer);
                    if (mission != null)
                    {
                        // Create new notification
                        var notification = new NPCMissionNotification(npc, mission, npc.Top);
                        InGameNotificationsTracker.AddNotification(notification);
                        activeNotifications.Add(notification);

                        // Mark this NPC as having a notification
                        npcMissionNotifications[i] = true;
                    }
                }
            }
            // If NPC no longer has an available mission but we created a notification before
            else if (npcMissionNotifications.ContainsKey(i) && npcMissionNotifications[i])
            {
                // Mark the notification for removal
                npcMissionNotifications[i] = false;
            }
        }

        // Clean up removed notifications
        CleanupNotifications();
    }

    /// <summary>
    /// Removes any notifications for NPCs that no longer have missions
    /// or for NPCs that are no longer active
    /// </summary>
    private void CleanupNotifications()
    {
        activeNotifications.RemoveAll(notification => notification.ShouldBeRemoved);
    }

    /// <summary>
    /// Removes all active notifications
    /// </summary>
    public void ClearAllNotifications()
    {
        npcMissionNotifications.Clear();
        activeNotifications.Clear();
    }

    /// <summary>
    /// Checks if an NPC already has a mission notification
    /// </summary>
    public bool HasNotification(int npcIndex)
    {
        return npcMissionNotifications.ContainsKey(npcIndex) && npcMissionNotifications[npcIndex];
    }
}