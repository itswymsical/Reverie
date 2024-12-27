using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Reverie.Common.Players;
using Reverie.Common.Systems;
using Reverie.Common.UI;
using System.Collections.Generic;
using Terraria;
using Terraria.Localization;

namespace Reverie.Core.Dialogue
{
    public sealed class DialogueManager
    {
        private static readonly DialogueManager _instance = new();

        private DialogueManager() { }

        public static DialogueManager Instance => _instance;

        private readonly Dictionary<string, NPCData> _npcDialogueData = [];

        private NPCDialogueBox _activeDialogue = null;
        private int? _currentDialogueMusicID = null;
        private DialogueID? _currentDialogueId = null;
        private int _previousMusicBox = -1;

        public void RegisterNPC(string npcId, NPCData npcData)
            => _npcDialogueData[npcId] = npcData;

        public NPCDialogueBox GetActiveDialogue() 
            => _activeDialogue;
        public NPCData GetNPCData(string npcId)
        => _npcDialogueData.TryGetValue(npcId, out var npcData) ? npcData : null;

        public bool PlayDialogueSequence(NPCData npcData, DialogueID dialogueId)
        {
            Main.CloseNPCChatOrSign();
            if (IsAnyDialogueActive())
            {
                return false;
            }

            var dialogue = DialogueList.GetDialogueById(dialogueId);
            if (dialogue != null)
            {
                _activeDialogue = NPCDialogueBox.CreateNewDialogueSequence(npcData, dialogue);
                ChangeMusicForDialogue(dialogue.MusicID);
                _currentDialogueId = dialogueId;
                return true;
            }

            return false;
        }

        private void ChangeMusicForDialogue(int? musicID)
        {
            if (musicID.HasValue)
            {
                _previousMusicBox = Main.musicBox2;
                _currentDialogueMusicID = musicID.Value;
                Main.musicBox2 = musicID.Value;
            }
        }

        public void UpdateActiveDialogue()
        {
            if (_activeDialogue != null)
            {
                if (_activeDialogue.ShouldBeRemoved)
                {
                    EndDialogue();
                }
                else
                {
                    _activeDialogue.Update();
                    EnsureDialogueMusicPlaying();
                }
            }
        }

        private void EnsureDialogueMusicPlaying()
        {
            if (_currentDialogueMusicID.HasValue)
            {
                Main.musicBox2 = _currentDialogueMusicID.Value;
            }
        }
        public void OnEndDialogue(DialogueID dialogueId)
        {
            ReveriePlayer player = Main.LocalPlayer.GetModPlayer<ReveriePlayer>();
            if (dialogueId == DialogueID.GuideYappingAboutReverieLore)
                ReverieUISystem.Instance.ClassInterface.SetState(ReverieUISystem.Instance.classUI);
        }
        private void EndDialogue()
        {
            if (_currentDialogueId.HasValue)
            {
                OnEndDialogue(_currentDialogueId.Value);
                _currentDialogueId = null;
            }

            if (_currentDialogueMusicID.HasValue)
            {
                Main.musicBox2 = _previousMusicBox;
                _currentDialogueMusicID = null;
            }

            _activeDialogue = null;
        }    

        public void DrawActiveDialogue(SpriteBatch spriteBatch, Vector2 bottomAnchorPosition) 
            => _activeDialogue?.DrawInGame(spriteBatch, bottomAnchorPosition);
        
        public bool IsDialogueActive(DialogueID dialogueId)
            => _activeDialogue != null && DialogueList.GetDialogueById(dialogueId) != null;
        
        public bool IsDialogueEntryActive(DialogueID dialogueId, int entryIndex)
        {
            return _activeDialogue != null &&
                   DialogueList.GetDialogueById(dialogueId) != null &&
                   _activeDialogue.IsCurrentEntry(entryIndex);
        }

        public bool IsAnyDialogueActive() => _activeDialogue != null;
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

    public enum DialogueType
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
