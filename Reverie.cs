using AnimatedModIconLib.Core;
using Reverie.Common.Players;
using Reverie.Core.Graphics.Interfaces;
using Reverie.Core.Loaders;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
    ///     Directory for UI assets.
    /// </summary>
    public const string UI_ASSET_DIRECTORY = nameof(Reverie) + "/Assets/Textures/UI/";
    public const string PLACEHOLDER = nameof(Reverie) + "/Assets/Textures/PLACEHOLDER";
    public const string TEXTURE_DIRECTORY = nameof(Reverie) + "/Assets/Textures/";
    /// <summary>
    ///     Directory invisile texture.
    /// </summary>
    public const string INVIS = nameof(Reverie) + "/Assets/Textures/Invis";
    /// <summary>
    ///     Directory for VFX textures.
    /// </summary>
    public const string VFX_DIRECTORY = nameof(Reverie) + "/Assets/Textures/VFX/";
    /// <summary>
    ///     Directory for sound effects.
    /// </summary>
    public const string SFX_DIRECTORY = nameof(Reverie) + "/Assets/Sounds/";
    /// <summary>
    ///     Directory for music soundtracks.
    /// </summary>
    public const string MUSIC_DIRECTORY = nameof(Reverie) + "/Assets/Music/";
    public const string CUTSCENE_MUSIC_DIRECTORY = MUSIC_DIRECTORY + "Cutscene/";
    public const string CUTSCENE_TEXTURE_DIRECTORY = nameof(Reverie) + "/Assets/Textures/Cutscene/";
    /// <summary>
    ///     Directory for logo textures.
    /// </summary>
    public const string LOGO_DIRECTORY = nameof(Reverie) + "/Assets/Textures/Logo/";
    public const string ICON_DIRECTORY = nameof(Reverie) + "/Assets/Textures/AnimatedIcon/";
    /// <summary>
    ///     The prefix to use for the name of this mod.
    /// </summary>
    public const string NAME_PREFIX = NAME + ": ";

    /// <summary>
    ///     Gets the <see cref="Mod" /> implementation of this mod.
    /// </summary>
    /// 
    public static Reverie Instance { get; set; }

    private List<IOrderedLoadable> loadCache;
    public Reverie() => Instance = this;

    public override void Load()
    {

        loadCache = [];

        foreach (Type type in Code.GetTypes())
        {
            if (!type.IsAbstract && type.GetInterfaces().Contains(typeof(IOrderedLoadable)))
            {
                object instance = Activator.CreateInstance(type);
                loadCache.Add((IOrderedLoadable)instance);
            }

            loadCache.Sort((n, t) => n.Priority.CompareTo(t.Priority));
        }

        for (int k = 0; k < loadCache.Count; k++)
        {
            loadCache[k].Load();

        }
        this.RegisterAnimatedModIcon(DrawModIcon);
    }

    public void DrawModIcon(SpriteBatch spriteBatch, Vector2 size)
    {
        Texture2D background = ModContent.Request<Texture2D>(ICON_DIRECTORY + "SkyGradient").Value;
        Texture2D moon = ModContent.Request<Texture2D>(ICON_DIRECTORY + "Moon").Value;
        Texture2D foreground = ModContent.Request<Texture2D>(ICON_DIRECTORY + "LightTrail").Value;
        Texture2D ocean = ModContent.Request<Texture2D>(ICON_DIRECTORY + "Ocean").Value;
        Texture2D medCloud = ModContent.Request<Texture2D>(ICON_DIRECTORY + "MediumCloud").Value;
        Texture2D bigCloud = ModContent.Request<Texture2D>(ICON_DIRECTORY + "BigCloud").Value;
        Texture2D tinyCloud = ModContent.Request<Texture2D>(ICON_DIRECTORY + "TinyCloud").Value;

        Texture2D stars = ModContent.Request<Texture2D>(ICON_DIRECTORY + "Stars").Value;
        Texture2D frame = ModContent.Request<Texture2D>(ICON_DIRECTORY + "Frame").Value;

        int iconWidth = (int)size.X;
        int iconHeight = (int)size.Y;
        Rectangle iconRect = new Rectangle(0, 0, iconWidth, iconHeight);

        float time = (float)Main.timeForVisualEffects * 0.005f;

        float slowPan = time * 0.35f;
        float mediumPan = time * 0.5f;
        float fastPan = time * 0.55f;

        slowPan = slowPan % 1f;
        mediumPan = mediumPan % 1f;
        fastPan = fastPan % 1f;

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer);

        spriteBatch.Draw(background, iconRect, Color.White);

        Rectangle starSource = new Rectangle(
            (int)(slowPan * stars.Width), 0,
            iconWidth, iconHeight
        );
        spriteBatch.Draw(stars, iconRect, starSource, Color.White * 0.9f);

        spriteBatch.End();

        var galaxyShader = ShaderLoader.GetShader("GalaxyShader").Value;
        if (galaxyShader != null)
        {
            Texture2D whitePixel = ModContent.Request<Texture2D>(VFX_DIRECTORY + "Wyrmscape").Value;

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, galaxyShader);

            galaxyShader.Parameters["uTime"]?.SetValue(time * 0.5f);
            galaxyShader.Parameters["uIntensity"]?.SetValue(0.45f);
            galaxyShader.Parameters["uColor"]?.SetValue(new Vector4(0.2f, 0.45f, 1f, 0.7f));
            galaxyShader.Parameters["uCenter"]?.SetValue(new Vector2(-1f, -0.8f));
            galaxyShader.Parameters["uScale"]?.SetValue(0.2f);
            galaxyShader.Parameters["uRotation"]?.SetValue(time * 0.055f);
            galaxyShader.Parameters["uArmCount"]?.SetValue(1f);

            Main.graphics.GraphicsDevice.Textures[1] = whitePixel;

            spriteBatch.Draw(whitePixel, iconRect, Color.White);
            spriteBatch.End();
        }

        var shineShader = ShaderLoader.GetShader("ShineShader")?.Value;
        if (shineShader != null)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, shineShader);

            shineShader.Parameters["uTime"]?.SetValue(time * 2f);
            shineShader.Parameters["uOpacity"]?.SetValue(2f);
            spriteBatch.Draw(moon, iconRect, Color.White);
            spriteBatch.End();
        }
        else
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer);
            spriteBatch.Draw(moon, iconRect, Color.White);
            spriteBatch.End();
        }

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer);

        Rectangle oceanSource = new Rectangle(
            (int)(mediumPan * ocean.Width), 0,
            iconWidth, iconHeight
        );

        Rectangle medCloudSource = new Rectangle(
            (int)(mediumPan * medCloud.Width), 0,
            iconWidth, iconHeight
        );
        spriteBatch.Draw(medCloud, iconRect, medCloudSource, Color.White * 0.95f);

        Rectangle bigCloudSource = new Rectangle(
            (int)(slowPan * bigCloud.Width), 0,
            iconWidth, iconHeight
        );
        spriteBatch.Draw(bigCloud, iconRect, bigCloudSource, Color.White * 0.5f);

        Rectangle tinyCloudSource = new Rectangle(
            (int)(fastPan * bigCloud.Width), 0,
            iconWidth, iconHeight
        );
        spriteBatch.Draw(tinyCloud, iconRect, tinyCloudSource, Color.White * 0.5f);

        spriteBatch.Draw(foreground, iconRect, oceanSource, Color.White * 0.75f);
        spriteBatch.End();

        var shine2 = ShaderLoader.GetShader("ShineShader")?.Value;
        if (shine2 != null)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, shineShader);

            shine2.Parameters["uTime"]?.SetValue(time * 4f);
            shine2.Parameters["uOpacity"]?.SetValue(2f);
            spriteBatch.Draw(ocean, iconRect, oceanSource, Color.White);
            spriteBatch.End();
        }
        else
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer);
            spriteBatch.Draw(ocean, iconRect, oceanSource, Color.White);
            spriteBatch.End();
        }

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer);
        spriteBatch.Draw(frame, iconRect, Color.White);
        spriteBatch.End();
    }

    public override void Unload()
    {

        if (loadCache != null)
        {
            foreach (IOrderedLoadable loadable in loadCache)
            {
                loadable.Unload();
            }

            loadCache = null;
        }
        else
        {
            Logger.Warn("load cache was null, IOrderedLoadable's may not have been unloaded...");
        }

        if (!Main.dedServ)
        {
            Instance ??= null;
        }

        AnimatedModIconHelper.UnloadAnimatedModIcon();
    }
}
