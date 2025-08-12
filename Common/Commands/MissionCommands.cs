using Microsoft.Xna.Framework;
using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using Reverie.Core.Missions;
using System.Linq;
using Reverie.Core.Missions.Core;
using Reverie.Utilities;

namespace Reverie.Common.Commands;

// Helper method to be used by all commands
public static class MissionCommandHelper
{
    public static int TryParseMissionIdentifier(string identifier)
    {
        // First check if it's a simple numeric ID
        if (int.TryParse(identifier, out var missionId))
        {
            return missionId;
        }

        // Try to match by name using reflection on MissionID class
        var missionIdType = typeof(MissionID);
        var fields = missionIdType.GetFields();

        foreach (var field in fields)
        {
            if (string.Equals(field.Name, identifier, StringComparison.OrdinalIgnoreCase))
            {
                return (int)field.GetValue(null);
            }
        }

        // Return -1 if no match found
        return -1;
    }
}

public class UnlockMissionCommand : ModCommand
{
    public override string Command => "unlockm";
    public override CommandType Type => CommandType.Chat | CommandType.World;
    public override string Usage => "/unlockm <missionID or name>";
    public override string Description => "Unlocks the specified mission (e.g., 1 or 'JourneysBegin').";

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        if (args.Length < 1)
        {
            caller.Reply("Usage: " + Usage, Color.Red);
            return;
        }

        var missionId = MissionCommandHelper.TryParseMissionIdentifier(args[0]);
        if (missionId == -1)
        {
            caller.Reply($"Could not find mission with identifier '{args[0]}'.", Color.Red);
            return;
        }

        var mission = MissionUtils.GetMission(caller.Player, missionId);
        if (mission == null)
        {
            caller.Reply($"Mission with ID {missionId} does not exist.", Color.Red);
            return;
        }

        MissionUtils.UnlockMission(caller.Player, missionId, true);
        caller.Reply($"Mission '{mission.Name}' (ID: {missionId}) unlocked successfully!", Color.Green);
    }
}

public class StartMissionCommand : ModCommand
{
    public override string Command => "startm";
    public override CommandType Type => CommandType.Chat | CommandType.World;
    public override string Usage => "/startm <missionID or name>";
    public override string Description => "Starts the specified mission (e.g., 1 or 'JourneysBegin').";

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        if (args.Length < 1)
        {
            caller.Reply("Usage: " + Usage, Color.Red);
            return;
        }

        var missionId = MissionCommandHelper.TryParseMissionIdentifier(args[0]);
        if (missionId == -1)
        {
            caller.Reply($"Could not find mission with identifier '{args[0]}'.", Color.Red);
            return;
        }

        var mission = MissionUtils.GetMission(caller.Player, missionId);
        if (mission == null)
        {
            caller.Reply($"Mission with ID {missionId} does not exist.", Color.Red);
            return;
        }

        if (mission.Status != MissionStatus.Unlocked)
        {
            caller.Reply($"Mission '{mission.Name}' is not unlocked yet. Use /unlockm first.", Color.Yellow);
            return;
        }

        MissionUtils.StartMission(caller.Player, missionId);
        caller.Reply($"Mission '{mission.Name}' (ID: {missionId}) started successfully!", Color.Green);
    }
}


public class CompleteMissionCommand : ModCommand
{
    public override string Command => "completem";
    public override CommandType Type => CommandType.Chat | CommandType.World;
    public override string Usage => "/completem <missionID or name>";
    public override string Description => "Completes the specified mission (e.g., 1 or 'JourneysBegin').";

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        if (args.Length < 1)
        {
            caller.Reply("Usage: " + Usage, Color.Red);
            return;
        }

        var missionId = MissionCommandHelper.TryParseMissionIdentifier(args[0]);
        if (missionId == -1)
        {
            caller.Reply($"Could not find mission with identifier '{args[0]}'.", Color.Red);
            return;
        }

        var mission = MissionUtils.GetMission(caller.Player, missionId);
        if (mission == null)
        {
            caller.Reply($"Mission with ID {missionId} does not exist.", Color.Red);
            return;
        }

        MissionUtils.CompleteMission(caller.Player, missionId);
        caller.Reply($"Mission '{mission.Name}' (ID: {missionId}) completed successfully!", Color.Green);
    }
}

public class ListSideMissionsCommand : ModCommand
{
    public override string Command => "listsides";
    public override CommandType Type => CommandType.Chat | CommandType.World;
    public override string Usage => "/listsides [available|active|completed|all]";
    public override string Description => "Lists side missions by category. Defaults to all side missions.";

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        var missionPlayer = caller.Player.GetModPlayer<MissionPlayer>();
        string filter = args.Length > 0 ? args[0].ToLower() : "all";

        switch (filter)
        {
            case "available":
                ListSideMissionsByState(caller, missionPlayer, MissionProgress.Inactive, MissionStatus.Unlocked);
                break;
            case "active":
                ListSideMissionsByState(caller, missionPlayer, MissionProgress.Ongoing, null);
                break;
            case "completed":
                ListSideMissionsByState(caller, missionPlayer, MissionProgress.Completed, null);
                break;
            case "all":
            default:
                caller.Reply("=== All Side Missions ===", Color.LightGreen);
                ListSideMissionsByState(caller, missionPlayer, MissionProgress.Inactive, MissionStatus.Unlocked);
                ListSideMissionsByState(caller, missionPlayer, MissionProgress.Ongoing, null);
                ListSideMissionsByState(caller, missionPlayer, MissionProgress.Completed, null);
                break;
        }
    }

    private void ListSideMissionsByState(CommandCaller caller, MissionPlayer missionPlayer,
        MissionProgress progress, MissionStatus? availability)
    {
        // Only check sideline missions
        var missions = missionPlayer.sidelineMissions.Values
            .Where(m => m.Progress == progress &&
                       (availability == null || m.Status == availability))
            .ToList();

        if (missions.Count > 0)
        {
            string title = progress switch
            {
                MissionProgress.Inactive => "=== Available Side Missions ===",
                MissionProgress.Ongoing => "=== Ongoing Side Missions ===",
                MissionProgress.Completed => "=== Completed Side Missions ===",
                _ => "=== Side Missions ==="
            };

            Color titleColor = progress switch
            {
                MissionProgress.Inactive => Color.LightBlue,
                MissionProgress.Ongoing => Color.Yellow,
                MissionProgress.Completed => Color.Green,
                _ => Color.White
            };

            caller.Reply(title, titleColor);

            foreach (var mission in missions)
            {
                if (progress == MissionProgress.Ongoing)
                {
                    var currentSet = mission.Objective[mission.CurrentIndex];
                    int completedObjectives = currentSet.Objectives.Count(o => o.IsCompleted);
                    int totalObjectives = currentSet.Objectives.Count;

                    caller.Reply($"ID: {mission.ID} - {mission.Name} - Progress: {completedObjectives}/{totalObjectives}", Color.White);
                }
                else
                {
                    caller.Reply($"ID: {mission.ID} - {mission.Name}", Color.White);
                }
            }
        }
        else if (progress == MissionProgress.Inactive)
        {
            caller.Reply("No available side missions.", Color.Gray);
        }
        else if (progress == MissionProgress.Ongoing)
        {
            caller.Reply("No active side missions.", Color.Gray);
        }
        else if (progress == MissionProgress.Completed)
        {
            caller.Reply("No completed side missions.", Color.Gray);
        }
    }
}

public class ListMainlineMissionsCommand : ModCommand
{
    public override string Command => "listmainline";
    public override CommandType Type => CommandType.Chat | CommandType.World;
    public override string Usage => "/listmainline [available|active|completed|all]";
    public override string Description => "Lists mainline missions by category. Defaults to all mainline missions.";

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        string filter = args.Length > 0 ? args[0].ToLower() : "all";

        switch (filter)
        {
            case "available":
                ListMainlineMissionsByState(caller, MissionProgress.Inactive, MissionStatus.Unlocked);
                break;
            case "active":
                ListMainlineMissionsByState(caller, MissionProgress.Ongoing, null);
                break;
            case "completed":
                ListMainlineMissionsByState(caller, MissionProgress.Completed, null);
                break;
            case "all":
            default:
                caller.Reply("=== All Mainline Missions ===", Color.LightGreen);
                ListMainlineMissionsByState(caller, MissionProgress.Inactive, MissionStatus.Unlocked);
                ListMainlineMissionsByState(caller, MissionProgress.Ongoing, null);
                ListMainlineMissionsByState(caller, MissionProgress.Completed, null);
                break;
        }
    }

    private void ListMainlineMissionsByState(CommandCaller caller, MissionProgress progress, MissionStatus? availability)
    {
        var missions = WorldMissionSystem.Instance.GetAllMainlineMissions()
            .Where(m => m.Progress == progress &&
                       (availability == null || m.Status == availability))
            .ToList();

        if (missions.Count > 0)
        {
            string title = progress switch
            {
                MissionProgress.Inactive => "=== Available Mainline Missions ===",
                MissionProgress.Ongoing => "=== Ongoing Mainline Missions ===",
                MissionProgress.Completed => "=== Completed Mainline Missions ===",
                _ => "=== Mainline Missions ==="
            };

            Color titleColor = progress switch
            {
                MissionProgress.Inactive => Color.LightBlue,
                MissionProgress.Ongoing => Color.Yellow,
                MissionProgress.Completed => Color.Green,
                _ => Color.White
            };

            caller.Reply(title, titleColor);

            foreach (var mission in missions)
            {
                if (progress == MissionProgress.Ongoing)
                {
                    var currentSet = mission.Objective[mission.CurrentIndex];
                    int completedObjectives = currentSet.Objectives.Count(o => o.IsCompleted);
                    int totalObjectives = currentSet.Objectives.Count;

                    caller.Reply($"ID: {mission.ID} - {mission.Name} - Progress: {completedObjectives}/{totalObjectives}", Color.White);
                }
                else
                {
                    caller.Reply($"ID: {mission.ID} - {mission.Name}", Color.White);
                }
            }
        }
        else if (progress == MissionProgress.Inactive)
        {
            caller.Reply("No available mainline missions.", Color.Gray);
        }
        else if (progress == MissionProgress.Ongoing)
        {
            caller.Reply("No active mainline missions.", Color.Gray);
        }
        else if (progress == MissionProgress.Completed)
        {
            caller.Reply("No completed mainline missions.", Color.Gray);
        }
    }
}