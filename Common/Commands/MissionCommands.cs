using Terraria.ModLoader;
using Reverie.Common.Players;
using Terraria;
using Reverie.Core.Missions;
using System;
using Microsoft.Xna.Framework;

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
                    missionEntry.State = MissionAvailability.Unlocked;
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
            mission.State = MissionAvailability.Unlocked;
            Main.NewText($"Mission '{mission.MissionData.Name}' has been reset.", 255, 255, 0);
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
            missionPlayer.DebugMissionAvailabilitys();
        }
    }
}