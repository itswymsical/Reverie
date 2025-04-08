using Reverie.Core.Missions;
using System.Collections.Generic;

namespace Reverie.Core.CustomEntities;

/// <summary>
/// Manages the creation and tracking of mission indicators in the world
/// </summary>
public class MissionIndicatorManager : ModSystem
{
    public static MissionIndicatorManager Instance { get; set; }
    public MissionIndicatorManager() => Instance = this;

    private readonly List<MissionIndicator> indicators = [];

    private readonly Dictionary<int, MissionIndicator> npcIndicators = [];

    public override void Unload()
    {
        if (!Main.dedServ)
        {
            Instance ??= null;
        }
        indicators.Clear();
        npcIndicators.Clear();
    }

    /// <summary>
    /// Creates a mission indicator at a specific world position
    /// </summary>
    public MissionIndicator CreateIndicator(Vector2 worldPosition, Mission mission)
    {
        var indicator = new MissionIndicator(worldPosition, mission);
        indicators.Add(indicator);
        return indicator;
    }

    /// <summary>
    /// Creates a mission indicator for an NPC
    /// </summary>
    public MissionIndicator CreateIndicatorForNPC(NPC npc, Mission mission)
    {
        // Don't create duplicate indicators
        if (npcIndicators.TryGetValue(npc.whoAmI, out var value))
            return value;

        var indicator = new MissionIndicator(npc.Top, mission);
        indicators.Add(indicator);
        npcIndicators[npc.whoAmI] = indicator;

        return indicator;
    }

    public override void PostUpdateEverything()
    {
        for (var i = indicators.Count - 1; i >= 0; i--)
        {
            indicators[i].Update();

            if (!indicators[i].IsVisible)
            {
                // remove from NPC tracking if applicable
                foreach (var kvp in npcIndicators)
                {
                    if (kvp.Value == indicators[i])
                    {
                        npcIndicators.Remove(kvp.Key);
                        break;
                    }
                }

                indicators.RemoveAt(i);
            }
        }
    }

    public override void PostDrawTiles()
    {
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
            DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

        Instance.Draw(Main.spriteBatch);

        Main.spriteBatch.End();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var indicator in indicators)
        {
            indicator.Draw(spriteBatch);
        }
    }
}