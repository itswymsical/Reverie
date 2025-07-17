namespace Reverie.Core.Graphics.Interfaces
{
    interface IOrderedLoadable
    {
        void Load();
        void Unload();
        float Priority { get; }
    }
}
