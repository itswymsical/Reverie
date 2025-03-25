
namespace Reverie.Core.Missions;

public partial class MissionManager
{
    private int housingCheckCounter = 0;
    public void OnUpdate()
    {
        housingCheckCounter++;
        if (housingCheckCounter >= 240)
        {
            CheckForValidHousing();
            housingCheckCounter = 0;
        }
    }

    public void CheckForValidHousing()
    {

        try
        {
            Player player = Main.LocalPlayer;
            int checkRadius = 60;
            int sampleDistance = 10;

            int playerX = (int)(player.position.X / 16);
            int playerY = (int)(player.position.Y / 16);

            bool validHousingFound = false;

            for (int xOffset = -checkRadius; xOffset <= checkRadius; xOffset += sampleDistance)
            {
                for (int yOffset = -checkRadius; yOffset <= checkRadius; yOffset += sampleDistance)
                {
                    int x = playerX + xOffset;
                    int y = playerY + yOffset;

                    if (!WorldGen.InWorld(x, y))
                        continue;

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
                OnValidHousingFound();
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error in housing check: {ex.Message}");
        }
    }
    

    private bool ValidateHousingAt(int x, int y)
    {
        try
        {
            bool tempGameMenu = Main.gameMenu;
            Main.gameMenu = false;

            bool isValid = WorldGen.StartRoomCheck(x, y);

            Main.gameMenu = tempGameMenu;

            return isValid;
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Housing validation error: {ex.Message}");
            return false;
        }
    }

    private void OnValidHousingFound()
    {
        foreach (var mission in ActiveMissions)
        {
            mission.OnValidHousingFound();
        }
    }

    public void CheckForNPCsWithHomes()
    {
        try
        {
            bool npcWithHomeFound = false;

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