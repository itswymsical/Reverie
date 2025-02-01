using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;
using Terraria.ModLoader;
using Reverie.Common.Players;
using Reverie.Core.Missions;
using Terraria.GameContent.UI.Elements;
using System.Collections.Generic;
using Terraria.Audio;
using Reverie.Common.UI.MissionUI;

namespace Reverie.Common.UI
{
    internal class MissionButtonUI : UIState
    {
        public UIPanel missionPanel;
        public static bool Visible;
        private MissionPlayer missionPlayer => Main.LocalPlayer.GetModPlayer<MissionPlayer>();
        private Dictionary<int, UIHoverImageButton> missionButtons = [];

        public override void OnInitialize()
        {
            missionPanel = new UIPanel();
            missionPanel.SetPadding(0);
            missionPanel.Left.Set(380f, 0f);
            missionPanel.Top.Set(100f, 0f);
            missionPanel.Width.Set(180f, 0f);
            missionPanel.Height.Set(180f, 0f);
            Append(missionPanel);

            UIText header = new UIText("Missions", 0.7f);
            header.Top.Set(12f, 0);
            missionPanel.Append(header);
        }

        public void UpdateAvailableMissions(int npcType)
        {
            // Clear existing buttons
            foreach (var button in missionButtons.Values)
            {
                missionPanel.RemoveChild(button);
            }
            missionButtons.Clear();

            // Get available missions for this NPC
            if (!missionPlayer.npcMissionsDict.TryGetValue(npcType, out var missionIds))
                return;

            float topOffset = 45f;
            foreach (var missionId in missionIds)
            {
                var mission = missionPlayer.GetMission(missionId);
                if (mission?.State != MissionAvailability.Unlocked || mission.Progress != MissionProgress.Inactive)
                    continue;

                string iconPath = "Reverie/Assets/UI/MissionUI/MissionAvailable";

                var button = new UIHoverImageButton(ModContent.Request<Texture2D>(iconPath),
                    $"{(mission.MissionData.IsMainline ? "Story Mission" : "Job Opportunity")}: {mission.MissionData.Name}"
                );

                button.Left.Set(15f, 0f);
                button.Top.Set(topOffset, 0f);
                button.Width.Set(32f, 0f);
                button.Height.Set(32f, 0f);
                button.OnLeftClick += (evt, element) => AcceptMission(mission);
                missionPanel.Append(button);
                missionButtons[missionId] = button;

                topOffset += 40f;
            }

            // Update panel height based on number of missions
            float requiredHeight = topOffset + 20f;
            missionPanel.Height.Set(System.Math.Max(180f, requiredHeight), 0f);
        }

        private void AcceptMission(Mission mission)
        {
            missionPlayer.StartMission(mission.ID);
            InGameNotificationsTracker.AddNotification(new MissionAcceptNotification(mission));
            Main.npcChatText = $"{mission.MissionData.Description}";
            Visible = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (Main.LocalPlayer.talkNPC == -1 || Main.playerInventory)
            {
                ModContent.GetInstance<MissionUISystem>().HideMissionInterface();
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            Main.hidePlayerCraftingMenu = true;
        }
    }

    internal class UIHoverImageButton : UIImageButton
    {
        internal string HoverText;

        public UIHoverImageButton(ReLogic.Content.Asset<Texture2D> texture, string hoverText) : base(texture)
        {
            HoverText = hoverText;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            if (IsMouseHovering)
            {
                Main.hoverItemName = HoverText;
            }
        }
    }

    public class MissionUISystem : ModSystem
    {
        internal UserInterface MissionButtonInterface;
        internal MissionButtonUI MissionButtonUI;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                MissionButtonInterface = new UserInterface();
                MissionButtonUI = new MissionButtonUI();
                MissionButtonUI.Initialize();
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (MissionButtonInterface?.CurrentState != null)
            {
                MissionButtonInterface.Update(gameTime);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "Reverie: Mission Interface",
                    delegate
                    {
                        if (MissionButtonInterface?.CurrentState != null)
                        {
                            MissionButtonInterface.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }

        public void ShowMissionInterface(int npcType)
        {
            if (Main.LocalPlayer.talkNPC == -1 || Main.playerInventory) return;

            MissionButtonUI.Visible = true;
            MissionButtonUI.UpdateAvailableMissions(npcType);
            MissionButtonInterface.SetState(MissionButtonUI);
        }

        public void HideMissionInterface()
        {
            MissionButtonUI.Visible = false;
            MissionButtonInterface.SetState(null);
        }

        public override void Unload()
        {
            MissionButtonInterface = null;
            MissionButtonUI = null;
        }
    }
}