using Reverie.Common.UI.LevelUI;
using Reverie.Common.UI.RelicUI;
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
            // Share XP with equipped relic before adding to player
            RelicPlayer relicPlayer = player.GetModPlayer<RelicPlayer>();
            relicPlayer.ShareExperienceWithRelic(value);

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
        if (level <= 1) return Main.masterMode ? 325 : Main.expertMode ? 300 : 280;

        return Main.masterMode ? (int)377.37753541f * level : Main.expertMode ? (int)327.37753541f * level : (int)302.37753541f * level;
    }
}