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

    private DialogueBox _activeDialogue = null;
    private int? _currentMusic = null;
    private DialogueID? _currentDialogue = null;
    private int _previousMusic = -1;

    private const float ZOOM_LEVEL = 1.55f;
    private const int ZOOM_TIME = 80;

    private bool _isZoomedIn = false;

    public bool IsUIHidden { get; private set; }

    private DialogueID? _nextDialogueId = null;
    private NPCData _nextNpcData = null;
    private bool _nextZoomIn = false;

    public void RegisterNPC(string npcId, NPCData npcData)
        => _npcDialogueData[npcId] = npcData;

    public DialogueBox GetActiveDialogue() => _activeDialogue;

    public NPCData GetNPCData(string npcId) => _npcDialogueData.TryGetValue(npcId, out var npcData) ? npcData : null;

    public bool StartDialogue(NPCData npcData, DialogueID dialogueId, bool zoomIn = false,
            DialogueID? nextDialogueId = null, NPCData nextNpcData = null, bool nextZoomIn = false)
    {
        Main.CloseNPCChatOrSign();
        Letterbox.Show();
        IsUIHidden = true;
        if (IsAnyActive())
        {
            return false;
        }

        var dialogue = DialogueList.GetDialogueById(dialogueId);
        if (dialogue != null)
        {
            _activeDialogue = DialogueBox.CreateNewSequence(npcData, dialogue, zoomIn);
            ChangeMusic(dialogue.MusicID);
            _currentDialogue = dialogueId;

            // Store next dialogue information
            _nextDialogueId = nextDialogueId;
            _nextNpcData = nextNpcData;
            _nextZoomIn = nextZoomIn;

            return true;
        }

        return false;
    }

    private void EndDialogue()
    {
        Letterbox.Hide();
        IsUIHidden = false;

        if (_currentDialogue.HasValue)
        {
            _currentDialogue = null;
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
        if (_nextDialogueId.HasValue && _nextNpcData != null)
        {
            var nextDialogueId = _nextDialogueId.Value;
            var nextNpcData = _nextNpcData;
            var nextZoomIn = _nextZoomIn;

            // Clear the next dialogue data before starting new dialogue
            _nextDialogueId = null;
            _nextNpcData = null;
            _nextZoomIn = false;

            // Start the next dialogue
            StartDialogue(nextNpcData, nextDialogueId, nextZoomIn);
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

    public bool IsDialogueActive(DialogueID dialogueId)
        => _activeDialogue != null && DialogueList.GetDialogueById(dialogueId) != null;

    public bool EntryIsActive(DialogueID dialogueId, int entryIndex)
    {
        return _activeDialogue != null &&
               DialogueList.GetDialogueById(dialogueId) != null &&
               _activeDialogue.IsCurrentEntry(entryIndex);
    }

    public bool IsAnyActive() => _activeDialogue != null;
}