using System.Diagnostics.CodeAnalysis;

namespace Reverie.Core.NPCs.Components;

/// <summary>
///     Provides <see cref="NPC"/> extension methods regarding <see cref="NPCComponent"/>.
/// </summary>
/// <remarks>
///     Kept outside of the <c>Utilities.Extensions</c> namespace for convenience.
/// </remarks>
public static class NPCComponentExtensions
{
    public static bool TryEnable<T>(this NPC npc, [NotNullWhen(true)] out T component) where T : NPCComponent
    {
        if (!npc.TryGetGlobalNPC(out component))
        {
            return false;
        }
        
        component!.Enabled = true;

        return true;
    }
}