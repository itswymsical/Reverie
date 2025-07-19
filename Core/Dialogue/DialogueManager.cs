using Reverie.Core.Cinematics;
using Reverie.Core.Cinematics.Camera;
using System.Collections.Generic;

namespace Reverie.Core.Dialogue;

public sealed class DialogueManager
{
    private static readonly DialogueManager instance = new();
    public static DialogueManager Instance => instance;

    private readonly Dictionary<string, List<string>> cachedLineKeys = new();
    private DialogueBox activeDialogue = null;
    private int? currentMusic = null;
    private int previousMusic = -1;
    private bool isZoomedIn = false;

    private const float ZOOM_LEVEL = 1.55f;
    private const int ZOOM_TIME = 80;

    public bool IsUIHidden { get; private set; }

    public bool StartDialogue(string dialogueKey, int lineCount, bool zoomIn = false, bool letterbox = true, int? music = null)
    {
        if (IsAnyActive())
            return false;

        var lineKeys = new List<string>();
        for (int i = 1; i <= lineCount; i++)
        {
            lineKeys.Add($"Line{i}");
        }

        Main.CloseNPCChatOrSign();

        if (letterbox)
        {
            Letterbox.Show();
        }
        IsUIHidden = letterbox;

        activeDialogue = DialogueBox.CreateDialogue(dialogueKey, lineKeys, zoomIn);
        if (activeDialogue == null)
        {
            if (letterbox)
            {
                Letterbox.Hide();
                IsUIHidden = false;
            }
            return false;
        }

        if (music.HasValue)
        {
            previousMusic = Main.musicBox2;
            currentMusic = music.Value;
            Main.musicBox2 = music.Value;
        }

        return true;
    }

    public void Update()
    {
        Letterbox.Update();

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
        Letterbox.Draw(spriteBatch, 0.05f);

        if (activeDialogue != null)
        {
            Vector2 adjustedPosition = bottomAnchorPosition;
            activeDialogue.DrawInGame(spriteBatch, adjustedPosition);
        }
    }

    private void UpdateZoom()
    {
        if (activeDialogue != null && !isZoomedIn && activeDialogue.ShouldZoom)
        {
            ZoomHandler.SetZoomAnimation(ZOOM_LEVEL, ZOOM_TIME);
            isZoomedIn = true;
        }
        else if (activeDialogue == null && isZoomedIn)
        {
            ZoomHandler.SetZoomAnimation(1f, ZOOM_TIME);
            isZoomedIn = false;
        }
    }

    private void EndDialogue()
    {
        Letterbox.Hide();
        IsUIHidden = false;

        if (currentMusic.HasValue)
        {
            Main.musicBox2 = previousMusic;
            currentMusic = null;
        }

        if (isZoomedIn)
        {
            ZoomHandler.SetZoomAnimation(1f, ZOOM_TIME);
            isZoomedIn = false;
        }

        activeDialogue = null;
    }

    public bool IsAnyActive() => activeDialogue != null;
    public void ClearCache() => cachedLineKeys.Clear();
}