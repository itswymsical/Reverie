using System.Diagnostics.CodeAnalysis;

namespace Reverie.Core.Items.Components;

/// <summary>
///     Provides <see cref="Item"/> extension methods regarding <see cref="ItemComponent"/>.
/// </summary>
/// <remarks>
///     Kept outside of the <c>Utilities.Extensions</c> namespace for convenience.
/// </remarks>
public static class ItemComponentExtensions
{
    public static bool TryEnable<T>(this Item npc, [NotNullWhen(true)] out T component) where T : ItemComponent
    {
        if (!npc.TryGetGlobalItem(out component))
        {
            return false;
        }
        
        component!.Enabled = true;

        return true;
    }
}