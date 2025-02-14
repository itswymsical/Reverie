

namespace Reverie.Core.Missions.MissionAttributes;

public static class ObjectiveHelper
{
    public static void RetrieveItemsFromPlayer(Player player, int itemType, int amount)
    {
        var remainingAmount = amount;

        for (var i = 0; i < player.inventory.Length && remainingAmount > 0; i++)
        {
            var item = player.inventory[i];
            if (item.type == itemType)
            {
                var amountToTake = Math.Min(remainingAmount, item.stack);
                remainingAmount -= amountToTake;
                item.stack -= amountToTake;

                if (item.stack <= 0)
                {
                    item.TurnToAir();
                }
            }
        }
    }
}