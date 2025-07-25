using Reverie.Core.Cinematics;
using Reverie.Core.Cinematics.Camera;
using System.Collections.Generic;
using System.Linq;

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

    private readonly Queue<QueuedDialogue> dialogueQueue = new();

    private const float ZOOM_LEVEL = 1.55f;
    private const int ZOOM_TIME = 80;

    public bool IsUIHidden { get; private set; }

    public delegate void DialogueEndHandler(string dialogueKey);
    public static event DialogueEndHandler OnDialogueEnd;

    public bool StartDialogue(string dialogueKey, int lineCount, bool zoomIn = false, bool letterbox = true, int? music = null)
    {
        var queuedDialogue = new QueuedDialogue
        {
            DialogueKey = dialogueKey,
            LineCount = lineCount,
            ZoomIn = zoomIn,
            Letterbox = letterbox,
            Music = music
        };

        // If no dialogue is active, start immediately
        if (!IsAnyActive())
        {
            return StartDialogueInternal(queuedDialogue);
        }
        else
        {
            dialogueQueue.Enqueue(queuedDialogue);
            return true;
        }
    }

    private bool StartDialogueInternal(QueuedDialogue queuedDialogue)
    {
        var lineKeys = new List<string>();
        for (int i = 1; i <= queuedDialogue.LineCount; i++)
        {
            lineKeys.Add($"Line{i}");
        }

        Main.CloseNPCChatOrSign();

        if (queuedDialogue.Letterbox)
        {
            Letterbox.Show();
        }
        IsUIHidden = queuedDialogue.Letterbox;

        activeDialogue = DialogueBox.CreateDialogue(queuedDialogue.DialogueKey, lineKeys, queuedDialogue.ZoomIn);
        if (activeDialogue == null)
        {
            if (queuedDialogue.Letterbox)
            {
                Letterbox.Hide();
                IsUIHidden = false;
            }
            return false;
        }

        if (queuedDialogue.Music.HasValue)
        {
            previousMusic = Main.musicBox2;
            currentMusic = queuedDialogue.Music.Value;
            Main.musicBox2 = queuedDialogue.Music.Value;
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
        string completedDialogueKey = activeDialogue?.DialogueKey;

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

        // Fire the event after clearing activeDialogue
        if (!string.IsNullOrEmpty(completedDialogueKey))
        {
            OnDialogueEnd?.Invoke(completedDialogueKey);
        }

        // Check if there's a queued dialogue to start
        if (dialogueQueue.Count > 0)
        {
            var nextDialogue = dialogueQueue.Dequeue();
            StartDialogueInternal(nextDialogue);
        }
    }

    /// <summary>
    /// Gets the key of the currently active dialogue, or null if none
    /// </summary>
    public string GetActiveDialogueKey()
    {
        return activeDialogue?.DialogueKey;
    }

    /// <summary>
    /// Checks if a specific dialogue key is currently active
    /// </summary>
    public bool IsDialogueActive(string dialogueKey)
    {
        return activeDialogue?.DialogueKey == dialogueKey;
    }

    /// <summary>
    /// Gets the number of dialogues currently in queue
    /// </summary>
    public int GetQueuedDialogueCount()
    {
        return dialogueQueue.Count;
    }

    /// <summary>
    /// Checks if a dialogue is queued (not currently active)
    /// </summary>
    public bool IsDialogueQueued(string dialogueKey)
    {
        return dialogueQueue.Any(d => d.DialogueKey == dialogueKey);
    }

    /// <summary>
    /// Clears all queued dialogues (keeps current active dialogue)
    /// </summary>
    public void ClearQueue()
    {
        dialogueQueue.Clear();
    }

    /// <summary>
    /// Force ends current dialogue and clears queue
    /// </summary>
    public void ForceEndAll()
    {
        dialogueQueue.Clear();
        if (activeDialogue != null)
        {
            activeDialogue.Close();
        }
    }

    public bool IsAnyActive() => activeDialogue != null;
    public void ClearCache() => cachedLineKeys.Clear();
}

public class QueuedDialogue
{
    public string DialogueKey { get; set; }
    public int LineCount { get; set; }
    public bool ZoomIn { get; set; }
    public bool Letterbox { get; set; }
    public int? Music { get; set; }
}