using Reverie.Core.Interfaces;

namespace Reverie.Core.PrimitiveDrawing
{
    public class HookGroup : IOrderedLoadable
    {
        public virtual float Priority => 1f;

        public virtual void Load() { }

        public virtual void Unload() { }
    }
}
