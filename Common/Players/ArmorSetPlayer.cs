using Terraria.ModLoader;

namespace Reverie.Common.Players
{
	public class ArmorSetPlayer : ModPlayer
	{
		public bool vikingSet;

        public override void ResetEffects()
		{
			vikingSet = false;
		}
	}
}
