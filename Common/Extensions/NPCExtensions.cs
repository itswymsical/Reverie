#region Using directives

using System;

using Terraria;
using Terraria.ModLoader;

using Microsoft.Xna.Framework;
using Terraria.GameContent.UI;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;
using Reverie.Core.Dialogue;
using Reverie.Common.Players;
using Reverie.Core.Missions;

#endregion

namespace Reverie.Common.Extensions
{
	internal static class NPCExtensions
	{
		/// <summary>
		/// Applies generic fighter AI to the given ModNPC.
		/// Only use ai[2] and ai[3] for custom AI when using this method.
		/// </summary>
		/// <param name="modNPC"></param>
		/// <param name="maxSpeed"></param>
		/// <param name="maxAllowedStuckTime"></param>
		public static void GenericFighterAI(this ModNPC modNPC, float maxSpeed = 1.5f, int maxAllowedStuckTime = 60, float jumpHeightModifier = 1f)
		{
			NPC npc = modNPC.NPC;

			bool canJump = false;
			bool isStuck = npc.ai[1] >= maxAllowedStuckTime;

			if (npc.velocity.X == 0f && !npc.justHit)
			{
				canJump = true;
			}

			bool lookingAtTarget = true;

			if (npc.velocity.Y == 0f && Math.Sign(npc.velocity.X) != npc.direction)
			{
				lookingAtTarget = false;
			}

			if (npc.position.X == npc.oldPosition.X || isStuck || !lookingAtTarget)
			{
				npc.ai[1]++;
			}
			else if (Math.Abs(npc.velocity.X) > 0.9f && npc.ai[1] > 0f)
			{
				npc.ai[1]--;
			}

			if (npc.ai[1] > maxAllowedStuckTime * 5 || npc.justHit)
			{
				npc.ai[1] = 0f;
			}

			if (isStuck)
			{
				// First update being stuck.
				if (npc.ai[1] == maxAllowedStuckTime)
				{
					npc.netUpdate = true;
				}

				if (npc.velocity.X == 0f)
				{
					if (npc.velocity.Y == 0f)
					{
						if (++npc.ai[0] >= 2f)
						{
							npc.ai[0] = 0f;
							npc.direction *= -1;
							npc.spriteDirection = npc.direction;
						}
					}
				}
				else
				{
					npc.ai[0] = 0f;
				}
				if (npc.direction == 0)
				{
					npc.direction = 1;
				}
			}
			else if (npc.ai[1] < maxAllowedStuckTime)
			{
				npc.TargetClosest();
			}

			if (npc.velocity.X < -maxSpeed || npc.velocity.X > maxSpeed)
			{
				if (npc.velocity.Y == 0f)
				{
					npc.velocity *= 0.8f;
				}
			}
			else
			{
				if (npc.velocity.X < maxSpeed && npc.direction == 1)
				{
					npc.velocity.X += 0.07f;
				}
				if (npc.velocity.X > -maxSpeed && npc.direction == -1)
				{
					npc.velocity.X -= 0.07f;
				}
				npc.velocity.X = MathHelper.Clamp(npc.velocity.X, -maxSpeed, maxSpeed);
			}

			bool collisionBottom = false;
			if (npc.velocity.Y == 0f)
			{
				int tileY = (int)(npc.position.Y + npc.height + 7f) / 16;
				int minTileX = (int)(npc.position.X / 16);
				int maxTileX = (int)(npc.position.X + npc.width) / 16;

				for (int tileX = minTileX; tileX <= maxTileX; tileX++)
				{
					if (Main.tile[tileX, tileY] == null)
					{
						return;
					}
					if (Main.tile[tileX, tileY].HasUnactuatedTile && Main.tileSolid[Main.tile[tileX, tileY].TileType])
					{
						collisionBottom = true;
						break;
					}
				}
			}

			if (npc.velocity.Y >= 0f)
			{
				SlopedCollision(npc);
			}

			if (collisionBottom)
			{
				ApplyJump(npc, canJump, jumpHeightModifier);
			}
		}

		private static void SlopedCollision(NPC npc)
		{
			int velocityDirection = Math.Sign(npc.velocity.X);
			Vector2 targetPosition = npc.position + new Vector2(npc.velocity.X, 0);

			int tileX = (int)((targetPosition.X + (npc.width / 2) + ((npc.width / 2 + 1) * velocityDirection)) / 16f);
			int tileY = (int)((targetPosition.Y + npc.height - 1f) / 16f);

			Tile tile1 = Framing.GetTileSafely(tileX, tileY);
			Tile tile2 = Framing.GetTileSafely(tileX, tileY - 1);
			Tile tile3 = Framing.GetTileSafely(tileX, tileY - 2);
			Tile tile4 = Framing.GetTileSafely(tileX, tileY - 3);
			Tile tile5 = Framing.GetTileSafely(tileX, tileY - 4);
			Tile tile6 = Framing.GetTileSafely(tileX - velocityDirection, tileY - 3);

			if (tileX * 16 < targetPosition.X + npc.width && tileX * 16 + 16 > targetPosition.X &&
				((tile1.HasUnactuatedTile && !tile1.TopSlope && !tile2.TopSlope && Main.tileSolid[tile1.TileType] && !Main.tileSolidTop[tile1.TileType]) ||
				(tile2.IsHalfBlock && tile2.HasUnactuatedTile)) && (!tile2.HasUnactuatedTile || !Main.tileSolid[tile2.TileType] || Main.tileSolidTop[tile2.TileType] ||
				(tile2.IsHalfBlock &&
				(!tile5.HasUnactuatedTile || !Main.tileSolid[tile5.TileType] || Main.tileSolidTop[tile5.TileType]))) &&
				(!tile3.HasUnactuatedTile || !Main.tileSolid[tile3.TileType] || Main.tileSolidTop[tile3.TileType]) &&
				(!tile4.HasUnactuatedTile || !Main.tileSolid[tile4.TileType] || Main.tileSolidTop[tile4.TileType]) &&
				(!tile6.HasUnactuatedTile || !Main.tileSolid[tile6.TileType]))
			{
				float tileYPosition = tileY * 16;
				if (Main.tile[tileX, tileY].IsHalfBlock)
				{
					tileYPosition += 8f;
				}
				if (Main.tile[tileX, tileY - 1].IsHalfBlock)
				{
					tileYPosition -= 8f;
				}

				if (tileYPosition < targetPosition.Y + npc.height)
				{
					float targetYPosition = targetPosition.Y + npc.height - tileYPosition;
					if (targetYPosition <= 16.1f)
					{
						npc.gfxOffY += npc.position.Y + npc.height - tileYPosition;
						npc.position.Y = tileYPosition - npc.height;

						if (targetYPosition < 9f)
						{
							npc.stepSpeed = 1f;
						}
						else
						{
							npc.stepSpeed = 2f;
						}
					}
				}
			}
		}

		/// <summary>
		/// Try to apply a 'jumping' velocity to the current NPC, based on the current state and position of said NPC.
		/// </summary>
		/// <param name="canJump">Pre-defined variable to see if the NPC can attempt a jump.</param>
		private static void ApplyJump(NPC npc, bool canJump, float jumpHeightModifier)
		{
			int tileX = (int)((npc.Center.X + 15 * npc.direction) / 16f);
			int tileY = (int)((npc.position.Y + npc.height - 15f) / 16f);

			Tile tile1 = Framing.GetTileSafely(tileX, tileY);
			Tile tile2 = Framing.GetTileSafely(tileX, tileY - 1);
			Tile tile3 = Framing.GetTileSafely(tileX, tileY + 1);
			Tile tile4 = Framing.GetTileSafely(tileX + npc.direction, tileY + 1);

            tile3.IsHalfBlock = true;

            if (npc.spriteDirection == Math.Sign(npc.velocity.X))
			{
				if (tile2.HasUnactuatedTile && Main.tileSolid[tile2.TileType])
				{
					npc.netUpdate = true;
					npc.velocity.Y = -6f;
				}
				else if (npc.position.Y + npc.height - (tileY * 16) > 20f && tile1.HasUnactuatedTile && !tile1.TopSlope && Main.tileSolid[tile1.TileType])
				{
					npc.netUpdate = true;
					npc.velocity.Y = -5f;
				}
				else if (npc.directionY < 0 &&
					(!tile3.HasUnactuatedTile || !Main.tileSolid[tile3.TileType]) &&
					(!tile4.HasUnactuatedTile || !Main.tileSolid[tile4.TileType]))
				{
					npc.netUpdate = true;
					npc.velocity.Y = -8f;
					npc.velocity.X *= 1.5f;
				}

				if (npc.velocity.Y == 0f && canJump && npc.ai[1] == 1f)
				{
					npc.velocity.Y = -5f;
				}

				if (npc.velocity.Y < 0)
				{
					npc.velocity.Y *= jumpHeightModifier;
				}
			}
		}
    }

	internal static class TownNPCExtensions
	{
        /// <summary>
        /// Makes a town NPC perform rock, paper, scissors.
        /// </summary>
        /// <param name="npc"></param>
        /// <returns></returns>
        public static bool TownNPC_PlayRPS(this NPC npc) => npc.ai[0] == 16f;
        /// <summary>
        /// Makes a town do that talking bubble thingy.
        /// </summary>
        /// <param name="npc"></param>
        /// <returns></returns>
        public static void TownNPC_TalkState(this NPC npc)
        { 
            static bool IsNPCInActiveDialogue(NPC npc)
            {
                var activeDialogue = DialogueManager.Instance.GetActiveDialogue();
                if (activeDialogue != null)
                {
                    return activeDialogue.npcData.NpcID == npc.type;
                }
                return false;
            }

            if (!IsNPCInActiveDialogue(npc))
            {
                npc.immortal = false;
                return;
            }
			else
			{
                npc.ai[0] = 3f; 
				npc.immortal = true;
                npc.velocity = Vector2.Zero;

                Player player = Main.player[Main.myPlayer];
                npc.direction = player.Center.X < npc.Center.X ? -1 : 1;
                npc.spriteDirection = npc.direction;
            }
        }

        public static bool NPCHasAvailableMission(this NPC npc, MissionPlayer missionPlayer, int npcType)
        {
            if (missionPlayer.npcMissionsDict.TryGetValue(npcType, out var missionIds))
            {
                foreach (var missionId in missionIds)
                {
                    var mission = missionPlayer.GetMission(missionId);
                    if (mission != null && mission.State == MissionState.Unlocked && mission.Progress != MissionProgress.Completed)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
