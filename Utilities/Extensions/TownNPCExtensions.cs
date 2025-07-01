using Reverie.Core.Dialogue;
using Terraria.ID;

namespace Reverie.Utilities.Extensions;

/// <summary>
///     Provides <see cref="NPC"/> extension methods, specifically for Town NPCs.
/// </summary>
public static class TownNPCExtensions
{
    /// <summary>
    /// Makes a Town <see cref="NPC"/> perform rock, paper, scissors.
    /// </summary>
    /// <param name="npc"></param>
    /// <returns></returns>
    public static bool ForceRockPaperScissors(this NPC npc) => npc.ai[0] == 16f;

    /// <summary>
    /// Attempts to make a Town NPC sit on a chair at the specified location.
    /// </summary>
    /// <param name="npc">The NPC to attempt sitting</param>
    /// <param name="homeFloorX">The X coordinate of the floor tile below the chair</param>
    /// <param name="homeFloorY">The Y coordinate of the floor tile below the chair</param>
    /// <returns>True if the NPC successfully started sitting, false otherwise</returns>
    public static bool TryForceSitting(this NPC npc, int homeFloorX, int homeFloorY)
    {
        Tile tile = Main.tile[homeFloorX, homeFloorY - 1];

        // Check if NPC can sit and isn't already in a special state
        bool canSit = !NPCID.Sets.CannotSitOnFurniture[npc.type] &&
                     !NPCID.Sets.IsTownSlime[npc.type] &&
                     npc.ai[0] != 5f;

        // Verify the tile is a valid chair
        if (canSit)
        {
            canSit &= tile != null &&
                     tile.HasTile &&
                     TileID.Sets.CanBeSatOnForNPCs[tile.TileType];
        }

        // Special check for specific throne frames
        if (canSit && tile.TileType == 15)
        {
            canSit &= tile.TileFrameY < 1080 || tile.TileFrameY > 1098;
        }

        // Check if another NPC is already sitting here
        if (canSit)
        {
            Point seatPoint = (npc.Bottom + Vector2.UnitY * -2f).ToTileCoordinates();
            for (int i = 0; i < 200; i++)
            {
                if (Main.npc[i].active &&
                    Main.npc[i].aiStyle == 7 &&
                    Main.npc[i].townNPC &&
                    Main.npc[i].ai[0] == 5f &&
                    (Main.npc[i].Bottom + Vector2.UnitY * -2f).ToTileCoordinates() == seatPoint)
                {
                    canSit = false;
                    break;
                }
            }
        }

        // If all checks pass, make the NPC sit
        if (canSit)
        {
            npc.ai[0] = 5f;
            npc.ai[1] = 900 + Main.rand.Next(10800);

            // Get sitting position and direction
            npc.SitDown(new Point(homeFloorX, homeFloorY - 1), out int targetDirection, out var bottom);

            npc.direction = targetDirection;
            npc.Bottom = bottom;
            npc.velocity = Vector2.Zero;
            npc.localAI[3] = 0f;
            npc.netUpdate = true;
        }

        return canSit;
    }

    /// <summary>
    /// Attempts to teleport a Town NPC to their home position and optionally make them sit.
    /// </summary>
    /// <param name="npc">The NPC to teleport</param>
    /// <param name="homeFloorX">The X coordinate of the floor tile at their home</param>
    /// <param name="homeFloorY">The Y coordinate of the floor tile at their home</param>
    /// <returns>True if teleport was successful, false if NPC became homeless</returns>
    public static bool TryTeleportToHome(this NPC npc, int homeFloorX, int homeFloorY)
    {
        // Try three different horizontal positions (center, right, left)
        for (int i = 0; i < 3; i++)
        {
            int horizontalOffset = i switch
            {
                0 => 0,  // Try center first
                1 => -1, // Try left
                _ => 1   // Try right
            };

            int targetX = homeFloorX + horizontalOffset;

            // Check if position is valid (Guide (type 37) can teleport anywhere, others need open space)
            if (npc.type == NPCID.OldMan || !Collision.SolidTiles(targetX - 1, targetX + 1, homeFloorY - 3, homeFloorY - 1))
            {
                // Stop NPC movement
                npc.velocity = Vector2.Zero;

                // Position NPC
                npc.position.X = targetX * 16 + 8 - npc.width / 2;
                npc.position.Y = (homeFloorY * 16 - npc.height) - 0.1f;

                npc.netUpdate = true;

                // Try to make them sit if possible
                npc.TryForceSitting(homeFloorX, homeFloorY);

                return true;
            }
        }

        // If no valid position was found, make NPC homeless and find new home
        npc.homeless = true;
        WorldGen.QuickFindHome(npc.whoAmI);

        return false;
    }
}
