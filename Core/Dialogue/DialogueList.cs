using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Core.Dialogue
{
    public static class DialogueList
    {
        private static Dictionary<DialogueID, DialogueSequence> _dialogues;
        static DialogueList() => Initialize();     

        private static void Initialize()
        {
            _dialogues = new Dictionary<DialogueID, DialogueSequence>
            {
                #region MAINLINE

                #region FIRST MISSION
                [DialogueID.WakingUpToTheGuideYapping] = new(
                DialogueType.ProgressionEvent,
                [
                    new("DialogueLibrary.Reawakening.WakingUp.Line1", 2, 0),
                    new("DialogueLibrary.Reawakening.WakingUp.Line2", 3, 0),
                    new("DialogueLibrary.Reawakening.WakingUp.Line3", 3, 0),
                    new("DialogueLibrary.Reawakening.WakingUp.Line4", 3, 0),
                    new("DialogueLibrary.Reawakening.WakingUp.Line5", 3, 0),
                    new("DialogueLibrary.Reawakening.WakingUp.Line6", 3, 0),
                    new("DialogueLibrary.Reawakening.WakingUp.Line7", 2, 0, speakingNPC: NPCDataManager.Default),
                    new("DialogueLibrary.Reawakening.WakingUp.Line8", 10, 0),
                    new("DialogueLibrary.Reawakening.WakingUp.Line9", 3, 0),
                    new("DialogueLibrary.Reawakening.WakingUp.Line10", 3, 0),
                    new("DialogueLibrary.Reawakening.WakingUp.Line11", 3, 0)
                ], MusicID.OtherworldlyDay),

                [DialogueID.GuideYappingAboutReverieLore] = new(
                DialogueType.ProgressionEvent,
                [
                    new("DialogueLibrary.Reawakening.Briefing.Line1", 3, 0),
                    new("DialogueLibrary.Reawakening.Briefing.Line2", 3, 0),
                    new("DialogueLibrary.Reawakening.Briefing.Line3", 3, 0),
                    new("DialogueLibrary.Reawakening.Briefing.Line4", 3, 0),
                    new("DialogueLibrary.Reawakening.Briefing.Line5", 3, 0),
                    new("DialogueLibrary.Reawakening.Briefing.Line6", 3, 0),
                    new("DialogueLibrary.Reawakening.Briefing.Line7", 6, 0),
                    new("DialogueLibrary.Reawakening.Briefing.Line8", 3, 0),
                    new("DialogueLibrary.Reawakening.Briefing.Line9", 3, 0),
                    new("DialogueLibrary.Reawakening.Briefing.Line10", 3, 0),
                    new("DialogueLibrary.Reawakening.Briefing.Line11", 3, 0),
                    new("DialogueLibrary.Reawakening.Briefing.Line12", 3, 0),
                    new("DialogueLibrary.Reawakening.Briefing.Line13", 3, 0),
                    new("DialogueLibrary.Reawakening.Briefing.Line14", 3, 0),
                    new("DialogueLibrary.Reawakening.Briefing.Line15", 3, 0)
                ]),

                [DialogueID.GuideGivingPropsTrainingArc] = new(
                DialogueType.Mission,
                [
                    new("DialogueLibrary.Reawakening.TrainingComplete.Line1", 3, 0),
                    new("DialogueLibrary.Reawakening.TrainingComplete.Line2", 3, 0),
                    new("DialogueLibrary.Reawakening.TrainingComplete.Line3", 3, 0),
                ]),

                [DialogueID.GuideGivesYouAMagicMirror] = new(
                DialogueType.ProgressionEvent,
                [
                    new("DialogueLibrary.Reawakening.GuideGivesYouAMagicMirror.Line1", 3, 0),
                    new("DialogueLibrary.Reawakening.GuideGivesYouAMagicMirror.Line2", 3, 0),
                    new("DialogueLibrary.Reawakening.GuideGivesYouAMagicMirror.Line3", 3, 0),
                    new("DialogueLibrary.Reawakening.GuideGivesYouAMagicMirror.Line4", 1, 0, Color.Yellow),
                    new("DialogueLibrary.Reawakening.GuideGivesYouAMagicMirror.Line5", 3, 0),
                    new("DialogueLibrary.Reawakening.GuideGivesYouAMagicMirror.Line6", 3, 0),
                    new("DialogueLibrary.Reawakening.GuideGivesYouAMagicMirror.Line7", 3, 0),
                    new("DialogueLibrary.Reawakening.GuideGivesYouAMagicMirror.Line8", 3, 0)
                ]),

                [DialogueID.ReawakeningEndingDialogue] = new(
                DialogueType.Mission,
                [
                    new("DialogueLibrary.Reawakening.ReawakeningEndingDialogue.Line1", 3, 0),
                    new("DialogueLibrary.Reawakening.ReawakeningEndingDialogue.Line2", 3, 0),
                    new("DialogueLibrary.Reawakening.ReawakeningEndingDialogue.Line3", 3, 0),
                    new("DialogueLibrary.Reawakening.ReawakeningEndingDialogue.Line4", 2, 0),
                    new("DialogueLibrary.Reawakening.ReawakeningEndingDialogue.Line5", 3, 0),
                    new("DialogueLibrary.Reawakening.ReawakeningEndingDialogue.Line6", 3, 0),
                    new("DialogueLibrary.Reawakening.ReawakeningEndingDialogue.Line7", 5, 0),
                    new("DialogueLibrary.Reawakening.ReawakeningEndingDialogue.Line8", 3, 0),
                ]),

                #region Class Selection Dialogue
                [DialogueID.SelectedClass_Marksman] = new(
                DialogueType.Mission,
                [
                    new("DialogueLibrary.Reawakening.SelectedClass.Marksman.Line1", 3, 0),
                    new("DialogueLibrary.Reawakening.SelectedClass.Marksman.Line2", 3, 0),
                    new("DialogueLibrary.Reawakening.SelectedClass.Marksman.Line3", 3, 0),
                ]),

                [DialogueID.SelectedClass_Warrior] = new(
                DialogueType.Mission,
                [
                    new("DialogueLibrary.Reawakening.SelectedClass.Warrior.Line1", 3, 0),
                    new("DialogueLibrary.Reawakening.SelectedClass.Warrior.Line2", 3, 0),
                    new("DialogueLibrary.Reawakening.SelectedClass.Warrior.Line3", 3, 0),
                ]),

                [DialogueID.SelectedClass_Mage] = new(
                DialogueType.Mission,
                [
                    new("DialogueLibrary.Reawakening.SelectedClass.Mage.Line1", 3, 0),
                    new("DialogueLibrary.Reawakening.SelectedClass.Mage.Line2", 3, 0),
                    new("DialogueLibrary.Reawakening.SelectedClass.Mage.Line3", 3, 0),
                ]),

                [DialogueID.SelectedClass_Conjurer] = new(
                DialogueType.ProgressionEvent,
                [
                    new("DialogueLibrary.Reawakening.SelectedClass.Conjurer.Line1", 3, 0),
                    new("DialogueLibrary.Reawakening.SelectedClass.Conjurer.Line2", 3, 0),
                    new("DialogueLibrary.Reawakening.SelectedClass.Conjurer.Line3", 3, 0),
                ]),
                #endregion


                #endregion

                [DialogueID.KilledTheEye] = new(
                DialogueType.Mission,
                [
                    new("DialogueLibrary.Reawakening.KilledTheEye.Line1", 3, 0),
                    new("DialogueLibrary.Reawakening.KilledTheEye.Line2", 3, 0),
                    new("DialogueLibrary.Reawakening.KilledTheEye.Line3", 4, 0),
                    new("DialogueLibrary.Reawakening.KilledTheEye.Line4", 3, 0),
                    new("DialogueLibrary.Reawakening.KilledTheEye.Line5", 4, 0),
                    new("DialogueLibrary.Reawakening.KilledTheEye.Line6", 4, 0),
                    new("DialogueLibrary.Reawakening.KilledTheEye.Line7", 3, 0),
                    new("DialogueLibrary.Reawakening.KilledTheEye.Line8", 3, 0)
                ]),
                #endregion

                #region SIDE MISSION

                [DialogueID.ArgiesHuntIntro] = new(
                DialogueType.Mission,
                [
                    new("DialogueLibrary.Reawakening.ArgiesHuntIntro.Line1", 3, 0),
                    new("DialogueLibrary.Reawakening.ArgiesHuntIntro.Line2", 3, 0),
                    new("DialogueLibrary.Reawakening.ArgiesHuntIntro.Line3", 3, 0),
                    new("DialogueLibrary.Reawakening.ArgiesHuntIntro.Line4", 3, 0),
                    new("DialogueLibrary.Reawakening.ArgiesHuntIntro.Line5", 3, 0),
                    new("DialogueLibrary.Reawakening.ArgiesHuntIntro.Line6", 2, 0)
                ], MusicLoader.GetMusicSlot(ReverieMusic.ReverieMusic.Instance, Assets.Music + "MushnibEncounter")),

                [DialogueID.StumpysIntro] = new(
                DialogueType.Mission,
                [
                    new("DialogueLibrary.Reawakening.StumpysIntro.Line1", 3, 0),
                    new("DialogueLibrary.Reawakening.StumpysIntro.Line2", 3, 0),
                    new("DialogueLibrary.Reawakening.StumpysIntro.Line3", 3, 0),
                    new("DialogueLibrary.Reawakening.StumpysIntro.Line4", 3, 0),
                    new("DialogueLibrary.Reawakening.StumpysIntro.Line5", 3, 0),
                    new("DialogueLibrary.Reawakening.StumpysIntro.Line6", 2, 0),
                    new("DialogueLibrary.Reawakening.StumpysIntro.Line7", 3, 0),
                    new("DialogueLibrary.Reawakening.StumpysIntro.Line8", 3, 0),
                    new("DialogueLibrary.Reawakening.StumpysIntro.Line9", 3, 0),
                    new("DialogueLibrary.Reawakening.StumpysIntro.Line10", 3, 0),
                    new("DialogueLibrary.Reawakening.StumpysIntro.Line11", 3, 0),
                    new("DialogueLibrary.Reawakening.StumpysIntro.Line12", 5, 0),
                    new("DialogueLibrary.Reawakening.StumpysIntro.Line13", 4, 0),
                    new("DialogueLibrary.Reawakening.StumpysIntro.Line14", 3, 0),
                    new("DialogueLibrary.Reawakening.StumpysIntro.Line15", 3, 0),
                    new("DialogueLibrary.Reawakening.StumpysIntro.Line16", 3, 0),
                    new("DialogueLibrary.Reawakening.StumpysIntro.Line17", 3, 0),
                    new("DialogueLibrary.Reawakening.StumpysIntro.Line18", 3, 0),
                    new("DialogueLibrary.Reawakening.StumpysIntro.Line19", 3, 0)
                ], MusicLoader.GetMusicSlot(ReverieMusic.ReverieMusic.Instance, Assets.Music + "StumpysTheme")),

                #endregion

                #region RANDOM EVENTS
                [DialogueID.GuideWhenYouFindAnAccessory] = new(
                DialogueType.ProgressionEvent,
                [
                    new("DialogueLibrary.Reawakening.GuideWhenYouFindAnAccessory.Line1", 3, 0),
                    new("DialogueLibrary.Reawakening.GuideWhenYouFindAnAccessory.Line2", 3, 0),
                    new("DialogueLibrary.Reawakening.GuideWhenYouFindAnAccessory.Line3", 3, 0),
                    new("DialogueLibrary.Reawakening.GuideWhenYouFindAnAccessory.Line4", 3, 0),
                    new("DialogueLibrary.Reawakening.GuideWhenYouFindAnAccessory.Line5", 3, 0),
                    new("DialogueLibrary.Reawakening.GuideWhenYouFindAnAccessory.Line6", 3, 0),
                    new("DialogueLibrary.Reawakening.GuideWhenYouFindAnAccessory.Line7", 3, 0)
                ]),

                [DialogueID.EnteredWoodlandCanopyBeforeProgression] = new(
                DialogueType.ProgressionEvent,
                [
                    new("DialogueLibrary.Reawakening.EnteredWoodlandCanopyBeforeProgression.Line1", 3, 0),
                    new("DialogueLibrary.Reawakening.EnteredWoodlandCanopyBeforeProgression.Line2", 3, 0),
                    new("DialogueLibrary.Reawakening.EnteredWoodlandCanopyBeforeProgression.Line3", 3, 0),
                    new("DialogueLibrary.Reawakening.EnteredWoodlandCanopyBeforeProgression.Line4", 3, 0),
                    new("DialogueLibrary.Reawakening.EnteredWoodlandCanopyBeforeProgression.Line5", 3, 0)
                ]),
                #endregion

            };
        }

        public static DialogueSequence GetDialogueById(DialogueID id)
            => _dialogues.TryGetValue(id, out var dialogue) ? dialogue : null;

        public static IEnumerable<DialogueSequence> GetAllDialogues() => _dialogues.Values;    
    }
    public enum DialogueID
    {
        WakingUpToTheGuideYapping,
        GuideYappingAboutReverieLore,

        SelectedClass_Warrior,
        SelectedClass_Marksman,
        SelectedClass_Mage,
        SelectedClass_Conjurer,

        GuideWhenYouFindAnAccessory,
        EnteredWoodlandCanopyBeforeProgression,

        GuideGivingPropsTrainingArc,
        GuideGivesYouAMagicMirror,
        ReawakeningEndingDialogue,
        KilledTheEye,

        StumpysIntro,
        ArgiesHuntIntro
    }
}
