namespace Reverie.Framework.Items.Components;

/// <summary>
///     Provides a base class for defining components that can extend the behavior of <see cref="Item"/>.
/// </summary>
public abstract class ItemComponent : GlobalItem
{
    /// <summary>
    ///     Gets or sets whether this component is enabled.
    /// </summary>
    public bool Enabled { get; set; }
    
    /// <summary>
    ///     <inheritdoc cref="GlobalItem.InstancePerEntity"/>
    /// </summary>
    /// <remarks>
    ///     This property is overridden to return <see langword="true"/>.
    /// </remarks>  
    public sealed override bool InstancePerEntity { get; } = true;
}