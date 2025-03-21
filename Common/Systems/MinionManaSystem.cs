using System.Collections.Generic;


namespace Reverie.Common.Systems;

public class MinionManaSystem : ModSystem
{
    private readonly Dictionary<int, int> minionDrainTimers = [];
    public override void PostUpdatePlayers()
    {
        foreach (Player player in Main.player)
        {
            if (player.active)
            {
                DrainManaForMinions(player);
            }
        }
    }

    private void DrainManaForMinions(Player player)
    {
        // Track if any mana was drained this tick
        bool manaDrained = false;
        int drainTime = 80;
        // Check each projectile
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile proj = Main.projectile[i];

            // Check if it's an active minion belonging to this player
            if (proj.active && proj.owner == player.whoAmI && proj.minion)
            {
                player.manaRegen = 0;
                // Get or initialize the drain timer for this projectile
                if (!minionDrainTimers.ContainsKey(i))
                {
                    minionDrainTimers[i] = 0;
                }

                // Increment the timer
                minionDrainTimers[i]++;

                int drainAmount = 1; // Default: drain 1 mana
                // For custom minion classes,
                // if (proj.ModProjectile is Minion minion)
                // {
                //     drainTime = minion.drainTime;
                //     drainAmount = minion.drainAmount;
                // }

                if (minionDrainTimers[i] >= drainTime)
                {
                    
                    minionDrainTimers[i] = 0;

                    if (player.statMana > 0)
                    {
                        player.statMana -= drainAmount;

                        manaDrained = true;
                    }
                }
            }
            else if (minionDrainTimers.ContainsKey(i))
            {
                minionDrainTimers.Remove(i);
            }
        }

        if (manaDrained)
        {
            if (player.statMana <= 0)
            {
                DespawnAllMinions(player);
                player.manaRegenDelay = 20f;
            }
        }
    }

    private void DespawnAllMinions(Player player)
    {
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile proj = Main.projectile[i];
            if (proj.active && proj.owner == player.whoAmI && proj.minion)
            {
                proj.Kill();

                if (minionDrainTimers.ContainsKey(i))
                {
                    minionDrainTimers.Remove(i);
                }
            }
        }
    }
}

public class MinionSlotRemovalSystem : ModPlayer
{
    public override void PostUpdateEquips()
    {
        Player.maxMinions = 50;
    }
}