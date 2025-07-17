using Reverie.Core.Graphics.Interfaces;

namespace Reverie.Core.Graphics;

public class HookGroup : IOrderedLoadable
{
    public virtual float Priority => 1f;

    public virtual void Load() { }

    public virtual void Unload() { }
}
