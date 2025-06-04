namespace Reverie.Common.Players;

public class SatchelPlayer : ModPlayer
{
    public bool flowerSatchelVisible;
    public Item activeSatchel;
    public override void Initialize()
    {
        flowerSatchelVisible = false;
        activeSatchel = null;
    }
    public override void ResetEffects()
    {
        // Reset visibility when needed (e.g., on death or game reload)
        if (activeSatchel == null || activeSatchel.IsAir)
        {
            flowerSatchelVisible = false;
            activeSatchel = null;
        }
    }
}
