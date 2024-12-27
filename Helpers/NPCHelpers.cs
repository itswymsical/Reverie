using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Reverie.Core.Mechanics;
using System.Collections.Generic;

namespace Reverie.Helpers
{
	internal static partial class Helper
	{
		public static void Kill(this NPC npc, bool hitEffect = true)
		{
			if (npc.ModNPC?.CheckDead() == false)
			{
				return;
			}

			npc.life = 0;

			if (hitEffect)
			{
				npc.HitEffect();
			}

			npc.checkDead();

			npc.active = false;
		}
		public static void GenericFighterAI(this NPC npc, float maxSpeed = 1.5f, int maxAllowedStuckTime = 60, float jumpHeightModifier = 1f)
		{
			bool isStuck = npc.ai[1] >= maxAllowedStuckTime;

			bool canJump = false;

			if (npc.velocity.X == 0f && !npc.justHit)
				canJump = true;

			bool lookingAtTarget = true;

			if (npc.velocity.Y == 0f && Math.Sign(npc.velocity.X) != npc.direction)
				lookingAtTarget = false;

			if (npc.position.X == npc.oldPosition.X || isStuck || !lookingAtTarget)
				npc.ai[1]++;
			else if (Math.Abs(npc.velocity.X) > 0.9f && npc.ai[1] > 0f)
				npc.ai[1]--;

			if (npc.ai[1] > maxAllowedStuckTime * 5 || npc.justHit)
				npc.ai[1] = 0f;

			if (isStuck)
			{
				if (npc.ai[1] == maxAllowedStuckTime)
					npc.netUpdate = true;

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
					npc.direction = 1;
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
					npc.velocity.X += 0.07f;

				if (npc.velocity.X > -maxSpeed && npc.direction == -1)
					npc.velocity.X -= 0.07f;

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
					if (!WorldGen.InWorld(tileX, tileY))
						return;

					Tile tile = Framing.GetTileSafely(tileX, tileY);

					if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType])
					{
						collisionBottom = true;

						break;
					}
				}
			}

			if (npc.velocity.Y >= 0f)
				SlopedCollision(npc);

			if (collisionBottom)
				ApplyJump(npc, canJump, jumpHeightModifier);

			Player player = Main.player[npc.target];
			bool onplatform = true;
			for (int i = (int)npc.position.X; i < npc.position.X + npc.width; i += npc.height / 2)
			{
				Tile tile = Framing.GetTileSafely(new Point((int)npc.position.X / 16, (int)(npc.position.Y + npc.height + 8) / 16));
				if (!TileID.Sets.Platforms[tile.TileType])
					onplatform = false;
			}
			if (onplatform && (npc.Center.Y < player.position.Y - 20))
				npc.noTileCollide = true;
			else
				npc.noTileCollide = false;
        }

		public static void GenericFlyerAI(this NPC NPC, float maxSpeedX = 3f, float maxSpeedY = 1f, float accelerationX = 0.05f, float accelerationY = 0.01f, float targetDistance = 100f, float wetModifier = 0.95f, float wetGravity = 0.5f, float wetMaxFallSpeed = 4f)
        {
            NPC.noGravity = true;
            if (NPC.collideX)
            {
                if (NPC.oldVelocity.X > 0f)
                    NPC.direction = -1;
                else
                    NPC.direction = 1;
                NPC.velocity.X = NPC.direction;
            }
            if (NPC.collideY)
            {
                if (NPC.oldVelocity.Y > 0f)
                    NPC.directionY = -1;
                else
                    NPC.directionY = 1;
                NPC.velocity.Y = NPC.directionY;
            }
            int num694 = NPC.target;
            int num695 = NPC.direction;
            if (NPC.target == 255 || Main.player[NPC.target].dead || Collision.CanHit(NPC.Center, 1, 1, Main.player[NPC.target].Center, 1, 1))
            {
                NPC.ai[0] = 90f;
                NPC.TargetClosest();
            }
            else if (NPC.ai[0] > 0f)
            {
                NPC.ai[0] -= 1f;
                NPC.TargetClosest();
            }
            if (NPC.netUpdate && num694 == NPC.target && num695 == NPC.direction)
                NPC.netUpdate = false;
            float num701 = targetDistance;
            float num702 = Math.Abs(NPC.position.X + (float)(NPC.width / 2) - (Main.player[NPC.target].position.X + (float)(Main.player[NPC.target].width / 2)));
            float num703 = Main.player[NPC.target].position.Y - (float)(NPC.height / 2);
            if (NPC.ai[0] <= 0f)
            {
                maxSpeedX *= 0.8f;
                accelerationX *= 0.7f;
                num703 = NPC.Center.Y + (float)(NPC.directionY * 1000);
                if (NPC.velocity.X < 0f)
                    NPC.direction = -1;
                else if (NPC.velocity.X > 0f || NPC.direction == 0)
                    NPC.direction = 1;
            }
            if (num702 > 30f)
            {
                if (NPC.direction == -1 && NPC.velocity.X > 0f - maxSpeedX)
                {
                    NPC.velocity.X -= accelerationX;
                    if (NPC.velocity.X > maxSpeedX)
                        NPC.velocity.X -= accelerationX;
                    else if (NPC.velocity.X > 0f)
                        NPC.velocity.X -= accelerationX / 2f;
                    if (NPC.velocity.X < 0f - maxSpeedX)
                        NPC.velocity.X = 0f - maxSpeedX;
                }
                else if (NPC.direction == 1 && NPC.velocity.X < maxSpeedX)
                {
                    NPC.velocity.X += accelerationX;
                    if (NPC.velocity.X < 0f - maxSpeedX)
                        NPC.velocity.X += accelerationX;
                    else if (NPC.velocity.X < 0f)
                        NPC.velocity.X += accelerationX / 2f;
                    if (NPC.velocity.X > maxSpeedX)
                        NPC.velocity.X = maxSpeedX;
                }
            }
            if (num702 > num701)
                num703 -= num701 / 2f;
            if (NPC.position.Y < num703)
            {
                NPC.velocity.Y += accelerationY;
                if (NPC.velocity.Y < 0f)
                    NPC.velocity.Y += accelerationY;
            }
            else
            {
                NPC.velocity.Y -= accelerationY;
                if (NPC.velocity.Y > 0f)
                    NPC.velocity.Y -= accelerationY;
            }
            if (NPC.velocity.Y < 0f - maxSpeedY)
                NPC.velocity.Y = 0f - maxSpeedY;
            if (NPC.velocity.Y > maxSpeedY)
                NPC.velocity.Y = maxSpeedY;
            if (NPC.wet)
            {
                if (NPC.velocity.Y > 0f)
                    NPC.velocity.Y *= wetModifier;
                NPC.velocity.Y -= wetGravity;
                if (NPC.velocity.Y < -wetMaxFallSpeed)
                    NPC.velocity.Y = -wetMaxFallSpeed;
            }
        }

		public static void GenericCritterAI(this NPC npc, float maxSpeed = 1f, int idleTime = 300)
		{
			bool shouldWalk = npc.ai[0] > idleTime;

			if (npc.direction == 0)
			{
				npc.direction = Main.rand.NextBool() ? -1 : 1;
			}

			npc.ai[0]++;

			if (npc.ai[0] > idleTime * 2)
			{
				if (Main.rand.NextBool(4))
				{
					npc.direction = Main.rand.NextBool() ? -1 : 1;
				}

				npc.ai[0] = 0;
			}

			if (shouldWalk)
			{
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
			}
			else
			{
				npc.velocity.X *= 0.8f;
			}

			if (npc.velocity.Y >= 0f)
			{
				SlopedCollision(npc);
			}
		}
		private static void SlopedCollision(NPC npc)
		{
			Vector2 targetPosition = npc.position + new Vector2(npc.velocity.X, 0);

			int velocityDirection = Math.Sign(npc.velocity.X);

			int tileX = (int)((targetPosition.X + (npc.width / 2) + ((npc.width / 2 + 1) * velocityDirection)) / 16f);
			int tileY = (int)((targetPosition.Y + npc.height - 1f) / 16f);

			Tile tile1 = Framing.GetTileSafely(tileX, tileY);
			Tile tile2 = Framing.GetTileSafely(tileX, tileY - 1);
			Tile tile3 = Framing.GetTileSafely(tileX, tileY - 2);
			Tile tile4 = Framing.GetTileSafely(tileX, tileY - 3);
			Tile tile5 = Framing.GetTileSafely(tileX, tileY - 4);
			Tile tile6 = Framing.GetTileSafely(tileX - velocityDirection, tileY - 3);

			if (tileX * 16f < targetPosition.X + npc.width && tileX * 16f + 16f > targetPosition.X &&
				((tile1.HasUnactuatedTile && !tile1.TopSlope && !tile2.TopSlope && Main.tileSolid[tile1.TileType] && !Main.tileSolidTop[tile1.TileType]) ||
				(tile2.IsHalfBlock && tile2.HasUnactuatedTile)) && (!tile2.HasUnactuatedTile || !Main.tileSolid[tile2.TileType] || Main.tileSolidTop[tile2.TileType] ||
				(tile2.IsHalfBlock &&
				(!tile5.HasUnactuatedTile || !Main.tileSolid[tile5.TileType] || Main.tileSolidTop[tile5.TileType]))) &&
				(!tile3.HasUnactuatedTile || !Main.tileSolid[tile3.TileType] || Main.tileSolidTop[tile3.TileType]) &&
				(!tile4.HasUnactuatedTile || !Main.tileSolid[tile4.TileType] || Main.tileSolidTop[tile4.TileType]) &&
				(!tile6.HasUnactuatedTile || !Main.tileSolid[tile6.TileType]))
			{
				float tileYPosition = tileY * 16f;

				if (Framing.GetTileSafely(tileX, tileY).IsHalfBlock)
					tileYPosition += 8f;

				if (Framing.GetTileSafely(tileX, tileY - 1).IsHalfBlock)
					tileYPosition -= 8f;

				if (tileYPosition < targetPosition.Y + npc.height)
				{
					float targetYPosition = targetPosition.Y + npc.height - tileYPosition;

					if (targetYPosition <= 16.1f)
					{
						npc.gfxOffY += npc.position.Y + npc.height - tileYPosition;

						npc.position.Y = tileYPosition - npc.height;

						npc.stepSpeed = targetYPosition < 9f ? 1f : 2f;
					}
				}
			}
		}
		private static void ApplyJump(NPC npc, bool canJump, float jumpHeightModifier)
		{
			int tileX = (int)((npc.Center.X + 15 * npc.direction) / 16f);
			int tileY = (int)((npc.position.Y + npc.height - 15f) / 16f);

			Tile tile1 = Framing.GetTileSafely(tileX, tileY);
			Tile tile2 = Framing.GetTileSafely(tileX, tileY - 1);
			Tile tile3 = Framing.GetTileSafely(tileX, tileY + 1);
			Tile tile4 = Framing.GetTileSafely(tileX + npc.direction, tileY + 1);

            _ = tile3.IsHalfBlock; //dunno how to fix yet, may be a root cause for errors.

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
					npc.velocity.Y = -5f;

				if (npc.velocity.Y < 0f)
					npc.velocity.Y *= jumpHeightModifier;
			}
		}
		public static bool HoleBelow(this NPC npc)
		{
			int tileWidth = (int)Math.Round(npc.width / 16f);

			int tileX = (int)(npc.Center.X / 16f) - tileWidth;

			if (npc.velocity.X > 0f)
				tileX += tileWidth;

			int tileY = (int)((npc.position.Y + npc.height) / 16f);

			for (int j = tileY; j < tileY + 2; j++)
			{
				for (int i = tileX; i < tileX + tileWidth; i++)
				{
					if (!WorldGen.InWorld(i, j))
						continue;

					Tile tile = Framing.GetTileSafely(i, j);

					if (tile.HasTile)
						return false;
				}
			}

			return true;
		}
        public static bool DrawNPCCenteredWithTexture(this NPC npc, Texture2D texture, SpriteBatch spriteBatch, Color color)
		{
			Vector2 origin = npc.frame.Size() / 2f + new Vector2(0f, npc.ModNPC.DrawOffsetY);

			SpriteEffects effects = npc.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			Vector2 drawPosition = npc.Center.ToDrawPosition() + new Vector2(0f, npc.gfxOffY);

			spriteBatch.Draw(texture, drawPosition, npc.frame, color, npc.rotation, origin, npc.scale, effects, 0f);

			return false;
		}
		public static bool DrawNPCCentered(this NPC npc, SpriteBatch spriteBatch, Color color)
		{
			Texture2D texture = TextureAssets.Npc[npc.type].Value;

			return npc.DrawNPCCenteredWithTexture(texture, spriteBatch, npc.GetAlpha(color));
		}
		public static void DrawNPCTrailCenteredWithTexture(this NPC npc, Texture2D texture, SpriteBatch spriteBatch, Color color, float initialOpacity = 0.8f, float opacityDegrade = 0.2f, int stepSize = 1)
		{
			Vector2 origin = npc.frame.Size() / 2f;

			SpriteEffects effects = npc.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			for (int i = 0; i < NPCID.Sets.TrailCacheLength[npc.type]; i += stepSize)
			{
				float opacity = initialOpacity - opacityDegrade * i;

				Vector2 position = npc.oldPos[i].ToDrawPosition() + npc.Hitbox.Size() / 2f + new Vector2(0f, npc.gfxOffY);

				spriteBatch.Draw(texture, position, npc.frame, color * opacity, npc.oldRot[i], origin, npc.scale, effects, 0f);
			}
		}
		public static void DrawNPCTrailCentered(this NPC npc, SpriteBatch spriteBatch, Color color, float initialOpacity = 0.8f, float opacityDegrade = 0.2f, int stepSize = 1)
		{
			Texture2D texture = TextureAssets.Npc[npc.type].Value;

			npc.DrawNPCTrailCenteredWithTexture(texture, spriteBatch, npc.GetAlpha(color), initialOpacity, opacityDegrade, stepSize);
		}
	}
}
