using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie;

/// <summary>
///     Represents the <see cref="Mod" /> implementation of this mod.
/// </summary>
public sealed partial class Reverie : Mod
{
    /// <summary>
    ///     The name of this mod.
    /// </summary>
    public const string NAME = nameof(Reverie);
    
    /// <summary>
    ///     The prefix to use for the name of this mod.
    /// </summary>
    public const string NAME_PREFIX = NAME + ": ";
    
    /// <summary>
    ///     Gets the <see cref="Mod" /> implementation of this mod.
    /// </summary>
    public static Reverie Instance => ModContent.GetInstance<Reverie>();
}