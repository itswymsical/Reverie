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

    public void RegisterNPC(string npcId, NPCData npcData)
        => _npcDialogueData[npcId] = npcData;

    public DialogueBox GetActiveDialogue() => _activeDialogue;

    public NPCData GetNPCData(string npcId) => _npcDialogueData.TryGetValue(npcId, out var npcData) ? npcData : null;

    /// <summary>
    /// Starts a dialogue directly using a <seealso cref="DialogueSequence"/>.
    /// NOTE: Its better to use the overloaded method with a localization key.
    /// </summary>
    public bool StartDialogue(NPCData npcData, DialogueSequence dialogue, string dialogueKey, bool zoomIn = false,
            string nextDialogueKey = null, NPCData nextNpcData = null, bool nextZoomIn = false)
    {
        Main.LocalPlayer.SetTalkNPC(-1);
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

            return true;
        }

        return false;
    }

    /// <summary>
    /// Starts a dialogue by localization key.
    /// </summary>
    public bool StartDialogue(NPCData npcData, string dialogueKey, int lineCount, bool zoomIn = false, int defaultDelay = 2, int defaultEmote = 0, int? musicId = null,
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

        return StartDialogue(npcData, dialogue, dialogueKey, zoomIn);
    }

    /// <summary>
    /// Clears the dialogue cache to free up memory.
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

        Letterbox.DrawCinematic(spriteBatch);
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