using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Reverie.Common.UI;
using Reverie.Core.Graphics;

using Terraria;
using Terraria.Localization;

namespace Reverie.Core.Dialogue
{
    public sealed class DialogueManager
    {
        private static readonly DialogueManager _instance = new();
        public static DialogueManager Instance => _instance;
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

        public void RegisterNPC(string npcId, NPCData npcData)
            => _npcDialogueData[npcId] = npcData;

        public DialogueBox GetActiveDialogue() => _activeDialogue;

        public NPCData GetNPCData(string npcId) => _npcDialogueData.TryGetValue(npcId, out var npcData) ? npcData : null;

        public bool StartDialogue(NPCData npcData, DialogueID dialogueId, bool zoomIn = false)
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

    public class DialogueEntry(string localizationKey, int delay, int emoteFrame, Color? entryTextColor = null, NPCData speakingNPC = null)
    {
        public string LocalizationKey { get; } = localizationKey;
        public int Delay { get; } = delay;
        public int EmoteFrame { get; } = emoteFrame;
        public Color? EntryTextColor { get; } = entryTextColor;
        public NPCData SpeakingNPC { get; } = speakingNPC;

        public LocalizedText GetText() => Reverie.Instance.GetLocalization(LocalizationKey);
    }

    public enum DialogueType //useful later
    {
        Mission,
        Banter,
        ProgressionEvent,
    }

    public class DialogueSequence(DialogueType type, IEnumerable<DialogueEntry> entries, int? musicId = null)
    {
        public DialogueType Type { get; } = type;
        public IReadOnlyList<DialogueEntry> Entries { get; } = new List<DialogueEntry>(entries);
        public int? MusicID { get; } = musicId;
    }
}