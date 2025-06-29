using Reverie.Core.Dialogue;
using Reverie.Core.Indicators;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.UI;

namespace Reverie.Core.Indicators;

/// <summary>
/// An indicator for dialogue, appears in the world and shows dialogue preview
/// </summary>
public class DialogueIndicator : ScreenIndicator
{
    private readonly NPCData npcData;
    private readonly string dialogueKey;
    private readonly int lineCount;
    private readonly bool zoomIn;
    private readonly int defaultDelay;
    private readonly int defaultEmote;
    private readonly int? musicId;
    private readonly (int line, int delay, int emote)[] modifications;

    private readonly Texture2D iconTexture;

    private float hoverFadeIn = 0f;
    private const float HOVER_FADE_SPEED = 0.1f;

    private float wagTimer = 0f;
    private bool isWagging = false;
    private const float WAG_DURATION = 1f;
    private const float WAG_INTENSITY = 0.3f;

    private const int PANEL_WIDTH = 250;
    private const int PADDING = 10;

    private readonly float bobAmount = (float)Math.PI * 0.5f;

    public static bool ShowDebugHitbox = false;

    public DialogueIndicator(Vector2 worldPosition, NPCData npcData, string dialogueKey, int lineCount,
        bool zoomIn = false, int defaultDelay = 2, int defaultEmote = 0, int? musicId = null,
        params (int line, int delay, int emote)[] modifications)
         : base(worldPosition, 40, 40)
    {
        this.npcData = npcData;
        this.dialogueKey = dialogueKey;
        this.lineCount = lineCount;
        this.zoomIn = zoomIn;
        this.defaultDelay = defaultDelay;
        this.defaultEmote = defaultEmote;
        this.musicId = musicId;
        this.modifications = modifications;

        iconTexture = TextureAssets.Chat2.Value;
        if (iconTexture != null)
        {
            Width = iconTexture.Width;
            Height = iconTexture.Height;
        }

        OnDrawWorld = DrawIndicator;
        OnClick += HandleClick;
    }

    public static DialogueIndicator CreateForNPC(NPC npc, NPCData npcData, string dialogueKey, int lineCount,
        bool zoomIn = false, int defaultDelay = 2, int defaultEmote = 0, int? musicId = null,
        params (int line, int delay, int emote)[] modifications)
    {
        var indicator = new DialogueIndicator(npc.Top, npcData, dialogueKey, lineCount, zoomIn, defaultDelay, defaultEmote, musicId, modifications);
        indicator.TrackEntity(npc, new Vector2(0, -50));
        return indicator;
    }

    protected override void CustomUpdate()
    {
        if (IsHovering && hoverFadeIn < 1f)
        {
            hoverFadeIn += HOVER_FADE_SPEED;
            if (hoverFadeIn > 1f) hoverFadeIn = 1f;

            if (JustHovered)
            {
                SoundEngine.PlaySound(SoundID.MenuTick);
                // Start the finger wag animation
                isWagging = true;
                wagTimer = 0f;
            }
        }
        else if (!IsHovering && hoverFadeIn > 0f)
        {
            hoverFadeIn -= HOVER_FADE_SPEED;
            if (hoverFadeIn < 0f) hoverFadeIn = 0f;
        }

        // Update wag animation
        if (isWagging)
        {
            wagTimer += 1f / 60f; // Assuming 60 FPS
            if (wagTimer >= WAG_DURATION)
            {
                isWagging = false;
                wagTimer = 0f;
            }
        }

        // Always apply gentle bobbing
        Offset = new Vector2(0, (float)Math.Sin(AnimationTimer * 0.8f) * bobAmount);
    }

    private void DrawIndicator(SpriteBatch spriteBatch, Vector2 worldPos, float opacity)
    {
        if (iconTexture == null)
            return;

        var scale = IsHovering ? 1.2f : 1f;
        var glowColor = IsHovering ? Color.LightBlue : Color.White * 0.9f;

        // Finger wag rotation - only when wagging
        var rotation = 0f;
        if (isWagging)
        {
            // Create a gentle left-right wag motion
            var progress = wagTimer / WAG_DURATION;
            rotation = (float)Math.Sin(progress * Math.PI * 4) * WAG_INTENSITY * (1f - progress);
        }

        spriteBatch.Draw(
            iconTexture,
            worldPos,
            null,
            glowColor * opacity,
            rotation,
            new Vector2(iconTexture.Width / 2, iconTexture.Height / 2),
            scale,
            SpriteEffects.None,
            0f
        );

        if (ShowDebugHitbox)
        {
            DrawDebugHitbox(spriteBatch, worldPos, opacity);
        }

        if (IsHovering)
        {
            DrawPanel(spriteBatch, worldPos, opacity * hoverFadeIn);
        }
    }

    private void DrawDebugHitbox(SpriteBatch spriteBatch, Vector2 worldPos, float opacity)
    {
        var scaledWidth = Width;
        var scaledHeight = Height;

        var hitboxRect = new Rectangle(
            (int)worldPos.X - scaledWidth / 2,
            (int)worldPos.Y - scaledHeight / 2,
            scaledWidth,
            scaledHeight
        );

        var pixel = TextureAssets.MagicPixel.Value;
        var borderColor = IsHovering ? Color.Cyan : Color.Blue;
        borderColor *= opacity * 0.8f;

        // Draw border
        spriteBatch.Draw(pixel, new Rectangle(hitboxRect.X, hitboxRect.Y, hitboxRect.Width, 2), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(hitboxRect.X, hitboxRect.Bottom - 2, hitboxRect.Width, 2), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(hitboxRect.X, hitboxRect.Y, 2, hitboxRect.Height), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(hitboxRect.Right - 2, hitboxRect.Y, 2, hitboxRect.Height), borderColor);

        var fillColor = IsHovering ? Color.Cyan : Color.Blue;
        fillColor *= opacity * 0.2f;
        spriteBatch.Draw(pixel, hitboxRect, fillColor);
    }

    private void DrawPanel(SpriteBatch spriteBatch, Vector2 screenPos, float opacity)
    {
        if (npcData == null)
            return;

        float zoom = Main.GameViewMatrix.Zoom.X;

        // Calculate panel height based on content
        int lineCount = 2; // NPC name + preview text
        if (musicId.HasValue)
            lineCount++;

        int panelHeight = 20 + (lineCount * 22) + PADDING * 2;

        // Position panel to the right of the icon
        float panelX = screenPos.X + (Width * zoom / 2) + 15;
        float panelY = screenPos.Y - (panelHeight / 2);

        // Keep panel on screen
        if (panelX + PANEL_WIDTH > Main.screenWidth)
        {
            panelX = screenPos.X - (Width * zoom / 2) - PANEL_WIDTH - 15;
        }

        if (panelY + panelHeight > Main.screenHeight)
        {
            panelY = Main.screenHeight - panelHeight - 10;
        }

        if (panelY < 10)
        {
            panelY = 10;
        }

        Rectangle panelRect = new Rectangle(
            (int)panelX,
            (int)panelY,
            PANEL_WIDTH,
            panelHeight
        );

        // Draw semi-transparent background
        var pixel = TextureAssets.MagicPixel.Value;
        spriteBatch.Draw(pixel, panelRect, npcData.BoxColor * 0.3f * opacity);

        // Draw border
        var borderColor = npcData.BoxColor * opacity;
        spriteBatch.Draw(pixel, new Rectangle(panelRect.X, panelRect.Y, panelRect.Width, 2), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(panelRect.X, panelRect.Bottom - 2, panelRect.Width, 2), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(panelRect.X, panelRect.Y, 2, panelRect.Height), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(panelRect.Right - 2, panelRect.Y, 2, panelRect.Height), borderColor);

        int textY = panelRect.Y + PADDING;

        // NPC name
        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            npcData.NpcName,
            panelRect.X + PADDING,
            textY,
            npcData.BoxColor * opacity,
            Color.Black * opacity,
            Vector2.Zero,
            1f
        );
        textY += 25;

        // Try to get first dialogue line as preview
        string previewText = "Talk to start dialogue";
        try
        {
            var dialogue = DialogueBuilder.BuildByKey(dialogueKey, lineCount, defaultDelay, defaultEmote, musicId, modifications);
            if (dialogue.Entries.Count > 0)
            {
                var firstEntry = dialogue.Entries[0];
                var localizedText = firstEntry.GetText();
                if (localizedText != null)
                {
                    string fullText = localizedText.Value;
                    // Truncate if too long
                    if (fullText.Length > 80)
                    {
                        previewText = fullText.Substring(0, 77) + "...";
                    }
                    else
                    {
                        previewText = fullText;
                    }
                }
            }
        }
        catch
        {
            // Keep default preview text
        }

        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            previewText,
            panelRect.X + PADDING,
            textY,
            Color.White * opacity,
            Color.Black * opacity,
            Vector2.Zero,
            0.8f
        );
        textY += 25;

        // Music indicator if present
        if (musicId.HasValue)
        {
            Utils.DrawBorderStringFourWay(
                spriteBatch,
                FontAssets.MouseText.Value,
                "♪ With music",
                panelRect.X + PADDING,
                textY,
                Color.LightBlue * opacity,
                Color.Black * opacity,
                Vector2.Zero,
                0.8f
            );
        }

        Utils.DrawBorderStringFourWay(
            spriteBatch,
            FontAssets.MouseText.Value,
            "Click to start",
            panelRect.X + 4,
            panelRect.Y + panelHeight + 5,
            Color.Yellow * opacity,
            Color.Black * opacity,
            default,
            0.8f
        );
    }

    private void HandleClick()
    {
        try
        {
            bool success = DialogueManager.Instance.StartDialogue(npcData, dialogueKey, lineCount, zoomIn, defaultDelay, defaultEmote, musicId, false, modifications);

            if (success)
            {
                SoundEngine.PlaySound(npcData.TalkSFX);
                IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error starting dialogue: {ex.Message}");
        }
    }
}
/// <summary>
/// Manages the creation and tracking of dialogue indicators in the world
/// </summary>
public class DialogueIndicatorManager : ModSystem
{
    public static DialogueIndicatorManager Instance { get; set; }
    public DialogueIndicatorManager() => Instance = this;

    private readonly List<DialogueIndicator> indicators = [];
    private readonly Dictionary<int, DialogueIndicator> npcIndicators = [];
    private readonly Dictionary<int, string> npcDialogueTracking = [];

    public override void Unload()
    {
        if (!Main.dedServ)
        {
            Instance = null;
        }
        indicators.Clear();
        npcIndicators.Clear();
        npcDialogueTracking.Clear();
    }

    public override void OnWorldUnload()
    {
        base.OnWorldUnload();
        indicators.Clear();
        npcIndicators.Clear();
        npcDialogueTracking.Clear();
    }

    public DialogueIndicator CreateIndicator(Vector2 worldPosition, NPCData npcData, string dialogueKey, int lineCount,
        bool zoomIn = false, int defaultDelay = 2, int defaultEmote = 0, int? musicId = null,
        params (int line, int delay, int emote)[] modifications)
    {
        var indicator = new DialogueIndicator(worldPosition, npcData, dialogueKey, lineCount, zoomIn, defaultDelay, defaultEmote, musicId, modifications);
        indicators.Add(indicator);
        return indicator;
    }

    public DialogueIndicator CreateIndicatorForNPC(NPC npc, NPCData npcData, string dialogueKey, int lineCount,
        bool zoomIn = false, int defaultDelay = 2, int defaultEmote = 0, int? musicId = null,
        params (int line, int delay, int emote)[] modifications)
    {
        int npcIndex = npc.whoAmI;

        if (npcIndicators.ContainsKey(npcIndex))
        {
            if (npcDialogueTracking.ContainsKey(npcIndex) && npcDialogueTracking[npcIndex] != dialogueKey)
            {
                var oldIndicator = npcIndicators[npcIndex];
                indicators.Remove(oldIndicator);
                npcIndicators.Remove(npcIndex);

                var indicator = DialogueIndicator.CreateForNPC(npc, npcData, dialogueKey, lineCount, zoomIn, defaultDelay, defaultEmote, musicId, modifications);
                indicators.Add(indicator);
                npcIndicators[npcIndex] = indicator;
                npcDialogueTracking[npcIndex] = dialogueKey;
                return indicator;
            }

            return npcIndicators[npcIndex];
        }

        var newIndicator = DialogueIndicator.CreateForNPC(npc, npcData, dialogueKey, lineCount, zoomIn, defaultDelay, defaultEmote, musicId, modifications);
        indicators.Add(newIndicator);
        npcIndicators[npcIndex] = newIndicator;
        npcDialogueTracking[npcIndex] = dialogueKey;

        return newIndicator;
    }

    public void RemoveIndicatorForNPC(int npcIndex)
    {
        if (npcIndicators.TryGetValue(npcIndex, out DialogueIndicator indicator))
        {
            indicators.Remove(indicator);
            npcIndicators.Remove(npcIndex);

            if (npcDialogueTracking.ContainsKey(npcIndex))
            {
                npcDialogueTracking.Remove(npcIndex);
            }
        }
    }

    public void RemoveIndicatorsForDialogue(string dialogueKey)
    {
        var npcsToRemove = new List<int>();

        foreach (var kvp in npcDialogueTracking)
        {
            if (kvp.Value == dialogueKey)
            {
                npcsToRemove.Add(kvp.Key);
            }
        }

        foreach (var npcIndex in npcsToRemove)
        {
            RemoveIndicatorForNPC(npcIndex);
        }
    }

    public bool HasIndicatorForNPC(int npcIndex)
    {
        return npcIndicators.ContainsKey(npcIndex);
    }

    public string GetDialogueForNPC(int npcIndex)
    {
        return npcDialogueTracking.TryGetValue(npcIndex, out string dialogueKey) ? dialogueKey : null;
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
                        if (npcDialogueTracking.ContainsKey(kvp.Key))
                        {
                            npcDialogueTracking.Remove(kvp.Key);
                        }
                        break;
                    }
                }

                indicators.RemoveAt(i);
            }
        }
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int invasionIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Invasion Progress Bars"));
        if (invasionIndex != -1)
        {
            layers.Insert(invasionIndex + 1, new LegacyGameInterfaceLayer(
                "Reverie: Dialogue Indicators",
                delegate
                {
                    Instance.Draw(Main.spriteBatch);
                    return true;
                },
                InterfaceScaleType.Game)
            );
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var indicator in indicators)
        {
            indicator.Draw(spriteBatch);
        }
    }

    public void ClearAllIndicators()
    {
        indicators.Clear();
        npcIndicators.Clear();
        npcDialogueTracking.Clear();
    }
}