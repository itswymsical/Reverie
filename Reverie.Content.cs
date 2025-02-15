using ReLogic.Content.Sources;
using Reverie.Core.IO.Sources;

namespace Reverie;

public sealed partial class Reverie : Mod
{
    public override IContentSource CreateDefaultContentSource()
    {
        var source = new RedirectContentSource(base.CreateDefaultContentSource());

        source.AddRedirect("Content", "Assets/Textures");

        return source;
    }
}