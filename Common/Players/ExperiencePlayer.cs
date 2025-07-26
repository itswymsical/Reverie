using Reverie.Common.UI.LevelUI;
using Terraria.Audio;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Reverie.Common.Players;

public class ExperiencePlayer : ModPlayer
{
    public int expLevel;
    public int expValue;
    public int skillPoints;

    public override void Initialize()
    {
        expLevel = 1;
        expValue = 0;
        skillPoints = 0;
    }

    public override void SaveData(TagCompound tag)
    {
        tag["expLevel"] = expLevel;
        tag["expValue"] = expValue;
        tag["skillPoints"] = skillPoints;
    }

    public override void LoadData(TagCompound tag)
    {
        if (tag.ContainsKey("expLevel")) expLevel = tag.GetInt("expLevel");
        
        if (tag.ContainsKey("expValue")) expValue = tag.GetInt("expValue");
        
        if (tag.ContainsKey("skillPoints")) skillPoints = tag.GetInt("skillPoints");
    }

    public static void AddExperience(Player player, int value)
    {
        ExperiencePlayer modPlayer = player.GetModPlayer<ExperiencePlayer>();
        if (modPlayer.expLevel <= 60)
        {
            modPlayer.expValue += value;

            while (modPlayer.expValue >= GetNextExperienceThreshold(modPlayer.expLevel))
            {
                modPlayer.expValue -= GetNextExperienceThreshold(modPlayer.expLevel);
                modPlayer.expLevel++;
                modPlayer.skillPoints++;

                SoundEngine.PlaySound(SoundID.AchievementComplete, player.position);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                    InGameNotificationsTracker.AddNotification(new LevelNotification());                   
                else
                    Main.NewText($"{player.name} Reached Level {modPlayer.expLevel} " + $"[i:{ItemID.FallenStar}], Skill Points: {modPlayer.skillPoints}");
            }
        }
        else return;        
    }

    public static int GetNextExperienceThreshold(int level)
    {
        // Borderlands-inspired system
        int baseXP = Main.masterMode ? 120 : Main.expertMode ? 100 : 80;

        // Early game boost (levels 1-15 are faster)
        float earlyGameMultiplier = level <= 15 ? 0.7f : 1f;

        // Polynomial growth with level-based scaling
        float levelFactor = (float)Math.Pow(level, 2.15f);

        float variance = 1f + (level % 3) * 0.05f; // Small 0-10% variance

        return (int)(baseXP * levelFactor * earlyGameMultiplier * variance);
    }
}