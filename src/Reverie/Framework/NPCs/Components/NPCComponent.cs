namespace Reverie.Framework.NPCs.Components;

/// <summary>
///     Provides a base class for defining components that can extend the behavior of <see cref="NPC"/>.
/// </summary>
public abstract class NPCComponent : GlobalNPC
{
    /// <summary>
    ///     Gets or sets whether this component is enabled.
    /// </summary>
    public bool Enabled { get; set; }
    
    /// <summary>
    ///     <inheritdoc cref="GlobalNPC.InstancePerEntity"/>
    /// </summary>
    /// <remarks>
    ///     This property is overridden to return <see langword="true"/>.
    /// </remarks>  
    public sealed override bool InstancePerEntity { get; } = true;
}