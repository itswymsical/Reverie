using Terraria;
using Terraria.Audio;
using Terraria.ModLoader.IO;

namespace Reverie.Common.Players;

public class ExperiencePlayer : ModPlayer
{
    public int expCapacity;
    public int curExp;

    public int MaxExperience => expCapacity * 500;

    public override void Initialize()
    {
        expCapacity = 1;
        curExp = 0;
    }

    public override void SaveData(TagCompound tag)
    {
        tag["expCapacity"] = expCapacity;
        tag["curExp"] = curExp;
    }

    public override void LoadData(TagCompound tag)
    {
        if (tag.ContainsKey("expCapacity")) expCapacity = tag.GetInt("expCapacity");
        if (tag.ContainsKey("curExp")) curExp = tag.GetInt("curExp");
    }

    public static void AddExperience(Player player, int value)
    {
        ExperiencePlayer modPlayer = player.GetModPlayer<ExperiencePlayer>();

        int newExp = modPlayer.curExp + value;
        int maxExp = modPlayer.MaxExperience;

        modPlayer.curExp = Math.Min(newExp, maxExp);

        if (modPlayer.curExp == maxExp && newExp > maxExp)
        {
            SoundEngine.PlaySound(SoundID.MaxMana, player.position);
        }
    }

    public bool TrySpendExperience(int amount)
    {
        if (curExp >= amount)
        {
            curExp -= amount;
            return true;
        }
        return false;
    }

    public bool CanAfford(int amount) => curExp >= amount;

    public void IncreaseCapacity(int amount = 1)
    {
        if (expCapacity < 20)
        {
            expCapacity = Math.Min(20, expCapacity + amount);
            CombatText.NewText(new Rectangle((int)Player.position.X, (int)Player.position.Y, Player.width, Player.height), new(15, 119, 208), 500);
        }
    }

    public float ExperienceRatio => (float)curExp / MaxExperience;
}