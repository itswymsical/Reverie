
using Reverie.Common.Players;
using Reverie.Core.Interfaces;
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
        if (!Main.dedServ)
        {
            Filters.Scene["ScreenRipple"] = new Filter(new ScreenShaderData("FilterMiniTower").UseImage("Images/Misc/Perlin").UseImage("Images/Misc/noise").UseImage("Images/Misc/Perlin").UseImage("Images/Misc/noise"), EffectPriority.VeryHigh);
            Filters.Scene["ScreenRipple"].Load();
        }

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
    }

    [Obsolete]
    public override void AddRecipes()
    {

        Recipe iceBlade = Recipe.Create(ItemID.IceBlade);
        iceBlade.AddIngredient(ItemID.IceBlock, 30)
            .AddIngredient(ItemID.FallenStar, 4)
            .AddCondition(Condition.InSnow)
            .AddTile(TileID.IceMachine)
            .Register();
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