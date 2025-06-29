using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;

namespace Reverie.Core.Dialogue;

public sealed class DialogueManager
{
    private static readonly DialogueManager instance = new();
    public static DialogueManager Instance => instance;

    private readonly Dictionary<string, List<DialogueData>> cache = new();
    private DialogueBox activeDialogue = null;
    private int? currentMusic = null;
    private int previousMusic = -1;
    private bool isZoomedIn = false;

    private const float ZOOM_LEVEL = 1.55f;
    private const int ZOOM_TIME = 80;

    public bool IsUIHidden { get; private set; }

    /// <summary>
    /// Start a dialogue sequence using the simplified DialogueBuilder approach
    /// </summary>
    public bool StartDialogue(string dialogueKey, bool zoomIn = false, bool letterbox = true)
    {
        Main.NewText($"[DEBUG] Starting dialogue: {dialogueKey}", Color.Cyan);

        if (IsAnyActive())
        {
            Main.NewText("[DEBUG] Dialogue already active", Color.Yellow);
            return false;
        }

        // Get or build dialogue using DialogueBuilder
        if (!cache.TryGetValue(dialogueKey, out var dialogueLines))
        {
            Main.NewText("[DEBUG] Building dialogue from localization", Color.Cyan);
            dialogueLines = DialogueBuilder.BuildSequenceFromLocalization(dialogueKey);

            if (dialogueLines == null || dialogueLines.Count == 0)
            {
                Main.NewText($"[ERROR] No dialogue found for key: {dialogueKey}", Color.Red);
                return false;
            }

            cache[dialogueKey] = dialogueLines;
            Main.NewText($"[DEBUG] Built {dialogueLines.Count} dialogue lines", Color.Green);
        }

        // Setup UI state
        Main.CloseNPCChatOrSign();
        if (letterbox)
        {
            try
            {
                // Letterbox.Show(); // If you have this system
            }
            catch { /* Ignore if letterbox system missing */ }
        }
        IsUIHidden = letterbox;

        // Create dialogue box with DialogueData list
        activeDialogue = DialogueBox.Create(dialogueLines, zoomIn);
        if (activeDialogue == null)
        {
            Main.NewText("[ERROR] Failed to create dialogue box", Color.Red);
            return false;
        }

        // Setup music (optional - check first line for music info)
        if (dialogueLines.Count > 0)
        {
            // You could add music support to DialogueData if needed
            // var firstLine = dialogueLines[0];
            // if (firstLine.MusicID.HasValue) { ... }
        }

        Main.NewText("[DEBUG] Dialogue started successfully", Color.Green);
        return true;
    }

    public void Update()
    {
        if (activeDialogue != null)
        {
            if (activeDialogue.ShouldBeRemoved)
            {
                EndDialogue();
            }
            else
            {
                activeDialogue.Update();
                UpdateZoom();

                if (currentMusic.HasValue)
                    Main.musicBox2 = currentMusic.Value;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 bottomAnchorPosition)
    {
        if (activeDialogue != null)
        {
            Vector2 adjustedPosition = bottomAnchorPosition;
            // Adjust for letterbox if needed
            // adjustedPosition.Y -= Letterbox.LetterboxHeight;

            activeDialogue.DrawInGame(spriteBatch, adjustedPosition);
        }
    }

    private void UpdateZoom()
    {
        if (activeDialogue != null && !isZoomedIn && activeDialogue.ShouldZoom)
        {
            try
            {
                // ZoomHandler.SetZoomAnimation(ZOOM_LEVEL, ZOOM_TIME);
                isZoomedIn = true;
            }
            catch { /* Ignore if zoom system missing */ }
        }
        else if (activeDialogue == null && isZoomedIn)
        {
            try
            {
                // ZoomHandler.SetZoomAnimation(1f, ZOOM_TIME);
                isZoomedIn = false;
            }
            catch { /* Ignore if zoom system missing */ }
        }
    }

    private void EndDialogue()
    {
        try
        {
            // Letterbox.Hide();
        }
        catch { /* Ignore if letterbox system missing */ }

        IsUIHidden = false;

        if (currentMusic.HasValue)
        {
            Main.musicBox2 = previousMusic;
            currentMusic = null;
        }

        if (isZoomedIn)
        {
            try
            {
                // ZoomHandler.SetZoomAnimation(1f, ZOOM_TIME);
                isZoomedIn = false;
            }
            catch { /* Ignore if zoom system missing */ }
        }

        activeDialogue = null;
        Main.NewText("[DEBUG] Dialogue ended", Color.Gray);
    }

    public bool IsAnyActive() => activeDialogue != null;

    public void ClearCache() => cache.Clear();

    /// <summary>
    /// Test method to start a single dialogue line
    /// </summary>
    public bool StartSingleLine(string dialogueKey, string lineKey, bool zoomIn = false)
    {
        Main.NewText($"[DEBUG] Starting single line: {dialogueKey}.{lineKey}", Color.Cyan);

        if (IsAnyActive())
            return false;

        var dialogueData = DialogueBuilder.BuildLineFromLocalization(dialogueKey, lineKey);
        if (dialogueData == null)
        {
            Main.NewText($"[ERROR] Failed to build line: {dialogueKey}.{lineKey}", Color.Red);
            return false;
        }

        // Create single-line dialogue
        var singleLineList = new List<DialogueData> { dialogueData };
        activeDialogue = DialogueBox.Create(singleLineList, zoomIn);

        if (activeDialogue == null)
        {
            Main.NewText("[ERROR] Failed to create single-line dialogue", Color.Red);
            return false;
        }

        Main.NewText("[DEBUG] Single line dialogue started", Color.Green);
        return true;
    }
}