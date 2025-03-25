using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reverie.Core.Missions
{
    public partial class MissionManager
    {
        private int housingCheckCounter = 0;

        // Add this to your existing Update method
        public void OnUpdate()
        {
            housingCheckCounter++;
            if (housingCheckCounter >= 240) // Check every 4 seconds (240 frames)
            {
                CheckForValidHousing();
                housingCheckCounter = 0;
            }
        }

        public void CheckForValidHousing()
        {
            lock (managerLock)
            {
                try
                {
                    // Sample locations around the player to check for valid housing
                    Player player = Main.LocalPlayer;
                    int checkRadius = 60;
                    int sampleDistance = 10;

                    // Get player's position in tile coordinates
                    int playerX = (int)(player.position.X / 16);
                    int playerY = (int)(player.position.Y / 16);

                    bool validHousingFound = false;

                    // Check in a grid pattern around the player
                    for (int xOffset = -checkRadius; xOffset <= checkRadius; xOffset += sampleDistance)
                    {
                        for (int yOffset = -checkRadius; yOffset <= checkRadius; yOffset += sampleDistance)
                        {
                            int x = playerX + xOffset;
                            int y = playerY + yOffset;

                            // Make sure coordinates are in valid world range
                            if (!WorldGen.InWorld(x, y))
                                continue;

                            // Check using the standard housing validation system
                            if (ValidateHousingAt(x, y))
                            {
                                validHousingFound = true;
                                break;
                            }
                        }

                        if (validHousingFound)
                            break;
                    }

                    if (validHousingFound)
                    {
                        // Notify missions that valid housing was found
                        OnValidHousingFound();
                    }
                }
                catch (Exception ex)
                {
                    ModContent.GetInstance<Reverie>().Logger.Error($"Error in housing check: {ex.Message}");
                }
            }
        }

        private bool ValidateHousingAt(int x, int y)
        {
            try
            {
                // Temporarily set gameMenu to false
                bool tempGameMenu = Main.gameMenu;
                Main.gameMenu = false;

                // Call Terraria's built-in housing validation method
                bool isValid = WorldGen.StartRoomCheck(x, y);

                // Restore original state
                Main.gameMenu = tempGameMenu;

                return isValid;
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Housing validation error: {ex.Message}");
                return false;
            }
        }

        // Call this method when valid housing is found to notify active missions
        private void OnValidHousingFound()
        {
            foreach (var mission in ActiveMissions)
            {
                mission.OnValidHousingFound();
            }
        }

        // Alternative way to detect housing - check for NPCs with homes
        public void CheckForNPCsWithHomes()
        {
            lock (managerLock)
            {
                try
                {
                    bool npcWithHomeFound = false;

                    // Check if any NPC has a home
                    for (int i = 0; i < Main.npc.Length; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && npc.townNPC && !npc.homeless)
                        {
                            npcWithHomeFound = true;
                            break;
                        }
                    }

                    if (npcWithHomeFound)
                    {
                        OnValidHousingFound();
                    }
                }
                catch (Exception ex)
                {
                    ModContent.GetInstance<Reverie>().Logger.Error($"Error checking for NPCs with homes: {ex.Message}");
                }
            }
        }
    }
}
