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
            int baseXP = CalculateBaseXP(npc);

            foreach (var entry in playerDamage)
            {
                var playerID = entry.Key;
                var damageDealt = entry.Value;
                var player = Main.player[playerID];
                var modPlayer = player.GetModPlayer<ExperiencePlayer>();

                if (player.active && !player.dead)
                {
                    var damageRatio = (float)damageDealt / totalDamage;
                    var experiencePoints = (int)(baseXP * damageRatio);

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
        int baseXP = normalizedHealth / 8; // Slightly reduced since XP is now a consumable resource

        float tierMultiplier = GetEnemyTierMultiplier(npc);
        baseXP = (int)(baseXP * tierMultiplier);

        float difficultyMultiplier = Main.masterMode ? 1.3f : Main.expertMode ? 1.15f : 1f;
        baseXP = (int)(baseXP * difficultyMultiplier);

        if (npc.boss)
        {
            baseXP *= GetBossMultiplier(npc);
        }

        return Math.Max(baseXP, 2); // Minimum 2 XP to make all kills meaningful
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
        if (!Main.hardMode)
        {
            if (npc.aiStyle == NPCAIStyleID.Slime ||
                npc.type == NPCID.Zombie || npc.type == NPCID.DemonEye)
                return 1f;

            if (npc.type == NPCID.ManEater || npc.type == NPCID.EaterofSouls)
                return 1.2f;

            return 1.1f;
        }
        else
        {
            if (npc.type == NPCID.Wraith || npc.type == NPCID.Pixie)
                return 1.8f;

            if (npc.type == NPCID.IcyMerman || npc.type == NPCID.PigronCorruption)
                return 2.2f;

            if (npc.type == NPCID.Lihzahrd || npc.type == NPCID.CultistDragonHead)
                return 2.8f;

            return 2f;
        }
    }

    private int GetBossMultiplier(NPC npc)
    {
        // Simplified boss multipliers since XP is now a resource
        if (npc.type == NPCID.KingSlime) return 8;
        if (npc.type == NPCID.EyeofCthulhu) return 12;
        if (npc.type == NPCID.EaterofWorldsHead || npc.type == NPCID.BrainofCthulhu) return 15;
        if (npc.type == NPCID.QueenBee) return 15;
        if (npc.type == NPCID.SkeletronHead) return 18;
        if (npc.type == NPCID.WallofFlesh) return 25;

        if (npc.type == NPCID.QueenSlimeBoss) return 25;
        if (npc.type == NPCID.Spazmatism || npc.type == NPCID.Retinazer) return 30;
        if (npc.type == NPCID.TheDestroyer) return 30;
        if (npc.type == NPCID.SkeletronPrime) return 30;
        if (npc.type == NPCID.Plantera) return 40;
        if (npc.type == NPCID.Golem) return 45;
        if (npc.type == NPCID.DukeFishron) return 50;
        if (npc.type == NPCID.CultistBoss) return 55;
        if (npc.type == NPCID.MoonLordCore) return 80;

        return 20;
    }
}