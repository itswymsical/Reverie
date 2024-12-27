using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using System.Linq;
using System.Text;
using Reverie.Common.Players;

namespace Reverie.Common.UI.MissionUI
{
    public class MissionObjectives : InfoDisplay
    {
        public override string Texture => $"{Assets.UI.MissionUI}MissionObjectives";
        public override string HoverTexture => Texture + "_Hover";
        public override bool Active() => true;

        private const string EMPTY_CHECKBOX = "☐";
        private const string CHECKED_CHECKBOX = "☑";

        public override string DisplayValue(ref Color displayColor, ref Color displayShadowColor)
        {
            var missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
            var activeMissions = missionPlayer.GetActiveMissions().ToList();

            if (activeMissions.Count == 0)
            {
                displayColor = Color.Gray;
                displayShadowColor = Color.Black;
                return "No active missions";
            }

            StringBuilder display = new();
            foreach (var mission in activeMissions)
            {
                display.AppendLine($"{mission.MissionData.Name}");

                var currentSet = mission.MissionData.ObjectiveSets[mission.CurrentSetIndex];
                foreach (var objective in currentSet.Objectives)
                {
                    string status = objective.IsCompleted ? CHECKED_CHECKBOX : EMPTY_CHECKBOX;
                    if (objective.RequiredCount > 1)
                    {
                        display.AppendLine($"{status} {objective.Description} [{objective.CurrentCount}/{objective.RequiredCount}]");
                    }

                    else
                    {
                        display.AppendLine($"{status} {objective.Description}");
                    }
                }
                displayColor = Color.White;
                display.AppendLine();
            }

            displayShadowColor = Color.Black;
            return display.ToString().TrimEnd();
        }
    }
}