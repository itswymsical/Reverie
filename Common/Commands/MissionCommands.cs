using Reverie.Core.Missions;
using System.Linq;
using System.Reflection;

namespace Reverie.Common.Commands;

public static class MissionCommandHelper
{
    public static int TryParseMissionIdentifier(string identifier)
    {
        // Numeric ID
        if (int.TryParse(identifier, out var missionId))
            return missionId;

        // Match by name in MissionID class
        var field = typeof(MissionID).GetFields()
            .FirstOrDefault(f => string.Equals(f.Name, identifier, StringComparison.OrdinalIgnoreCase));

        return field != null ? (int)field.GetValue(null) : -1;
    }

    public static Mission GetOrCreateMission(int missionId, Player player)
    {
        // Try to create mission
        var mission = MissionSystem.CreateMission(missionId);
        if (mission == null)
            return null;

        // Get or create from appropriate storage
        return mission.IsMainline
            ? MissionWorld.Instance.GetOrCreateMission(missionId)
            : player.GetModPlayer<MissionPlayer>().GetOrCreateMission(missionId);
    }
}

public class UnlockMissionCommand : ModCommand
{
    public override string Command => "unlockm";
    public override CommandType Type => CommandType.Chat | CommandType.World;
    public override string Usage => "/unlockm <missionID or name>";
    public override string Description => "Unlocks the specified mission";

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
            caller.Reply($"Mission '{args[0]}' not found", Color.Red);
            return;
        }

        var mission = MissionCommandHelper.GetOrCreateMission(missionId, caller.Player);
        if (mission == null)
        {
            caller.Reply($"Mission ID {missionId} does not exist", Color.Red);
            return;
        }

        // Unlock via appropriate storage
        if (mission.IsMainline)
            MissionWorld.Instance.UnlockMission(missionId);
        else
            caller.Player.GetModPlayer<MissionPlayer>().UnlockMission(missionId);

        caller.Reply($"Unlocked: {mission.Name}", Color.Green);
    }
}

public class StartMissionCommand : ModCommand
{
    public override string Command => "startm";
    public override CommandType Type => CommandType.Chat | CommandType.World;
    public override string Usage => "/startm <missionID or name>";
    public override string Description => "Starts the specified mission";

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
            caller.Reply($"Mission '{args[0]}' not found", Color.Red);
            return;
        }

        var mission = MissionCommandHelper.GetOrCreateMission(missionId, caller.Player);
        if (mission == null)
        {
            caller.Reply($"Mission ID {missionId} does not exist", Color.Red);
            return;
        }

        if (mission.Status != MissionStatus.Unlocked)
        {
            caller.Reply($"Mission '{mission.Name}' is locked. Use /unlockm first", Color.Yellow);
            return;
        }

        // Start via appropriate storage
        if (mission.IsMainline)
            MissionWorld.Instance.StartMission(missionId);
        else
            caller.Player.GetModPlayer<MissionPlayer>().StartMission(missionId);

        caller.Reply($"Started: {mission.Name}", Color.Green);
    }
}

public class ResetMissionCommand : ModCommand
{
    public override string Command => "resetm";
    public override CommandType Type => CommandType.Chat | CommandType.World;
    public override string Usage => "/resetm <missionID or name>";
    public override string Description => "Resets the specified mission";

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
            caller.Reply($"Mission '{args[0]}' not found", Color.Red);
            return;
        }

        var mission = MissionCommandHelper.GetOrCreateMission(missionId, caller.Player);
        if (mission == null)
        {
            caller.Reply($"Mission ID {missionId} does not exist", Color.Red);
            return;
        }

        // Reset the mission
        mission.Reset();
        caller.Reply($"Reset: {mission.Name}", Color.Green);
    }
}

public class CompleteMissionCommand : ModCommand
{
    public override string Command => "completem";
    public override CommandType Type => CommandType.Chat | CommandType.World;
    public override string Usage => "/completem <missionID or name>";
    public override string Description => "Completes the specified mission";

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
            caller.Reply($"Mission '{args[0]}' not found", Color.Red);
            return;
        }

        var mission = MissionCommandHelper.GetOrCreateMission(missionId, caller.Player);
        if (mission == null)
        {
            caller.Reply($"Mission ID {missionId} does not exist", Color.Red);
            return;
        }

        // Force complete all objectives
        foreach (var set in mission.ObjectiveList)
        {
            foreach (var obj in set.Objective)
            {
                obj.Count = obj.RequiredCount;
                obj.IsCompleted = true;
            }
        }

        // Complete via appropriate storage
        if (mission.IsMainline)
            MissionWorld.Instance.CompleteMission(missionId);
        else
            caller.Player.GetModPlayer<MissionPlayer>().CompleteMission(missionId);

        caller.Reply($"Completed: {mission.Name}", Color.Green);
    }
}

public class ListMissionsCommand : ModCommand
{
    public override string Command => "listm";
    public override CommandType Type => CommandType.Chat | CommandType.World;
    public override string Usage => "/listm [mainline|sideline]";
    public override string Description => "Lists available missions";

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        var filter = args.Length > 0 ? args[0].ToLower() : "all";

        var missionPlayer = caller.Player.GetModPlayer<MissionPlayer>();

        if (filter == "all" || filter == "mainline")
        {
            caller.Reply("=== Mainline Missions ===", Color.Gold);
            var mainline = MissionWorld.Instance.GetType()
                .GetField("mainlineMissions", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(MissionWorld.Instance) as System.Collections.Generic.Dictionary<int, Mission>;

            if (mainline?.Any() == true)
            {
                foreach (var mission in mainline.Values)
                {
                    var status = mission.Progress switch
                    {
                        MissionProgress.Ongoing => "[ACTIVE]",
                        MissionProgress.Completed => "[DONE]",
                        _ => mission.Status == MissionStatus.Unlocked ? "[READY]" : "[LOCKED]"
                    };
                    caller.Reply($"  {status} {mission.ID}: {mission.Name}", Color.White);
                }
            }
            else
            {
                caller.Reply("  No mainline missions", Color.Gray);
            }
        }

        if (filter == "all" || filter == "sideline")
        {
            caller.Reply("=== Sideline Missions ===", Color.CornflowerBlue);
            var sideline = missionPlayer.ActiveMissions()
                .Concat(missionPlayer.AvailableMissions())
                .Distinct();

            if (sideline.Any())
            {
                foreach (var mission in sideline)
                {
                    var status = mission.Progress switch
                    {
                        MissionProgress.Ongoing => "[ACTIVE]",
                        MissionProgress.Completed => "[DONE]",
                        _ => "[READY]"
                    };
                    caller.Reply($"  {status} {mission.ID}: {mission.Name}", Color.White);
                }
            }
            else
            {
                caller.Reply("  No sideline missions", Color.Gray);
            }
        }
    }
}