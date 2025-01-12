using Terraria.ModLoader;
using Reverie.Common.Players;
using Terraria;
using Reverie.Core.Missions;

namespace Reverie.Core.Commands
{
    public class ResetMissionCommand : ModCommand
    {
        public override CommandType Type
            => CommandType.Chat;

        public override string Command
            => "resetmission";

        public override string Usage
            => "/resetmission <missionId|all>" +
            "\n missionId — ID of the mission to reset" +
            "\n all — Reset all missions";

        public override string Description
            => "Reset a specific mission or all missions by ID";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length == 0)
                throw new UsageException("Expected mission ID or 'all'.");

            MissionPlayer missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();

            if (args[0].ToLower() == "all")
            {
                foreach (var missionEntry in missionPlayer.missionDict.Values)
                {
                    missionEntry.Reset();
                    missionEntry.State = MissionState.Unlocked;
                }
                Main.NewText("All missions have been reset.", 255, 255, 0);
                return;
            }

            if (!int.TryParse(args[0], out int missionId))
                throw new UsageException("Mission ID must be a number.");

            var mission = missionPlayer.GetMission(missionId);
            if (mission == null)
                throw new UsageException($"No mission found with ID: {missionId}");

            mission.Reset();
            mission.State = MissionState.Unlocked;
            Main.NewText($"Mission '{mission.MissionData.Name}' has been reset.", 255, 255, 0);
        }
    }

    public class CompleteMissionCommand : ModCommand
    {
        public override CommandType Type
            => CommandType.Chat;

        public override string Command
            => "completemission";

        public override string Usage
            => "/completemission <missionId|all>" +
            "\n missionId — ID of the mission to complete" +
            "\n all — complete all missions";

        public override string Description
            => "Complete a specific mission or all missions by ID";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length == 0)
                throw new UsageException("Expected mission ID or 'all'.");

            MissionPlayer missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();

            if (args[0].ToLower() == "all")
            {
                foreach (var missionEntry in missionPlayer.missionDict.Values)
                {
                    missionEntry.Complete();
                    missionEntry.State = MissionState.Completed;
                }
                Main.NewText("All missions have been set to complete.", 255, 255, 0);
                return;
            }

            if (!int.TryParse(args[0], out int missionId))
                throw new UsageException("Mission ID must be a number.");

            var mission = missionPlayer.GetMission(missionId);
            if (mission == null)
                throw new UsageException($"No mission found with ID: {missionId}");

            mission.Complete();
            mission.State = MissionState.Completed;
            Main.NewText($"Mission '{mission.MissionData.Name}' has been set to complete.", 255, 255, 0);
        }
    }

    public class DebugMissionCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "debugmissions";
        public override string Description => "Shows the current state of all missions";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            MissionPlayer missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
            missionPlayer.DebugMissionStates();
        }
    }
}