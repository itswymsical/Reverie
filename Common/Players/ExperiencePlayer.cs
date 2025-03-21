using Terraria.ModLoader.IO;
using Terraria.Audio;
using Terraria.UI;
using Reverie.Common.UI.LevelSystem;

namespace Reverie.Common.Players;

public class ExperiencePlayer : ModPlayer
{
    public int playerLevel;
    public int experienceValue;
    public int skillPoints;

    public override void Initialize()
    {
        playerLevel = 1;
        experienceValue = 0;
        skillPoints = 0;
    }

    public override void SaveData(TagCompound tag)
    {
        tag["experienceLevel"] = playerLevel;
        tag["experienceValue"] = experienceValue;
        tag["skillPoints"] = skillPoints;
    }

    public override void LoadData(TagCompound tag)
    {
        if (tag.ContainsKey("experienceLevel")) playerLevel = tag.GetInt("experienceLevel");
        
        if (tag.ContainsKey("experienceValue")) experienceValue = tag.GetInt("experienceValue");
        
        if (tag.ContainsKey("skillPoints")) skillPoints = tag.GetInt("skillPoints");
    }

    public static void AddExperience(Player player, int value)
    {
        ExperiencePlayer modPlayer = player.GetModPlayer<ExperiencePlayer>();
        if (modPlayer.playerLevel <= 99)
        {
            modPlayer.experienceValue += value;

            while (modPlayer.experienceValue >= GetNextExperienceThreshold(modPlayer.playerLevel))
            {
                modPlayer.experienceValue -= GetNextExperienceThreshold(modPlayer.playerLevel);
                modPlayer.playerLevel++;
                modPlayer.skillPoints++;

                SoundEngine.PlaySound(SoundID.AchievementComplete, player.position);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                    InGameNotificationsTracker.AddNotification(new LevelUpNotification());                   
                else
                    Main.NewText($"{player.name} Reached Level {modPlayer.playerLevel} " + $"[i:{ItemID.FallenStar}], Skill Points: {modPlayer.skillPoints}");
            }
        }
        else return;        
    }
    public static int GetNextExperienceThreshold(int level)
    {
        if (level <= 1) return Main.masterMode ? 100 : Main.expertMode ? 75 : 50;
        
        return Main.masterMode ? 175 * level : Main.expertMode ? 150 * level : 125 * level;
    }
}