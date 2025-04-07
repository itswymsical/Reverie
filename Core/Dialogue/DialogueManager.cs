using Reverie.Common.Systems;
using Reverie.Common.UI.Dialogue;
using Reverie.Core.Cinematics;
using System.Collections.Generic;

namespace Reverie.Core.Dialogue;

public sealed class DialogueManager
{
    private static readonly DialogueManager instance;
    public static DialogueManager Instance => instance;

    static DialogueManager()
    {
        instance = new DialogueManager();
    }

    private DialogueManager() { }

    private readonly Dictionary<string, NPCData> _npcDialogueData = [];

    // Cache for dialogues to avoid rebuilding frequently used ones
    private readonly Dictionary<string, DialogueSequence> _dialogueCache = [];

    private DialogueBox _activeDialogue = null;
    private int? _currentMusic = null;
    private string _currentDialogueKey = null;
    private int _previousMusic = -1;

    private const float ZOOM_LEVEL = 1.55f;
    private const int ZOOM_TIME = 80;

    private bool _isZoomedIn = false;

    public bool IsUIHidden { get; private set; }

    private string _nextDialogueKey = null;
    private NPCData _nextNpcData = null;
    private bool _nextZoomIn = false;

    public void RegisterNPC(string npcId, NPCData npcData)
        => _npcDialogueData[npcId] = npcData;

    public DialogueBox GetActiveDialogue() => _activeDialogue;

    public NPCData GetNPCData(string npcId) => _npcDialogueData.TryGetValue(npcId, out var npcData) ? npcData : null;

    /// <summary>
    /// Start a dialogue directly using a DialogueSequence
    /// </summary>
    public bool StartDialogue(NPCData npcData, DialogueSequence dialogue, string dialogueKey, bool zoomIn = false,
            string nextDialogueKey = null, NPCData nextNpcData = null, bool nextZoomIn = false)
    {
        Main.CloseNPCChatOrSign();
        //Letterbox.Show();
        IsUIHidden = true;
        if (IsAnyActive())
        {
            return false;
        }

        if (dialogue != null)
        {
            _activeDialogue = DialogueBox.CreateNewSequence(npcData, dialogue, zoomIn);
            ChangeMusic(dialogue.MusicID);
            _currentDialogueKey = dialogueKey;

            // Store next dialogue information
            _nextDialogueKey = nextDialogueKey;
            _nextNpcData = nextNpcData;
            _nextZoomIn = nextZoomIn;

            return true;
        }

        return false;
    }

    /// <summary>
    /// Start a dialogue by its key, building it on demand
    /// </summary>
    public bool StartDialogueByKey(NPCData npcData, string dialogueKey, int lineCount, bool zoomIn = false,
            string nextDialogueKey = null, NPCData nextNpcData = null, bool nextZoomIn = false,
            int defaultDelay = 2, int defaultEmote = 0, int? musicId = null,
             bool letterbox = false, params(int line, int delay, int emote)[] modifications)
    {
        DialogueSequence dialogue;
        if (letterbox)
        {
            Letterbox.Show();
        }
        // Check cache first
        if (!_dialogueCache.TryGetValue(dialogueKey, out dialogue))
        {
            dialogue = DialogueBuilder.BuildByKey(dialogueKey, lineCount, defaultDelay, defaultEmote, musicId, modifications);

            // Cache the dialogue for future use
            _dialogueCache[dialogueKey] = dialogue;
        }

        return StartDialogue(npcData, dialogue, dialogueKey, zoomIn, nextDialogueKey, nextNpcData, nextZoomIn);
    }

    /// <summary>
    /// Start a simple one-line dialogue
    /// </summary>
    public bool StartSimpleDialogue(NPCData npcData, string dialogueKey, bool zoomIn = false,
            int delay = 2, int emote = 0, int? musicId = null)
    {
        DialogueSequence dialogue;

        // Check cache first
        if (!_dialogueCache.TryGetValue(dialogueKey, out dialogue))
        {
            dialogue = DialogueBuilder.SimpleLineByKey(dialogueKey, delay, emote, musicId);

            // Cache the dialogue for future use
            _dialogueCache[dialogueKey] = dialogue;
        }

        return StartDialogue(npcData, dialogue, dialogueKey, zoomIn);
    }

    /// <summary>
    /// Clears the dialogue cache to free up memory
    /// </summary>
    public void ClearDialogueCache()
    {
        _dialogueCache.Clear();
    }

    private void EndDialogue()
    {
        Letterbox.Hide();
        IsUIHidden = false;

        if (_currentDialogueKey != null)
        {
            _currentDialogueKey = null;
        }

        if (_currentMusic.HasValue)
        {
            Main.musicBox2 = _previousMusic;
            _currentMusic = null;
        }

        if (_isZoomedIn)
        {
            ZoomHandler.SetZoomAnimation(1f, ZOOM_TIME);
            _isZoomedIn = false;
        }

        _activeDialogue = null;

        // Check if we have a next dialogue to start
        if (_nextDialogueKey != null && _nextNpcData != null)
        {
            var nextDialogueKey = _nextDialogueKey;
            var nextNpcData = _nextNpcData;
            var nextZoomIn = _nextZoomIn;

            // Clear the next dialogue data before starting new dialogue
            _nextDialogueKey = null;
            _nextNpcData = null;
            _nextZoomIn = false;

            // Get the cached dialogue if available
            if (_dialogueCache.TryGetValue(nextDialogueKey, out var nextDialogue))
            {
                StartDialogue(nextNpcData, nextDialogue, nextDialogueKey, nextZoomIn);
            }
            // If not in cache, we can't start it automatically - the caller must provide the line count
        }
    }

    public void UpdateActive()
    {
        if (_activeDialogue != null)
        {
            if (_activeDialogue.ShouldBeRemoved)
            {
                EndDialogue();
            }
            else
            {
                Letterbox.Update();

                _activeDialogue.Update();
                if (_currentMusic.HasValue)
                {
                    Main.musicBox2 = _currentMusic.Value;
                }
            }
        }

        UpdateZoom();
    }

    private void UpdateZoom()
    {
        if (_activeDialogue != null && !_isZoomedIn && _activeDialogue.ShouldZoom)
        {
            ZoomHandler.SetZoomAnimation(ZOOM_LEVEL, ZOOM_TIME);
            _isZoomedIn = true;
        }
        else if (_activeDialogue == null && _isZoomedIn)
        {
            ZoomHandler.SetZoomAnimation(1f, ZOOM_TIME);
            _isZoomedIn = false;
        }
    }

    private void ChangeMusic(int? musicID)
    {
        if (musicID.HasValue)
        {
            _previousMusic = Main.musicBox2;
            _currentMusic = musicID.Value;
            Main.musicBox2 = musicID.Value;
        }
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 bottomAnchorPosition)
    {
        if (_activeDialogue == null) return;

        Letterbox.Draw(spriteBatch);
        Vector2 adjustedPosition = bottomAnchorPosition;
        adjustedPosition.Y -= Letterbox.LetterboxHeight;

        _activeDialogue.DrawInGame(spriteBatch, adjustedPosition);
    }

    public bool IsDialogueActive(string dialogueKey)
        => _activeDialogue != null && _currentDialogueKey == dialogueKey;

    public bool EntryIsActive(string dialogueKey, int entryIndex)
    {
        return _activeDialogue != null &&
               _currentDialogueKey == dialogueKey &&
               _activeDialogue.IsCurrentEntry(entryIndex);
    }

    public bool IsAnyActive() => _activeDialogue != null;
}