using ReLogic.Content;
using Reverie.Common.Systems;
using Reverie.Core.Dialogue;
using Reverie.Utilities;
using System.Collections.Generic;
using System.Text;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.UI;

namespace Reverie.Common.UI.Dialogue;

public class DialogueBox : IInGameNotification
{
    #region Constants and Fields
    private const float ANIMATION_DURATION = 30f;
    private const float PANEL_WIDTH = 420f;

    private bool isRemoved;
    private float animationProgress;

    private int autoRemoveTimer;
    private const int AUTO_REMOVE_DELAY = 200; // read quick mf

    private bool isLastDialogue;
    private Vector2 targetPosition;
    private Vector2 startPosition;
    private int charDisplayTimer;
    private int charIndex;
    private int currentEmoteFrame;

    private readonly Queue<DialogueEntry> currentSequence = new();
    private DialogueEntry currentDialogue;
    public required NPCData npcData;
    private NPCData currentSpeakingNPC;

    private readonly Texture2D ArrowTexture = ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Dialogue/ArrowForward", AssetRequestMode.ImmediateLoad).Value;
    private readonly Texture2D PortraitFrameTexture = ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Dialogue/PortraitFrame", AssetRequestMode.ImmediateLoad).Value;
    #endregion

    #region Properties
    public bool ShouldZoom { get; set; }
    public bool ShouldBeRemoved => isRemoved && animationProgress >= ANIMATION_DURATION;
    public float Opacity => isRemoved ? 1f - animationProgress / ANIMATION_DURATION : MathHelper.Clamp(animationProgress / ANIMATION_DURATION, 0f, 1f);
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
            Color = npcData.BoxColor,
            CharacterSound = npcData.TalkSFX,
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
        else if(!isRemoved)
        {
            isRemoved = true;
            isLastDialogue = true;
            animationProgress = 0f;
        }
    }

    private void UpdateAnimation()
    {
        if (!isRemoved || animationProgress < ANIMATION_DURATION)
        {
            animationProgress = Math.Min(animationProgress + 1f, ANIMATION_DURATION);
        }
    }

    private void UpdateDisplay()
    {
        var currentDialogueText = currentDialogue.GetText();

        // Fast forward for you losers who hate to read
        if (ReverieSystem.FFDialogueKeybind.JustPressed)
        {
            if (charIndex < currentDialogueText.Value.Length)
            {
                charIndex = currentDialogueText.Value.Length;
                PlayCharacterSound(CharacterSound);
            }
            else if (currentSequence.Count > 0)
            {
                NextEntry();
                return;
            }
            else if (!isRemoved)
            {
                isRemoved = true;
                isLastDialogue = true;
                animationProgress = 0f;
            }
        }

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
                autoRemoveTimer = AUTO_REMOVE_DELAY;
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

        var isHovering = panelRectangle.Contains(Main.MouseScreen.ToPoint());
        DrawUtils.DrawPanel(spriteBatch, panelRectangle, currentSpeakingNPC.BoxColor * (isHovering ? 0.75f : 0.5f) * Opacity);

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

        Vector2 frameSize = new(PortraitFrameTexture.Width, PortraitFrameTexture.Height);
        var framePosition = iconPosition - (frameSize - iconSize) / 2;
        spriteBatch.Draw(PortraitFrameTexture, framePosition, null, IconColor * Opacity, 0f, Vector2.Zero, 1.2f, SpriteEffects.None, 0f);

        var sourceRect = currentSpeakingNPC.Portrait.GetFrameRect(currentEmoteFrame);
        spriteBatch.Draw(currentSpeakingNPC.Portrait.Texture.Value, iconPosition, sourceRect, IconColor * Opacity, 0f, Vector2.Zero, 1.2f, SpriteEffects.None, 0f);

        DrawNpcName(spriteBatch, iconPosition, iconSize);
    }

    private void DrawNpcName(SpriteBatch spriteBatch, Vector2 iconPosition, Vector2 iconSize)
    {
        var currentNpcName = currentSpeakingNPC.GetNpcGivenName(currentSpeakingNPC.NpcID);
        var nameTextSize = FontAssets.ItemStack.Value.MeasureString(currentNpcName);
        Vector2 nameTextPosition = new(iconPosition.X + (iconSize.X + 16f) / 2 - nameTextSize.X / 2, iconPosition.Y + iconSize.Y + 24f);

        Utils.DrawBorderString(spriteBatch, currentNpcName, nameTextPosition, DefaultTextColor * Opacity, 1f, anchorx: 0f, anchory: 0.5f);
    }

    private void DrawText(SpriteBatch spriteBatch)
    {
        var panelSize = CalculatePanelSize();
        var panelPosition = CalculatePanelPosition(panelSize);
        var panelRectangle = Utils.CenteredRectangle(panelPosition, panelSize);

        var currentDialogueText = currentDialogue.GetText();
        var displayText = currentDialogueText.Value.Substring(0, Math.Min(charIndex, currentDialogueText.Value.Length));

        var textPosition = panelRectangle.TopLeft() + new Vector2(10f, 20f);

        var wrappedText = WrapText(displayText, PANEL_WIDTH - 10f);
        for (var i = 0; i < wrappedText.Length; i++)
        {
            var linePosition = textPosition + new Vector2(0f, i * FontAssets.ItemStack.Value.LineSpacing);
            var textColor = currentDialogue.EntryTextColor ?? DefaultTextColor;
            Utils.DrawBorderString(spriteBatch, wrappedText[i], linePosition, textColor * Opacity, .9f, anchorx: 0f, anchory: 0.5f);
        }
    }

    private void DrawArrow(SpriteBatch spriteBatch)
    {
        var currentDialogueText = currentDialogue.GetText();

        if (charIndex < currentDialogueText.Value.Length || currentSequence.Count == 0 && isLastDialogue) return;

        var panelSize = CalculatePanelSize();
        var panelPosition = CalculatePanelPosition(panelSize);
        var panelRectangle = Utils.CenteredRectangle(panelPosition, panelSize);

        var arrowScale = 0.75f;
        var arrowPosition = panelRectangle.BottomRight() - new Vector2(ArrowTexture.Width * arrowScale + 10, ArrowTexture.Height * arrowScale + 10);

        var yOffset = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 3f;
        arrowPosition.Y += yOffset;

        spriteBatch.Draw(ArrowTexture, arrowPosition, null, Color.White * Opacity, 0f, Vector2.Zero, arrowScale, SpriteEffects.None, 0f);
    }

    private void OnMouseOver()
    {
        if (PlayerInput.IgnoreMouseInterface) return;

        Main.LocalPlayer.mouseInterface = true;

        if (!Main.mouseLeft || !Main.mouseLeftRelease || isLastDialogue && isRemoved) return;

        Main.mouseLeftRelease = false;

        var currentDialogueText = currentDialogue.GetText();

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
        var currentDialogueText = currentDialogue.GetText();
        var displayText = currentDialogueText.Value[..Math.Min(charIndex, currentDialogueText.Value.Length)];

        var wrappedText = WrapText(displayText, PANEL_WIDTH);
        var lineCount = wrappedText.Length;
        var textHeight = FontAssets.ItemStack.Value.MeasureString(displayText).Y * lineCount;

        var panelHeight = textHeight + new Vector2(35f, 25f).Y * 2.7f;
        return new Vector2(PANEL_WIDTH + 18, panelHeight);
    }

    private Vector2 CalculatePanelPosition(Vector2 panelSize)
    {
        var t = isRemoved ? animationProgress / ANIMATION_DURATION : 1f - animationProgress / ANIMATION_DURATION;
        var currentPosition = Vector2.Lerp(targetPosition, startPosition, t);
        return currentPosition - new Vector2(0f, panelSize.Y * 0.4f);
    }

    private static void PlayCharacterSound(SoundStyle sound) => SoundEngine.PlaySound(sound, Main.LocalPlayer.position);

    private static string[] WrapText(string text, float maxWidth)
    {
        List<string> lines = [];
        StringBuilder currentLine = new();
        var spaceWidth = FontAssets.ItemStack.Value.MeasureString(" ").X;

        foreach (var word in text.Split(' '))
        {
            var wordWidth = FontAssets.ItemStack.Value.MeasureString(word).X;
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