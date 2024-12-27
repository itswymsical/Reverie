using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Linq;
using System;
using Reverie.Common.Players;
using static Reverie.Reverie;

namespace Reverie.Common.Global
{
    public class ExperienceGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public Dictionary<int, int> playerDamage = [];
        public int totalDamageDealt = 0;

        public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
          
            if (!playerDamage.ContainsKey(player.whoAmI))
            {
                playerDamage[player.whoAmI] = 0;
            }
            playerDamage[player.whoAmI] += hit.Damage;
            totalDamageDealt += hit.Damage;
            base.OnHitByItem(npc, player, item, hit, damageDone);
        }

        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (projectile.owner >= 0 && projectile.owner < Main.maxPlayers)
            {
                Player player = Main.player[projectile.owner];
                if (!playerDamage.ContainsKey(player.whoAmI))
                {
                    playerDamage[player.whoAmI] = 0;
                }
                playerDamage[player.whoAmI] += hit.Damage;
                totalDamageDealt += hit.Damage;
            }
            base.OnHitByProjectile(npc, projectile, hit, damageDone);
        }

        public override void OnKill(NPC npc)
        {
            if (npc.friendly || npc.CountsAsACritter || npc.SpawnedFromStatue || npc.isLikeATownNPC)
            {
                playerDamage.Clear();
                totalDamageDealt = 0;
                return;
            }

            bool isInstantKill = totalDamageDealt >= npc.lifeMax;

            if (isInstantKill || playerDamage.Count != 0 && npc.type != NPCID.TargetDummy)
            {
                int totalDamage = Math.Max(totalDamageDealt, npc.lifeMax);

                foreach (var entry in playerDamage)
                {
                    int playerID = entry.Key;
                    int damageDealt = entry.Value;
                    Player player = Main.player[playerID];

                    ExperiencePlayer modPlayer = player.GetModPlayer<ExperiencePlayer>();

                    if (player.active && !player.dead && modPlayer.experienceLevel <= 99)
                    {
                        float damageRatio = (float)damageDealt / totalDamage;
                        int experiencePoints = (int)(npc.lifeMax * damageRatio / 8);

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
                            ModPacket packet = Mod.GetPacket();
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
            base.OnKill(npc);
        }
    }
}