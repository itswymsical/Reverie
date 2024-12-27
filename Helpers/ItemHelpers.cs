using Terraria;
using Reverie.Common.Global;
using Terraria.ID;

namespace Reverie.Helpers
{
    internal static partial class Helper
    {
        public static bool IsShovel(this Item item) => item.GetGlobalItem<ReverieGlobalItem>().Shovel;
        public static bool IsShiny(this Item item) => (item.Name.Contains("Ore"));
    }
}