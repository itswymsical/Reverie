
using AnimatedModIconLib.Core;
using ReLogic.Content;
using Reverie.Common.Players;
using Reverie.Common.Systems;
using Reverie.Core.Graphics.Interfaces;
using Reverie.Core.Loaders;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.UI.Chat;

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
    /// <summary>
    ///     The prefix to use for the name of this mod.
    /// </summary>
    public const string NAME_PREFIX = NAME + ": ";


    public const string DIALOGUE_LIBRARY = "DialogueLibrary.";

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
        Texture2D background = ModContent.Request<Texture2D>(LOGO_DIRECTORY + "nebula").Value;
        Texture2D border = ModContent.Request<Texture2D>(LOGO_DIRECTORY + "icon").Value;

        int iconWidth = (int)size.X;
        int iconHeight = (int)size.Y;

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer);

        float panSpeed = .3f;
        float offsetX = (float)(Main.timeForVisualEffects * panSpeed) % background.Width;

        Rectangle sourceRect = new Rectangle(
            (int)offsetX,
            0,
            iconWidth,
            iconHeight
        );

        Rectangle destRect = new Rectangle(0, 0, iconWidth, iconHeight);
        spriteBatch.Draw(background, destRect, sourceRect, Color.White);
        spriteBatch.End();

        var starEffect = ShaderLoader.GetShader("SunburstShader").Value;

        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, starEffect, Main.UIScaleMatrix);

        if (starEffect != null)
        {
            starEffect.Parameters["uTime"]?.SetValue((float)(Main.timeForVisualEffects * 0.009f));

            starEffect.Parameters["uScreenResolution"]?.SetValue(size);
            starEffect.Parameters["uSourceRect"]?.SetValue(new Vector4(0, 0, size.X, size.Y));

            starEffect.Parameters["uIntensity"]?.SetValue(2.5f);
            starEffect.Parameters["uRayCount"]?.SetValue(5f);

            starEffect.Parameters["uCenter"]?.SetValue(Vector2.Zero);
            starEffect.Parameters["uScale"]?.SetValue(1f);

            starEffect.Parameters["uImage0"]?.SetValue(ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Perlin").Value);

            starEffect.CurrentTechnique.Passes[0].Apply();
        }

        var berlin = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Perlin").Value;
        var benis = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}EnergyTrail").Value;
        Rectangle fullRect = new Rectangle(0, 0, iconWidth, iconHeight);
        spriteBatch.Draw(berlin, fullRect, Color.White);
        spriteBatch.Draw(benis, fullRect, Color.White);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.DepthRead, Main.Rasterizer);
        spriteBatch.Draw(border, new Rectangle(0, 0, iconWidth, iconHeight), Color.White);
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

    public enum MessageType : byte
    {
        AddExperience,
        ClassStatPlayerSync
    }

    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        MessageType msgType = (MessageType)reader.ReadByte();

        switch (msgType)
        {
            case MessageType.AddExperience:
                int playerID = reader.ReadInt32();
                int experience = reader.ReadInt32();
                if (playerID >= 0 && playerID < Main.maxPlayers)
                {
                    Player player = Main.player[playerID];
                    ExperiencePlayer.AddExperience(player, experience);
                    CombatText.NewText(player.Hitbox, Color.LightGoldenrodYellow, $"+{experience} Exp", true);
                }
                break;

            default:
                Logger.WarnFormat($"{NAME + NAME_PREFIX} Unknown Message type: {0}", msgType);
                break;
        }
    }
}