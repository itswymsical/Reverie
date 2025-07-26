using Reverie.Common.Players;
using System.Collections.Generic;
using System.Linq;

namespace Reverie.Common.NPCs;

public class ExperienceGlobalNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public Dictionary<int, int> playerDamage = [];
    public int totalDamageDealt = 0;

    public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
    {
        base.OnHitByItem(npc, player, item, hit, damageDone);
        if (npc.immortal) return;

        if (!playerDamage.ContainsKey(player.whoAmI))
        {
            playerDamage[player.whoAmI] = 0;
        }
        playerDamage[player.whoAmI] += hit.Damage;
        totalDamageDealt += hit.Damage;
    }

    public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        base.OnHitByProjectile(npc, projectile, hit, damageDone);
        if (npc.immortal) return;
        if (projectile.owner >= 0 && projectile.owner < Main.maxPlayers)
        {
            var player = Main.player[projectile.owner];
            if (!playerDamage.ContainsKey(player.whoAmI))
            {
                playerDamage[player.whoAmI] = 0;
            }
            playerDamage[player.whoAmI] += hit.Damage;
            totalDamageDealt += hit.Damage;
        }
    }

    public override void OnKill(NPC npc)
    {
        base.OnKill(npc);
        if (npc.immortal) return;
        if (npc.friendly || npc.CountsAsACritter || npc.SpawnedFromStatue || npc.isLikeATownNPC || npc.type == NPCID.TargetDummy)
        {
            playerDamage.Clear();
            totalDamageDealt = 0;
            return;
        }

        var isInstantKill = totalDamageDealt >= npc.lifeMax;
        if (isInstantKill || playerDamage.Count != 0 && npc.type != NPCID.TargetDummy)
        {
            var totalDamage = Math.Max(totalDamageDealt, npc.lifeMax);

            // Calculate base XP using normalized values
            int baseXP = CalculateBaseXP(npc);

            foreach (var entry in playerDamage)
            {
                var playerID = entry.Key;
                var damageDealt = entry.Value;
                var player = Main.player[playerID];
                var modPlayer = player.GetModPlayer<ExperiencePlayer>();

                if (player.active && !player.dead && modPlayer.expLevel <= 60)
                {
                    var damageRatio = (float)damageDealt / totalDamage;
                    var experiencePoints = (int)(baseXP * damageRatio);

                    // Apply level scaling to prevent over-leveling on weak enemies
                    experiencePoints = ApplyLevelScaling(experiencePoints, modPlayer.expLevel, npc);

                    if (isInstantKill && damageDealt == playerDamage.Values.Max())
                        experiencePoints = baseXP;

                    ExperiencePlayer.AddExperience(player, experiencePoints);

                    AdvancedPopupRequest text = new()
                    {
                        Color = Color.LightGoldenrodYellow,
                        Text = $"+{experiencePoints} Exp",
                        DurationInFrames = 60
                    };
                    PopupText.NewText(text, player.position);

                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        var packet = Mod.GetPacket();
                        packet.Write((byte)MessageType.AddExperience);
                        packet.Write(playerID);
                        packet.Write(experiencePoints);
                        packet.Send();
                    }
                }
            }
        }
        playerDamage.Clear();
        totalDamageDealt = 0;
    }

    private int CalculateBaseXP(NPC npc)
    {
        int normalizedHealth = GetNormalizedHealth(npc);

        int baseXP = normalizedHealth / 6;

        float tierMultiplier = GetEnemyTierMultiplier(npc);
        baseXP = (int)(baseXP * tierMultiplier);

        float difficultyMultiplier = Main.masterMode ? 1.4f : Main.expertMode ? 1.2f : 1f;
        baseXP = (int)(baseXP * difficultyMultiplier);

        if (npc.boss)
        {
            baseXP *= GetBossMultiplier(npc);
        }

        return Math.Max(baseXP, 1);
    }

    private int GetNormalizedHealth(NPC npc)
    {
        if (Main.masterMode)
            return (int)(npc.lifeMax / 3f);
        else if (Main.expertMode)
            return (int)(npc.lifeMax / 2f);
        else
            return npc.lifeMax;
    }

    private float GetEnemyTierMultiplier(NPC npc)
    {
        // Pre-Hardmode enemies
        if (!Main.hardMode)
        {
            // Early game (surface/underground)
            if (npc.aiStyle == NPCAIStyleID.Slime ||
                npc.type == NPCID.Zombie || npc.type == NPCID.DemonEye)
                return 1.05f;

            if (npc.type == NPCID.ManEater || npc.type == NPCID.EaterofSouls)
                return 1.2f;

            return 1f;
        }
        else
        {
            if (npc.type == NPCID.Wraith || npc.type == NPCID.Pixie)
                return 2f;

            if (npc.type == NPCID.IcyMerman || npc.type == NPCID.PigronCorruption)
                return 2.5f;

            if (npc.type == NPCID.Lihzahrd || npc.type == NPCID.CultistDragonHead)
                return 3f;

            return 2.2f;
        }
    }

    private int GetBossMultiplier(NPC npc)
    {
        if (npc.type == NPCID.KingSlime) return 3;
        if (npc.type == NPCID.EyeofCthulhu) return 4;
        if (npc.type == NPCID.EaterofWorldsHead || npc.type == NPCID.BrainofCthulhu) return 5;
        if (npc.type == NPCID.QueenBee) return 5;
        if (npc.type == NPCID.SkeletronHead) return 6;
        if (npc.type == NPCID.WallofFlesh) return 8;

        if (npc.type == NPCID.QueenSlimeBoss) return 8;
        if (npc.type == NPCID.Spazmatism || npc.type == NPCID.Retinazer) return 10;
        if (npc.type == NPCID.TheDestroyer) return 10;
        if (npc.type == NPCID.SkeletronPrime) return 10;
        if (npc.type == NPCID.Plantera) return 15;
        if (npc.type == NPCID.Golem) return 18;
        if (npc.type == NPCID.DukeFishron) return 20;
        if (npc.type == NPCID.CultistBoss) return 22;
        if (npc.type == NPCID.MoonLordCore) return 30;

        return 7;
    }

    private int ApplyLevelScaling(int baseXP, int playerLevel, NPC npc)
    {
        int enemyLevel = EstimateEnemyLevel(npc);

        int levelDifference = playerLevel - enemyLevel;

        if (levelDifference > 10)
        {
            float penalty = Math.Max(0.1f, 1f - (levelDifference - 10) * 0.05f);
            baseXP = (int)(baseXP * penalty);
        }
        else if (levelDifference < -5)
        {
            float bonus = 1f + Math.Abs(levelDifference + 5) * 0.1f;
            baseXP = (int)(baseXP * Math.Min(bonus, 2f));
        }

        return baseXP;
    }

    private int EstimateEnemyLevel(NPC npc)
    {
        if (!Main.hardMode)
        {
            if (npc.boss) return Math.Min(20, npc.type switch
            {
                NPCID.KingSlime => 5,
                NPCID.EyeofCthulhu => 8,
                NPCID.EaterofWorldsHead or NPCID.BrainofCthulhu => 12,
                NPCID.QueenBee => 14,
                NPCID.SkeletronHead => 16,
                NPCID.WallofFlesh => 20,
                _ => 10
            });

            return 1 + (int)(npc.lifeMax / 50f);
        }
        else
        {
            if (npc.boss) return npc.type switch
            {
                NPCID.QueenSlimeBoss => 25,
                NPCID.Spazmatism or NPCID.Retinazer or NPCID.TheDestroyer or NPCID.SkeletronPrime => 30,
                NPCID.Plantera => 40,
                NPCID.Golem => 45,
                NPCID.DukeFishron => 50,
                NPCID.CultistBoss => 55,
                NPCID.MoonLordCore => 60,
                _ => 35
            };

            return 20 + (int)(npc.lifeMax / 200f); // Hardmode enemy estimate
        }
    }
}