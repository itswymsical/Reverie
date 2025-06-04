// Modified from Spirit Mod's Backpack slot
// Credits: Spirit Mod, GabeHasWon
// https://github.com/GabeHasWon/SpiritReforged/blob/master/Common/UI/BackpackInterface/BackbackUISlot.cs#L1

using System.Collections.Generic;
using Terraria.UI;

namespace Reverie.Common.UI.FlowerSatchel;

public class FlowerSystem : ModSystem
{
    private UserInterface flowerSatchelInterface;
    private FlowerSatchelUIState flowerSatchelUI;

    public override void Load()
    {
        // Initialize UI
        if (!Main.dedServ)
        {
            flowerSatchelUI = new FlowerSatchelUIState();
            flowerSatchelUI.Activate();
            flowerSatchelInterface = new UserInterface();
            flowerSatchelInterface.SetState(flowerSatchelUI);
        }
    }

    public override void UpdateUI(GameTime gameTime)
    {
        if (flowerSatchelInterface?.CurrentState != null)
        {
            flowerSatchelInterface.Update(gameTime);
        }
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        var inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
        if (inventoryIndex != -1)
        {
            layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
                "Reverie: Flower Satchel",
                delegate
                {
                    if (Main.playerInventory)
                    {
                        flowerSatchelInterface?.Draw(Main.spriteBatch, new GameTime());
                    }
                    return true;
                },
                InterfaceScaleType.UI)
            );
        }
    }
}
