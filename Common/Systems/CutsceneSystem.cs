using Reverie.Core.Cinematics;
using Reverie.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Terraria.UI;

namespace Reverie.Common.Systems;

public class CutsceneSystem : ModSystem
{
    public static Cutscene CurrentCutscene { get; private set; }

    private static readonly string[] UILayersToHide =
    [
        "Vanilla: Inventory",
        "Vanilla: Hotbar",
        "Vanilla: Resource Bars",
        "Vanilla: Map / Minimap",
        "Vanilla: Info Accessories Bar",
        "Vanilla: Builder Accessories Bar",
        "Vanilla: Settings Button",
        "Vanilla: Mouse Over",
        "Vanilla: Radial Hotbars",
        "Vanilla: Player Chat"
    ];

    public override void Load()
    {
        On_Main.DrawInterface += DrawCutscene;
        On_Main.DoDraw += UpdateCutscene;
    }

    public override void Unload()
    {
        On_Main.DrawInterface -= DrawCutscene;
        On_Main.DoDraw -= UpdateCutscene;
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        if (CurrentCutscene != null)
        {
            foreach (var layer in layers)
            {
                if (UILayersToHide.Contains(layer.Name))
                {
                    layer.Active = false;
                }
                else if (layer.Name == "Vanilla: NPC / Sign Dialog" ||
                         layer.Name == "Vanilla: Achievement Complete Popups")
                {
                    layer.Active = true;
                }
            }
        }
    }

    private void UpdateCutscene(On_Main.orig_DoDraw orig, Main self, GameTime gameTime)
    {
        orig(self, gameTime);

        if (CurrentCutscene != null)
        {
            CurrentCutscene.Update(gameTime);
            if (CurrentCutscene.IsFinished())
            {
                CurrentCutscene.End();
                CurrentCutscene = null;
            }
        }
    }

    private void DrawCutscene(On_Main.orig_DrawInterface orig, Main self, GameTime gameTime)
    {
        try
        {
            if (CurrentCutscene != null)
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    SamplerState.LinearClamp,
                    DepthStencilState.None,
                    Main.Rasterizer,
                    null,
                    Main.UIScaleMatrix);

                if (CurrentCutscene is IDrawCutscene customDrawCutscene)
                {
                    customDrawCutscene.CustomDraw(Main.spriteBatch);
                }
                else
                {
                    CurrentCutscene.Draw(Main.spriteBatch);
                }

                Main.spriteBatch.End();
            }

            orig(self, gameTime);
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error in DrawCutscene: {ex.Message}\nStack Trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Starts playing a new cutscene.
    /// </summary>
    /// <param name="cutscene">The cutscene to play.</param>
    public static void PlayCutscene(Cutscene cutscene)
    {
        try
        {
            if (CurrentCutscene != null)
            {
                ModContent.GetInstance<Reverie>().Logger.Warn("Attempting to start a new cutscene while one is already in progress. Ending the current cutscene.");
                CurrentCutscene.End();
            }

            CurrentCutscene = cutscene;
            CurrentCutscene.Start();
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error("Error playing cutscene: " + ex.Message);
        }
    }
}
