using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Reverie.Core.Dialogue;
using Reverie.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace Reverie.Common.UI
{
    public class DialogueBox : IInGameNotification
    {
        #region Constants and Fields
        private const float AnimationDuration = 30f;
        private const float PanelWidth = 420f;

        private bool isRemoved;
        private float animationProgress;

        private int autoRemoveTimer;
        private const int AutoRemoveDelay = 330; //5.5 seconds, read quick mf

        private bool isLastDialogue;
        private Vector2 targetPosition;
        private Vector2 startPosition;
        private int charDisplayTimer;
        private int charIndex;
        private int currentEmoteFrame;

        private readonly Queue<DialogueEntry> currentSequence = new();
        private DialogueEntry currentDialogue;
        public NPCData npcData;
        private NPCData currentSpeakingNPC;

        private static Texture2D arrowTexture;
        private static Texture2D portraitFrameTexture;
        #endregion

        #region Properties
        public bool ShouldZoom { get; set; }
        public bool ShouldBeRemoved => isRemoved && animationProgress >= AnimationDuration;
        public float Opacity => isRemoved ? 1f - (animationProgress / AnimationDuration) : MathHelper.Clamp(animationProgress / AnimationDuration, 0f, 1f);
        public Color DefaultTextColor { get; init; } = Color.White;
        public Color IconColor { get; init; } = Color.White;
        public NPCPortrait CurrentPortrait { get; init; }
        public string NpcName { get; init; } = string.Empty;
        public Color Color { get; init; } = Color.White;
        public SoundStyle CharacterSound { get; init; } = SoundID.MenuOpen;
        public int CurrentEntryIndex { get; private set; }
        #endregion

        #region Public Methods
        public static DialogueBox CreateNewSequence(NPCData npcData, DialogueSequence sequence, bool zoomIn)
        {
            var notification = new DialogueBox
            {
                CurrentPortrait = npcData.Portrait,
                npcData = npcData,
                NpcName = npcData.NpcName,
                Color = npcData.DialogueColor,
                CharacterSound = npcData.CharacterSound,
                ShouldZoom = zoomIn
            };

            foreach (var entry in sequence.Entries)
            {
                notification.currentSequence.Enqueue(entry);
            }

            notification.NextEntry();

            return notification;
        }

        public void Update()
        {
            UpdateAnimation();
            UpdateDisplay();
            UpdateAutoRemove();
        }

        public void DrawInGame(SpriteBatch spriteBatch, Vector2 bottomAnchorPosition)
        {
            if (Opacity <= 0f || Main.gameMenu) return;

            LoadTextures();
            SetupPositions(bottomAnchorPosition);

            DrawPanel(spriteBatch);
            DrawPortrait(spriteBatch);
            DrawText(spriteBatch);
            DrawArrow(spriteBatch);
        }

        public void PushAnchor(ref Vector2 positionAnchorBottom) => positionAnchorBottom.Y -= 50f * Opacity;

        public bool IsCurrentEntry(int entryIndex) => CurrentEntryIndex == entryIndex && !ShouldBeRemoved;
        #endregion

        #region Private Methods
        private void NextEntry()
        {
            if (currentSequence.Count > 0)
            {
                currentDialogue = currentSequence.Dequeue();
                charIndex = 0;
                autoRemoveTimer = 0;
                currentEmoteFrame = currentDialogue.EmoteFrame;
                CurrentEntryIndex++;

                currentSpeakingNPC = currentDialogue.SpeakingNPC ?? npcData;
            }
            else
            {
                isLastDialogue = true;
            }
        }

        private void UpdateAnimation()
        {
            if (!isRemoved || animationProgress < AnimationDuration)
            {
                animationProgress = Math.Min(animationProgress + 1f, AnimationDuration);
            }
        }

        private void UpdateDisplay()
        {
            LocalizedText currentDialogueText = currentDialogue.GetText();

            if (charIndex < currentDialogueText.Value.Length)
            {
                charDisplayTimer++;
                if (charDisplayTimer >= currentDialogue.Delay)
                {
                    charDisplayTimer = 0;
                    charIndex++;
                    PlayCharacterSound(CharacterSound);
                }
            }
            else
            {
                if (autoRemoveTimer == 0)
                {
                    autoRemoveTimer = AutoRemoveDelay;
                }
            }
        }

        private void UpdateAutoRemove()
        {
            if (Main.gameMenu)
            {
                isRemoved = true;
                animationProgress = 0f;
            }

            if (autoRemoveTimer > 0)
            {
                autoRemoveTimer--;
                if (autoRemoveTimer == 0)
                {
                    NextEntry();
                }
            }
        }

        private static void LoadTextures()
        {
            arrowTexture = ModContent.Request<Texture2D>($"{Assets.UI.Directory}ArrowForward").Value;
            portraitFrameTexture = ModContent.Request<Texture2D>($"{Assets.UI.DialogueUI}PortraitFrame").Value;
        }

        private void SetupPositions(Vector2 bottomAnchorPosition)
        {
            if (targetPosition == Vector2.Zero)
            {
                var panelSize = CalculatePanelSize();
                targetPosition = bottomAnchorPosition + new Vector2(0f, -panelSize.Y * 0.5f);
                startPosition = targetPosition + new Vector2(0f, panelSize.Y + 70f);
            }
        }

        private void DrawPanel(SpriteBatch spriteBatch)
        {
            var panelSize = CalculatePanelSize();
            var panelPosition = CalculatePanelPosition(panelSize);
            var panelRectangle = Utils.CenteredRectangle(panelPosition, panelSize);

            bool isHovering = panelRectangle.Contains(Main.MouseScreen.ToPoint());
            Helper.DrawPanel(spriteBatch, panelRectangle, currentSpeakingNPC.DialogueColor * (isHovering ? 0.75f : 0.5f) * Opacity);

            if (isHovering) OnMouseOver();
        }

        private void DrawPortrait(SpriteBatch spriteBatch)
        {
            if (currentSpeakingNPC.Portrait.Texture == null) return;

            var panelSize = CalculatePanelSize();
            var panelPosition = CalculatePanelPosition(panelSize);
            var panelRectangle = Utils.CenteredRectangle(panelPosition, panelSize);

            Vector2 iconSize = new(currentSpeakingNPC.Portrait.FrameWidth, currentSpeakingNPC.Portrait.FrameHeight);
            Vector2 iconPosition = new(panelRectangle.Left - iconSize.X - 28f, panelRectangle.Center.Y - 50f);

            Vector2 frameSize = new(portraitFrameTexture.Width, portraitFrameTexture.Height);
            Vector2 framePosition = iconPosition - (frameSize - iconSize) / 2;
            spriteBatch.Draw(portraitFrameTexture, framePosition, null, IconColor * Opacity, 0f, Vector2.Zero, 1.2f, SpriteEffects.None, 0f);

            Rectangle sourceRect = currentSpeakingNPC.Portrait.GetFrameRect(currentEmoteFrame);
            spriteBatch.Draw(currentSpeakingNPC.Portrait.Texture.Value, iconPosition, sourceRect, IconColor * Opacity, 0f, Vector2.Zero, 1.2f, SpriteEffects.None, 0f);

            DrawNpcName(spriteBatch, iconPosition, iconSize);
        }

        private void DrawNpcName(SpriteBatch spriteBatch, Vector2 iconPosition, Vector2 iconSize)
        {
            string currentNpcName = currentSpeakingNPC.GetNpcGivenName(currentSpeakingNPC.NpcID);
            Vector2 nameTextSize = FontAssets.ItemStack.Value.MeasureString(currentNpcName);
            Vector2 nameTextPosition = new(iconPosition.X + (iconSize.X + 16f) / 2 - nameTextSize.X / 2, iconPosition.Y + iconSize.Y + 24f);

            Utils.DrawBorderString(spriteBatch, currentNpcName, nameTextPosition, DefaultTextColor * Opacity, 1f, anchorx: 0f, anchory: 0.5f);
        }

        private void DrawText(SpriteBatch spriteBatch)
        {
            var panelSize = CalculatePanelSize();
            var panelPosition = CalculatePanelPosition(panelSize);
            var panelRectangle = Utils.CenteredRectangle(panelPosition, panelSize);

            LocalizedText currentDialogueText = currentDialogue.GetText();
            string displayText = currentDialogueText.Value.Substring(0, Math.Min(charIndex, currentDialogueText.Value.Length));

            Vector2 textPosition = panelRectangle.TopLeft() + new Vector2(10f, 20f);

            string[] wrappedText = WrapText(displayText, PanelWidth - 10f);
            for (int i = 0; i < wrappedText.Length; i++)
            {
                Vector2 linePosition = textPosition + new Vector2(0f, i * FontAssets.ItemStack.Value.LineSpacing);
                Color textColor = currentDialogue.EntryTextColor ?? DefaultTextColor;
                Utils.DrawBorderString(spriteBatch, wrappedText[i], linePosition, textColor * Opacity, .9f, anchorx: 0f, anchory: 0.5f);
            }
        }

        private void DrawArrow(SpriteBatch spriteBatch)
        {
            LocalizedText currentDialogueText = currentDialogue.GetText();

            if (charIndex < currentDialogueText.Value.Length || (currentSequence.Count == 0 && isLastDialogue)) return;

            var panelSize = CalculatePanelSize();
            var panelPosition = CalculatePanelPosition(panelSize);
            var panelRectangle = Utils.CenteredRectangle(panelPosition, panelSize);

            float arrowScale = 0.75f;
            Vector2 arrowPosition = panelRectangle.BottomRight() - new Vector2(arrowTexture.Width * arrowScale + 10, arrowTexture.Height * arrowScale + 10);

            float yOffset = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 3f;
            arrowPosition.Y += yOffset;

            spriteBatch.Draw(arrowTexture, arrowPosition, null, Color.White * Opacity, 0f, Vector2.Zero, arrowScale, SpriteEffects.None, 0f);
        }

        private void OnMouseOver()
        {
            if (PlayerInput.IgnoreMouseInterface) return;

            Main.LocalPlayer.mouseInterface = true;

            if (!Main.mouseLeft || !Main.mouseLeftRelease || (isLastDialogue && isRemoved)) return;

            Main.mouseLeftRelease = false;

            LocalizedText currentDialogueText = currentDialogue.GetText();

            if (charIndex < currentDialogueText.Value.Length)
            {
                charIndex = currentDialogueText.Value.Length;
            }
            else if (currentSequence.Count > 0)
            {
                NextEntry();
            }
            else
            {
                isRemoved = true;
                isLastDialogue = true;
                animationProgress = 0f;
            }
        }


        private Vector2 CalculatePanelSize()
        {
            LocalizedText currentDialogueText = currentDialogue.GetText();
            string displayText = currentDialogueText.Value.Substring(0, Math.Min(charIndex, currentDialogueText.Value.Length));

            string[] wrappedText = WrapText(displayText, PanelWidth);
            int lineCount = wrappedText.Length;
            float textHeight = FontAssets.ItemStack.Value.MeasureString(displayText).Y * lineCount;

            float panelHeight = textHeight + new Vector2(35f, 25f).Y * 2.7f;
            return new Vector2(PanelWidth + 18, panelHeight);
        }

        private Vector2 CalculatePanelPosition(Vector2 panelSize)
        {
            float t = isRemoved ? animationProgress / AnimationDuration : 1f - (animationProgress / AnimationDuration);
            Vector2 currentPosition = Vector2.Lerp(targetPosition, startPosition, t);
            return currentPosition - new Vector2(0f, panelSize.Y * 0.4f);
        }

        private static void PlayCharacterSound(SoundStyle sound) => SoundEngine.PlaySound(sound, Main.LocalPlayer.position);

        private static string[] WrapText(string text, float maxWidth)
        {
            List<string> lines = [];
            StringBuilder currentLine = new();
            float spaceWidth = FontAssets.ItemStack.Value.MeasureString(" ").X;

            foreach (var word in text.Split(' '))
            {
                float wordWidth = FontAssets.ItemStack.Value.MeasureString(word).X;
                if (currentLine.Length > 0 && FontAssets.ItemStack.Value.MeasureString(currentLine.ToString()).X + wordWidth + spaceWidth > maxWidth)
                {
                    lines.Add(currentLine.ToString());
                    currentLine.Clear();
                }
                if (currentLine.Length > 0)
                    currentLine.Append(" ");
                currentLine.Append(word);
            }

            if (currentLine.Length > 0)
                lines.Add(currentLine.ToString());

            return [.. lines];
        }
        #endregion
    }
}