using Reverie.Common.Systems.Particles;
using Terraria.ModLoader;

namespace Reverie.Common.Commands;

public class SandHazeCommand : ModCommand
{
    public override string Command => "sandhaze";

    public override CommandType Type => CommandType.Chat;

    public override string Usage => "/sandhaze [info|clear|toggle]";

    public override string Description => "Debug commands for sand haze system";

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        var manager = SandHazeManager.Instance;

        if (args.Length == 0)
        {
            caller.Reply("Sand Haze Debug Commands:");
            caller.Reply("  /sandhaze info - Show particle count and system status");
            caller.Reply("  /sandhaze clear - Clear all active particles");
            caller.Reply("  /sandhaze toggle - Toggle sand haze on/off");
            return;
        }

        switch (args[0].ToLower())
        {
            case "info":
                var config = ModContent.GetInstance<SandHazeConfig>();
                caller.Reply($"Active Particles: {manager.ActiveParticleCount}");
                caller.Reply($"Sand Haze Enabled: {config.EffectiveEnableSandHaze}");
                caller.Reply($"Current Preset: {config.Preset}");
                caller.Reply($"Performance Mode: {config.PerformanceMode}");
                break;

            case "clear":
                manager.ClearAllParticles();
                caller.Reply("All sand haze particles cleared.");
                break;

            case "toggle":
                var toggleConfig = ModContent.GetInstance<SandHazeConfig>();
                toggleConfig.EnableSandHaze = !toggleConfig.EnableSandHaze;
                caller.Reply($"Sand haze {(toggleConfig.EnableSandHaze ? "enabled" : "disabled")}");
                break;

            default:
                caller.Reply("Unknown command. Use /sandhaze for help.");
                break;
        }
    }
}