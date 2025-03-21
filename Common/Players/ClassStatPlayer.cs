using System.IO;
using Terraria.ModLoader.IO;

namespace Reverie.Common.Players;

public enum ClassType
{
    None = 0,
    Vanguard = 1
}

public class ClassStatPlayer : ModPlayer
{
    public ClassType CurrentClass = ClassType.None;

    public int levelHealthBonus;
    public int levelManaBonus;
    public int levelMinionBonus;

    public const int HEALTH_PER_LEVEL = 4;
    public const int MANA_LEVEL_THRESHOLD = 2;
    public const int MANA_PER_LEVEL = 4;
    public const int MINION_LEVEL_THRESHOLD = 3;

    private int _lastKnownLevel = 1;

    public override void Initialize()
    {
        CurrentClass = ClassType.None;
        ResetStatBonuses();
        _lastKnownLevel = 1;
    }

    public override void ModifyMaxStats(out StatModifier health, out StatModifier mana)
    {
        health = StatModifier.Default;
        health.Base = levelHealthBonus;

        mana = StatModifier.Default;
        mana.Base = levelManaBonus;
    }

    public override void PostUpdateEquips()
    {
        Player.maxMinions += levelMinionBonus;
    }

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        ModPacket packet = Mod.GetPacket();
        packet.Write((byte)MessageType.ClassStatPlayerSync);
        packet.Write((byte)Player.whoAmI);
        packet.Write((byte)CurrentClass);
        packet.Write(levelHealthBonus);
        packet.Write(levelManaBonus);
        packet.Write(levelMinionBonus);
        packet.Send(toWho, fromWho);
    }

    public void ReceivePlayerSync(BinaryReader reader)
    {
        CurrentClass = (ClassType)reader.ReadByte();
        levelHealthBonus = reader.ReadInt32();
        levelManaBonus = reader.ReadInt32();
        levelMinionBonus = reader.ReadInt32();
    }

    public override void CopyClientState(ModPlayer targetCopy)
    {
        ClassStatPlayer clone = (ClassStatPlayer)targetCopy;
        clone.CurrentClass = CurrentClass;
        clone.levelHealthBonus = levelHealthBonus;
        clone.levelManaBonus = levelManaBonus;
        clone.levelMinionBonus = levelMinionBonus;
    }

    public override void SendClientChanges(ModPlayer clientPlayer)
    {
        ClassStatPlayer clone = (ClassStatPlayer)clientPlayer;

        if (CurrentClass != clone.CurrentClass ||
            levelHealthBonus != clone.levelHealthBonus ||
            levelManaBonus != clone.levelManaBonus ||
            levelMinionBonus != clone.levelMinionBonus)
        {
            SyncPlayer(toWho: -1, fromWho: Main.myPlayer, newPlayer: false);
        }
    }

    public override void SaveData(TagCompound tag)
    {
        tag["CurrentClass"] = (int)CurrentClass;
        tag["levelHealthBonus"] = levelHealthBonus;
        tag["levelManaBonus"] = levelManaBonus;
        tag["levelMinionBonus"] = levelMinionBonus;
    }

    public override void LoadData(TagCompound tag)
    {
        CurrentClass = tag.ContainsKey("CurrentClass") ? (ClassType)tag.GetInt("CurrentClass") : ClassType.None;
        levelHealthBonus = tag.ContainsKey("levelHealthBonus") ? tag.GetInt("levelHealthBonus") : 0;
        levelManaBonus = tag.ContainsKey("levelManaBonus") ? tag.GetInt("levelManaBonus") : 0;
        levelMinionBonus = tag.ContainsKey("levelMinionBonus") ? tag.GetInt("levelMinionBonus") : 0;
    }

    public void SetClass(ClassType classType)
    {
        if (CurrentClass != ClassType.None)
        {
            ResetStatBonuses();
        }

        CurrentClass = classType;

        if (CurrentClass != ClassType.None)
        {
            ExperiencePlayer expPlayer = Player.GetModPlayer<ExperiencePlayer>();
            ApplyLevelUpStats(expPlayer.playerLevel);
        }
    }

    public void ApplyLevelUpStats(int level)
    {
        if (CurrentClass == ClassType.None)
            return;

        levelHealthBonus = level * HEALTH_PER_LEVEL;

        levelManaBonus = (level / MANA_LEVEL_THRESHOLD) * MANA_PER_LEVEL;

        levelMinionBonus = level / MINION_LEVEL_THRESHOLD;

        switch (CurrentClass)
        {
            case ClassType.Vanguard:
                levelHealthBonus += (int)(levelHealthBonus * 0.1f);
                break;
        }
    }

    private void ResetStatBonuses()
    {
        levelHealthBonus = 0;
        levelManaBonus = 0;
        levelMinionBonus = 0;
    }

    public override void PostUpdate()
    {
        ExperiencePlayer expPlayer = Player.GetModPlayer<ExperiencePlayer>();

        int currentLevel = expPlayer.playerLevel;

        if (currentLevel > _lastKnownLevel)
        {
            ApplyLevelUpStats(currentLevel);

            _lastKnownLevel = currentLevel;
        }
    }

    public override void OnEnterWorld()
    {
        ExperiencePlayer expPlayer = Player.GetModPlayer<ExperiencePlayer>();
        _lastKnownLevel = expPlayer.playerLevel;

        ApplyLevelUpStats(expPlayer.playerLevel);
    }
}