using Reverie.Core.Missions;
using System.Collections.Generic;

namespace Reverie.Core.Entities;

/// <summary>
/// Manages the creation and tracking of mission indicators in the world
/// </summary>
public class MissionIndicatorManager : ModSystem
{
    public static MissionIndicatorManager Instance { get; set; }
    public MissionIndicatorManager() => Instance = this;

    private readonly List<MissionIndicator> indicators = [];

    private readonly Dictionary<int, MissionIndicator> npcIndicators = [];
    private Dictionary<int, int> npcMissionTracking = [];

    public override void Unload()
    {
        if (!Main.dedServ)
        {
            Instance = null;
        }
        indicators.Clear();
        npcIndicators.Clear();
    }
    public override void OnWorldUnload()
    {
        base.OnWorldUnload();

        indicators.Clear();
        npcIndicators.Clear();
    }

    public MissionIndicator CreateIndicator(Vector2 worldPosition, Mission mission)
    {
        var indicator = new MissionIndicator(worldPosition, mission);
        indicators.Add(indicator);
        return indicator;
    }

    public MissionIndicator CreateIndicatorForNPC(NPC npc, Mission mission)
    {
        int npcIndex = npc.whoAmI;

        if (npcIndicators.ContainsKey(npcIndex))
        {
            if (npcMissionTracking.ContainsKey(npcIndex) && npcMissionTracking[npcIndex] != mission.ID)
            {
                var oldIndicator = npcIndicators[npcIndex];
                indicators.Remove(oldIndicator);
                npcIndicators.Remove(npcIndex);

                var indicator = MissionIndicator.CreateForNPC(npc, mission);
                indicators.Add(indicator);
                npcIndicators[npcIndex] = indicator;
                npcMissionTracking[npcIndex] = mission.ID;
                return indicator;
            }

            return npcIndicators[npcIndex];
        }

        var newIndicator = MissionIndicator.CreateForNPC(npc, mission);
        indicators.Add(newIndicator);
        npcIndicators[npcIndex] = newIndicator;
        npcMissionTracking[npcIndex] = mission.ID;

        return newIndicator;
    }

    public override void PostUpdateEverything()
    {
        for (var i = indicators.Count - 1; i >= 0; i--)
        {
            indicators[i].Update();

            if (!indicators[i].IsVisible)
            {
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

    public override void PostDrawInterface(SpriteBatch spriteBatch)
    {
        //spriteBatch.End();
        //Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
        //DepthStencilState.None, RasterizerState.CullNone, null);

        Instance.Draw(spriteBatch);

        //Main.spriteBatch.End();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var indicator in indicators)
        {
            indicator.Draw(spriteBatch);
        }
    }

    public void ClearAllNotifications()
    {
        indicators.Clear();
        npcIndicators.Clear();
        npcMissionTracking.Clear();
    }
}