using Microsoft.Xna.Framework;
using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using Reverie.Core.Missions;
using System.Linq;
using Reverie.Core.Missions.Core;

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

        var missionPlayer = caller.Player.GetModPlayer<MissionPlayer>();
        var mission = missionPlayer.GetMission(missionId);

        if (mission == null)
        {
            caller.Reply($"Mission with ID {missionId} does not exist.", Color.Red);
            return;
        }

        missionPlayer.UnlockMission(missionId, true);
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

        var missionPlayer = caller.Player.GetModPlayer<MissionPlayer>();
        var mission = missionPlayer.GetMission(missionId);

        if (mission == null)
        {
            caller.Reply($"Mission with ID {missionId} does not exist.", Color.Red);
            return;
        }

        if (mission.Availability != MissionAvailability.Unlocked)
        {
            caller.Reply($"Mission '{mission.Name}' is not unlocked yet. Use /unlockmission first.", Color.Yellow);
            return;
        }

        missionPlayer.StartMission(missionId);
        caller.Reply($"Mission '{mission.Name}' (ID: {missionId}) started successfully!", Color.Green);
    }
}

public class ResetMissionCommand : ModCommand
{
    public override string Command => "resetm";
    public override CommandType Type => CommandType.Chat | CommandType.World;
    public override string Usage => "/resetm <missionID or name>";
    public override string Description => "Resets the specified mission (e.g., 1 or 'JourneysBegin').";

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

        var missionPlayer = caller.Player.GetModPlayer<MissionPlayer>();
        var mission = missionPlayer.GetMission(missionId);

        if (mission == null)
        {
            caller.Reply($"Mission with ID {missionId} does not exist.", Color.Red);
            return;
        }

        missionPlayer.ResetMission(missionId);
        caller.Reply($"Mission '{mission.Name}' (ID: {missionId}) reset successfully!", Color.Green);
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

        var missionPlayer = caller.Player.GetModPlayer<MissionPlayer>();
        var mission = missionPlayer.GetMission(missionId);

        if (mission == null)
        {
            caller.Reply($"Mission with ID {missionId} does not exist.", Color.Red);
            return;
        }

        missionPlayer.CompleteMission(missionId);
        caller.Reply($"Mission '{mission.Name}' (ID: {missionId}) completed successfully!", Color.Green);
    }
}

public class ResetSideMissionsCommand : ModCommand
{
    public override string Command => "resetsides";
    public override CommandType Type => CommandType.Chat | CommandType.World;
    public override string Usage => "/resetsides [missionID or name]";
    public override string Description => "Resets all side missions or a specific side mission.";

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        var missionPlayer = caller.Player.GetModPlayer<MissionPlayer>();

        // If a specific mission is specified
        if (args.Length > 0)
        {
            int missionId = MissionCommandHelper.TryParseMissionIdentifier(args[0]);
            if (missionId == -1)
            {
                caller.Reply($"Could not find mission with identifier '{args[0]}'.", Color.Red);
                return;
            }

            var mission = missionPlayer.GetMission(missionId);
            if (mission == null)
            {
                caller.Reply($"Mission with ID {missionId} does not exist.", Color.Red);
                return;
            }

            // Check if this is a side mission
            if (mission.IsMainline)
            {
                caller.Reply($"Mission '{mission.Name}' is a mainline mission, not a side mission. Use /resetmission instead.", Color.Yellow);
                return;
            }

            // Reset the specific side mission
            missionPlayer.ResetMission(missionId);
            caller.Reply($"Side mission '{mission.Name}' (ID: {missionId}) reset successfully!", Color.Green);
            return;
        }

        // Reset all side missions if no specific one is provided
        int resetCount = 0;

        // Get all missions from the player's dictionary
        var sideMissions = missionPlayer.missionDict.Values
            .Where(m => !m.IsMainline)
            .ToList();

        if (sideMissions.Count == 0)
        {
            caller.Reply("No side missions found to reset.", Color.Yellow);
            return;
        }

        // Reset each side mission
        foreach (var mission in sideMissions)
        {
            missionPlayer.ResetMission(mission.ID);
            resetCount++;
        }

        caller.Reply($"Reset {resetCount} side missions successfully!", Color.Green);
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
                ListSideMissionsByState(caller, missionPlayer, MissionProgress.Inactive, MissionAvailability.Unlocked);
                break;
            case "active":
                ListSideMissionsByState(caller, missionPlayer, MissionProgress.Active, null);
                break;
            case "completed":
                ListSideMissionsByState(caller, missionPlayer, MissionProgress.Completed, null);
                break;
            case "all":
            default:
                caller.Reply("=== All Side Missions ===", Color.LightGreen);
                ListSideMissionsByState(caller, missionPlayer, MissionProgress.Inactive, MissionAvailability.Unlocked);
                ListSideMissionsByState(caller, missionPlayer, MissionProgress.Active, null);
                ListSideMissionsByState(caller, missionPlayer, MissionProgress.Completed, null);
                break;
        }
    }

    private void ListSideMissionsByState(CommandCaller caller, MissionPlayer missionPlayer,
        MissionProgress progress, MissionAvailability? availability)
    {
        var missions = missionPlayer.missionDict.Values
            .Where(m => !m.IsMainline && m.Progress == progress &&
                       (availability == null || m.Availability == availability))
            .ToList();

        if (missions.Count > 0)
        {
            string title = progress switch
            {
                MissionProgress.Inactive => "=== Available Side Missions ===",
                MissionProgress.Active => "=== Active Side Missions ===",
                MissionProgress.Completed => "=== Completed Side Missions ===",
                _ => "=== Side Missions ==="
            };

            Color titleColor = progress switch
            {
                MissionProgress.Inactive => Color.LightBlue,
                MissionProgress.Active => Color.Yellow,
                MissionProgress.Completed => Color.Green,
                _ => Color.White
            };

            caller.Reply(title, titleColor);

            foreach (var mission in missions)
            {
                if (progress == MissionProgress.Active)
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
        else if (progress == MissionProgress.Active)
        {
            caller.Reply("No active side missions.", Color.Gray);
        }
        else if (progress == MissionProgress.Completed)
        {
            caller.Reply("No completed side missions.", Color.Gray);
        }
    }
}
