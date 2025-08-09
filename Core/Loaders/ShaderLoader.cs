using ReLogic.Content;
using Reverie.Core.Graphics.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.ModLoader.Core;

namespace Reverie.Core.Loaders;

class ShaderLoader : IOrderedLoadable
{
    private static readonly Dictionary<string, Lazy<Asset<Effect>>> Shaders = [];

    public float Priority => 0.9f;

    public void Load()
    {
        if (Main.dedServ)
            return;

        var info = typeof(Mod).GetProperty("File", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);
        var file = (TmodFile)info.Invoke(Instance, null);

        var shaders = file.Where(n => n.Name.StartsWith("Assets/Effects/") && n.Name.EndsWith(".fxc"));

        foreach (var entry in shaders)
        {
            var name = entry.Name.Replace(".fxc", "").Replace("Assets/Effects/", "");
            var path = entry.Name.Replace(".fxc", "");
            LoadShader(name, path);
        }
    }

    public void Unload()
    {

    }

    public static Asset<Effect> GetShader(string key)
    {
        if (Shaders.ContainsKey(key))
        {
            return Shaders[key].Value;
        }
        else
        {
            LoadShader(key, $"Assets/Effects/{key}");
            return Shaders[key].Value;
        }
    }

    public static void LoadShader(string name, string path)
    {
        if (!Shaders.ContainsKey(name))
            Shaders.Add(name, new(() => Instance.Assets.Request<Effect>(path)));
    }
}