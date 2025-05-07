namespace Reverie.Core.Missions.System;

public class MissionLoadingSystem : ModSystem
{
    private static bool worldJustLoaded = false;
    private static int worldLoadCounter = 0;
    private const int LOAD_DELAY_FRAMES = 10;

    public override void OnWorldLoad()
    {
        MissionManager.Instance.OnWorldLoad();
        MissionFactory.Instance.Reset();

        worldJustLoaded = true;
        worldLoadCounter = 0;

        ModContent.GetInstance<Reverie>().Logger.Info("MissionLoadingSystem: OnWorldLoad complete");
    }

    public override void PostUpdateWorld()
    {
        if (worldJustLoaded)
        {
            worldLoadCounter++;

            if (worldLoadCounter >= LOAD_DELAY_FRAMES)
            {
                MissionManager.Instance.OnWorldFullyLoaded();
                worldJustLoaded = false;

                ModContent.GetInstance<Reverie>().Logger.Info("MissionLoadingSystem: Deferred world load complete");
            }
        }
    }
}
