using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Core.Dialogue
{
    public enum DialogueID
    {
        Mission_01_WakingUp,
        Mission_01_Briefing,
        Mission_01_TrainingComplete,
        Mission_01_MagicMirror,
        Mission_01_Outro,

        SelectedClass_Warrior,
        SelectedClass_Marksman,
        SelectedClass_Mage,
        SelectedClass_Conjurer,

        Mission_04_StumpyIntro
    }
    public static class DialogueList
    {
        private static Dictionary<DialogueID, DialogueSequence> _dialogues;
        static DialogueList() => Initialize();     

        private static void Initialize()
        {
            _dialogues = new Dictionary<DialogueID, DialogueSequence>
            {
                [DialogueID.Mission_01_WakingUp] = new(
                DialogueType.ProgressionEvent,
                [
                    new("DialogueLibrary.Mission_01.WakingUp.Line1", 1, 0),
                    new("DialogueLibrary.Mission_01.WakingUp.Line2", 2, 0),
                    new("DialogueLibrary.Mission_01.WakingUp.Line3", 2, 0),
                    new("DialogueLibrary.Mission_01.WakingUp.Line4", 2, 0),
                    new("DialogueLibrary.Mission_01.WakingUp.Line5", 2, 0),
                    new("DialogueLibrary.Mission_01.WakingUp.Line6", 2, 0),
                    new("DialogueLibrary.Mission_01.WakingUp.Line7", 2, 0, speakingNPC: NPCDataManager.Default),
                    new("DialogueLibrary.Mission_01.WakingUp.Line8", 10, 0),
                    new("DialogueLibrary.Mission_01.WakingUp.Line9", 2, 0),
                    new("DialogueLibrary.Mission_01.WakingUp.Line10", 2, 0),
                    new("DialogueLibrary.Mission_01.WakingUp.Line11", 2, 0)
                ], MusicID.OtherworldlyDay),

                [DialogueID.Mission_01_Briefing] = new(
                DialogueType.ProgressionEvent,
                [
                    new("DialogueLibrary.Mission_01.Briefing.Line1", 2, 0),
                    new("DialogueLibrary.Mission_01.Briefing.Line2", 2, 0),
                    new("DialogueLibrary.Mission_01.Briefing.Line3", 2, 0),
                    new("DialogueLibrary.Mission_01.Briefing.Line4", 2, 0),
                    new("DialogueLibrary.Mission_01.Briefing.Line5", 2, 0),
                    new("DialogueLibrary.Mission_01.Briefing.Line6", 2, 0),
                    new("DialogueLibrary.Mission_01.Briefing.Line7", 4, 0),
                    new("DialogueLibrary.Mission_01.Briefing.Line8", 2, 0),
                    new("DialogueLibrary.Mission_01.Briefing.Line9", 2, 0),
                    new("DialogueLibrary.Mission_01.Briefing.Line10", 2, 0),
                    new("DialogueLibrary.Mission_01.Briefing.Line11", 2, 0),
                    new("DialogueLibrary.Mission_01.Briefing.Line12", 2, 0),
                    new("DialogueLibrary.Mission_01.Briefing.Line13", 2, 0),
                    new("DialogueLibrary.Mission_01.Briefing.Line14", 2, 0),
                    new("DialogueLibrary.Mission_01.Briefing.Line15", 2, 0),
                    new("DialogueLibrary.Mission_01.Briefing.Line16", 2, 0),
                    new("DialogueLibrary.Mission_01.Briefing.Line17", 2, 0),
                    new("DialogueLibrary.Mission_01.Briefing.Line18", 2, 0)
                ], MusicID.OtherworldlyDay),

                [DialogueID.Mission_01_TrainingComplete] = new(
                DialogueType.ProgressionEvent,
                [
                    new("DialogueLibrary.Mission_01.TrainingComplete.Line1", 2, 0),
                    new("DialogueLibrary.Mission_01.TrainingComplete.Line2", 2, 0),
                    new("DialogueLibrary.Mission_01.TrainingComplete.Line3", 2, 0),
                ]),

                [DialogueID.Mission_01_MagicMirror] = new(
                DialogueType.ProgressionEvent,
                [
                    new("DialogueLibrary.Mission_01.MagicMirror.Line1", 2, 0),
                    new("DialogueLibrary.Mission_01.MagicMirror.Line2", 2, 0),
                    new("DialogueLibrary.Mission_01.MagicMirror.Line3", 1,  0, Color.Yellow),
                    new("DialogueLibrary.Mission_01.MagicMirror.Line4", 2, 0),
                    new("DialogueLibrary.Mission_01.MagicMirror.Line5", 2, 0),
                    new("DialogueLibrary.Mission_01.MagicMirror.Line6", 2, 0),
                    new("DialogueLibrary.Mission_01.MagicMirror.Line7", 2, 0),
                ]),

                [DialogueID.Mission_01_Outro] = new(
                DialogueType.ProgressionEvent,
                [
                    new("DialogueLibrary.Mission_01.Outro.Line1", 2, 0),
                    new("DialogueLibrary.Mission_01.Outro.Line2", 2, 0),
                    new("DialogueLibrary.Mission_01.Outro.Line3", 2, 0),
                    new("DialogueLibrary.Mission_01.Outro.Line4", 2, 0),
                    new("DialogueLibrary.Mission_01.Outro.Line5", 2, 0)
                ]),

                #region Class Selection Dialogue
                [DialogueID.SelectedClass_Marksman] = new(
                DialogueType.ProgressionEvent,
                [
                    new("DialogueLibrary.Mission_01.SelectedClass.Marksman.Line1", 3, 0),
                    new("DialogueLibrary.Mission_01.SelectedClass.Marksman.Line2", 3, 0),
                    new("DialogueLibrary.Mission_01.SelectedClass.Marksman.Line3", 3, 0),
                ]),

                [DialogueID.SelectedClass_Warrior] = new(
                DialogueType.ProgressionEvent,
                [
                    new("DialogueLibrary.Mission_01.SelectedClass.Warrior.Line1", 3, 0),
                    new("DialogueLibrary.Mission_01.SelectedClass.Warrior.Line2", 3, 0),
                    new("DialogueLibrary.Mission_01.SelectedClass.Warrior.Line3", 3, 0),
                ]),

                [DialogueID.SelectedClass_Mage] = new(
                DialogueType.ProgressionEvent,
                [
                    new("DialogueLibrary.Mission_01.SelectedClass.Mage.Line1", 3, 0),
                    new("DialogueLibrary.Mission_01.SelectedClass.Mage.Line2", 3, 0),
                    new("DialogueLibrary.Mission_01.SelectedClass.Mage.Line3", 3, 0),
                ]),

                [DialogueID.SelectedClass_Conjurer] = new(
                DialogueType.ProgressionEvent,
                [
                    new("DialogueLibrary.Mission_01.SelectedClass.Conjurer.Line1", 3, 0),
                    new("DialogueLibrary.Mission_01.SelectedClass.Conjurer.Line2", 3, 0),
                    new("DialogueLibrary.Mission_01.SelectedClass.Conjurer.Line3", 3, 0),
                ]),
                #endregion

                #region SIDE MISSION

                [DialogueID.Mission_04_StumpyIntro] = new(
                DialogueType.Mission,
                [
                    new("DialogueLibrary.Mission_04.StumpyIntro.Line1", 3, 0),
                    new("DialogueLibrary.Mission_04.StumpyIntro.Line2", 3, 0),
                    new("DialogueLibrary.Mission_04.StumpyIntro.Line3", 3, 0),
                    new("DialogueLibrary.Mission_04.StumpyIntro.Line4", 3, 0),
                    new("DialogueLibrary.Mission_04.StumpyIntro.Line5", 3, 0),
                    new("DialogueLibrary.Mission_04.StumpyIntro.Line6", 2, 0),
                    new("DialogueLibrary.Mission_04.StumpyIntro.Line7", 3, 0),
                    new("DialogueLibrary.Mission_04.StumpyIntro.Line8", 3, 0),
                    new("DialogueLibrary.Mission_04.StumpyIntro.Line9", 3, 0),
                    new("DialogueLibrary.Mission_04.StumpyIntro.Line10", 3, 0),
                    new("DialogueLibrary.Mission_04.StumpyIntro.Line11", 3, 0),
                    new("DialogueLibrary.Mission_04.StumpyIntro.Line12", 5, 0),
                    new("DialogueLibrary.Mission_04.StumpyIntro.Line13", 4, 0),
                    new("DialogueLibrary.Mission_04.StumpyIntro.Line14", 3, 0),
                    new("DialogueLibrary.Mission_04.StumpyIntro.Line15", 3, 0),
                    new("DialogueLibrary.Mission_04.StumpyIntro.Line16", 3, 0),
                    new("DialogueLibrary.Mission_04.StumpyIntro.Line17", 3, 0),
                    new("DialogueLibrary.Mission_04.StumpyIntro.Line18", 3, 0),
                    new("DialogueLibrary.Mission_04.StumpyIntro.Line19", 3, 0)
                ], MusicLoader.GetMusicSlot(ReverieMusic.ReverieMusic.Instance, Assets.Music + "StumpysTheme")),

                #endregion
            };
        }

        public static DialogueSequence GetDialogueById(DialogueID id)
            => _dialogues.TryGetValue(id, out var dialogue) ? dialogue : null;

        public static IEnumerable<DialogueSequence> GetAllDialogues() => _dialogues.Values;    
    }

}