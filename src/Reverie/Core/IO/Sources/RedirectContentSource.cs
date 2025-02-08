// Code written and provided by @steviegt6 at GitHub:
// https://github.com/Path-of-Terraria/PathOfTerraria/blob/main/Core/Sources/SmartContentSource.cs

using System.Collections.Generic;
using System.IO;
using System.Linq;
using ReLogic.Content;
using ReLogic.Content.Sources;

namespace Reverie.Core.IO.Sources;

//todo: fix for compiling outside of the debugger.
public sealed class RedirectContentSource(IContentSource source) : IContentSource
{
    private readonly IContentSource _source = source;
    private readonly Dictionary<string, string> _redirects = [];

    public IContentValidator ContentValidator
    {
        get => _source.ContentValidator;
        set => _source.ContentValidator = value;
    }

    public RejectedAssetCollection Rejections => _source.Rejections;

    public IEnumerable<string> EnumerateAssets()
    {
        return _source.EnumerateAssets().Select(RewritePath);
    }

    public string GetExtension(string assetName)
    {
        return _source.GetExtension(RewritePath(assetName));
    }

    public Stream OpenStream(string fullAssetName)
    {
        return _source.OpenStream(RewritePath(fullAssetName));
    }

    public void AddRedirect(string from, string to)
    {
        _redirects[from] = to;  // Use indexer to allow overwriting
    }

    private string RewritePath(string path)
    {
        // Normalize path separators
        path = path.Replace('\\', '/');

        foreach (var (from, to) in _redirects)
        {
            if (path.StartsWith(from, StringComparison.OrdinalIgnoreCase))
            {
                // Use Path.Combine for proper path joining
                var relativePath = path[from.Length..];
                if (relativePath.StartsWith('/')) relativePath = relativePath[1..];
                return to + '/' + relativePath;
            }
        }

        return path;
    }
}