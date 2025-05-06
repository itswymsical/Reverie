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

            foreach (var entry in playerDamage)
            {
                var playerID = entry.Key;
                var damageDealt = entry.Value;
                var player = Main.player[playerID];

                var modPlayer = player.GetModPlayer<ExperiencePlayer>();

                if (player.active && !player.dead && modPlayer.expLevel <= 60)
                {
                    var damageRatio = (float)damageDealt / totalDamage;
                    var experiencePoints = (int)(npc.lifeMax * damageRatio / 8);

                    // If it's an instant kill, award full XP to the player who dealt the most damage
                    if (isInstantKill && damageDealt == playerDamage.Values.Max()) experiencePoints = npc.lifeMax / 8;
                    

                    ExperiencePlayer.AddExperience(player, experiencePoints);

                    AdvancedPopupRequest text = new()
                    {
                        Color = Color.LightGoldenrodYellow * experiencePoints,
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
}