using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Core.Dialogue
{
    public enum DialogueID
    {
        Reawakening_Opening,
        Reawakening_ProcessingSequence,
        Reawakening_EyeSequence,
        Reawakening_GuideResponse,
        Reawakening_TrainingSequence,
        Reawakening_SylvanForeshadow,
        Reawakening_ProgressCheck,
        Reawakening_TrainingComplete,
        Reawakening_MagicMirror,
        Reawakening_EyeReturn,
        Reawakening_SylvanwaldeTeaser,
        Reawakening_Closing,
    }

    public static class DialogueList
    {
        private static Dictionary<DialogueID, DialogueSequence> _dialogues;
        static DialogueList() => Initialize();     

        private static void Initialize()
        {
            _dialogues = new Dictionary<DialogueID, DialogueSequence>
            {
                [DialogueID.Reawakening_Opening] = new(
                DialogueType.Mission,
                [        
                    new("DialogueLibrary.Reawakening.Opening.Line1", 2, 0),
                    new("DialogueLibrary.Reawakening.Opening.Line2", 2, 0),
                    new("DialogueLibrary.Reawakening.Opening.Line3", 2, 0),
                    new("DialogueLibrary.Reawakening.Opening.Line4", 2, 0),
                    new("DialogueLibrary.Reawakening.Opening.Line5", 2, 0),
                    new("DialogueLibrary.Reawakening.Opening.Line6", 2, 0),
                    new("DialogueLibrary.Reawakening.Opening.Line7", 2, 0),
                    new("DialogueLibrary.Reawakening.Opening.Line8", 2, 0),
                    new("DialogueLibrary.Reawakening.Opening.Line9", 2, 0),
                    new("DialogueLibrary.Reawakening.Opening.Line10", 2, 0),
                    new("DialogueLibrary.Reawakening.Opening.Line11", 2, 0),
                    new("DialogueLibrary.Reawakening.Opening.Line12", 2, 0),
                    new("DialogueLibrary.Reawakening.Opening.Line13", 2, 0),
                    new("DialogueLibrary.Reawakening.Opening.Line14", 2, 0)

                ], MusicLoader.GetMusicSlot(ReverieMusic.ReverieMusic.Instance, $"{Assets.Music}Resurgence")),

                [DialogueID.Reawakening_ProcessingSequence] = new(
                DialogueType.Mission,
                [
                    new("DialogueLibrary.Reawakening.ProcessingSequence.Line1", 2, 0),
                    new("DialogueLibrary.Reawakening.ProcessingSequence.Line2", 2, 0),
                    new("DialogueLibrary.Reawakening.ProcessingSequence.Line3", 2, 0),
                    new("DialogueLibrary.Reawakening.ProcessingSequence.Line4", 2, 0),
                    new("DialogueLibrary.Reawakening.ProcessingSequence.Line5", 2, 0),
                    new("DialogueLibrary.Reawakening.ProcessingSequence.Line6", 2, 0),
                    new("DialogueLibrary.Reawakening.ProcessingSequence.Line7", 2, 0),
                ], MusicLoader.GetMusicSlot(ReverieMusic.ReverieMusic.Instance, $"{Assets.Music}Resurgence")),

                [DialogueID.Reawakening_EyeSequence] = new(
                DialogueType.Mission,
                [
                    new("DialogueLibrary.Reawakening.EyeSequence.Line1", 2, 0),
                    new("DialogueLibrary.Reawakening.EyeSequence.Line2", 2, 0),
                    new("DialogueLibrary.Reawakening.EyeSequence.Line3", 2, 0),
                    new("DialogueLibrary.Reawakening.EyeSequence.Line4", 2, 0),
                    new("DialogueLibrary.Reawakening.EyeSequence.Line5", 2, 0),
                    new("DialogueLibrary.Reawakening.EyeSequence.Line6", 2, 0),
                    new("DialogueLibrary.Reawakening.EyeSequence.Line7", 2, 0),
                    new("DialogueLibrary.Reawakening.EyeSequence.Line8", 2, 0)
                ]),

                [DialogueID.Reawakening_GuideResponse] = new(
                DialogueType.Mission,
                [
                    new("DialogueLibrary.re.MagicMirror.Line1", 2, 0),
                    new("DialogueLibrary.Reawakening.GuideResponse.Line2", 2, 0),
                    new("DialogueLibrary.Reawakening.GuideResponse.Line3", 1,  0, Color.Yellow),
                    new("DialogueLibrary.Reawakening.GuideResponse.Line4", 2, 0),
                    new("DialogueLibrary.Reawakening.GuideResponse.Line5", 2, 0),
                    new("DialogueLibrary.Reawakening.GuideResponse.Line6", 2, 0),
                ]),

                [DialogueID.Reawakening_TrainingSequence] = new(
                DialogueType.Mission,
                [
                    new("DialogueLibrary.Reawakening.TrainingSequence.Line1", 2, 0),
                    new("DialogueLibrary.Reawakening.TrainingSequence.Line2", 2, 0),
                    new("DialogueLibrary.Reawakening.TrainingSequence.Line3", 2, 0),
                    new("DialogueLibrary.Reawakening.TrainingSequence.Line4", 2, 0),
                    new("DialogueLibrary.Reawakening.TrainingSequence.Line5", 2, 0),
                    new("DialogueLibrary.Reawakening.TrainingSequence.Line6", 2, 0),
                    new("DialogueLibrary.Reawakening.TrainingSequence.Line7", 2, 0),
                    new("DialogueLibrary.Reawakening.TrainingSequence.Line8", 2, 0),
                    new("DialogueLibrary.Reawakening.TrainingSequence.Line9", 2, 0),
                    new("DialogueLibrary.Reawakening.TrainingSequence.Line10", 2, 0),
                    new("DialogueLibrary.Reawakening.TrainingSequence.Line11", 2, 0),
                ]),

                [DialogueID.Reawakening_SylvanForeshadow] = new(
                DialogueType.Mission,
                [
                    new("DialogueLibrary.Reawakening.SylvanForeshadow.Line1", 2, 0),
                    new("DialogueLibrary.Reawakening.SylvanForeshadow.Line2", 2, 0),
                    new("DialogueLibrary.Reawakening.SylvanForeshadow.Line3", 2, 0),
                    new("DialogueLibrary.Reawakening.SylvanForeshadow.Line4", 2, 0),
                    new("DialogueLibrary.Reawakening.SylvanForeshadow.Line5", 2, 0),
                    new("DialogueLibrary.Reawakening.SylvanForeshadow.Line6", 2, 0),
                    new("DialogueLibrary.Reawakening.SylvanForeshadow.Line7", 2, 0),
                    new("DialogueLibrary.Reawakening.SylvanForeshadow.Line8", 2, 0),
                    new("DialogueLibrary.Reawakening.SylvanForeshadow.Line9", 2, 0),
                    new("DialogueLibrary.Reawakening.SylvanForeshadow.Line10", 2, 0)
                ]),

                [DialogueID.Reawakening_ProgressCheck] = new(
                DialogueType.Mission,
                [
                    new("DialogueLibrary.Reawakening.ProgressCheck.Line1", 2, 0),
                    new("DialogueLibrary.Reawakening.ProgressCheck.Line2", 2, 0),
                    new("DialogueLibrary.Reawakening.ProgressCheck.Line3", 2, 0),
                    new("DialogueLibrary.Reawakening.ProgressCheck.Line4", 2, 0),
                    new("DialogueLibrary.Reawakening.ProgressCheck.Line5", 2, 0),
                ]),

                [DialogueID.Reawakening_TrainingComplete] = new(
                DialogueType.Mission,
                [
                    new("DialogueLibrary.Reawakening.TrainingComplete.Line1", 2, 0),
                    new("DialogueLibrary.Reawakening.TrainingComplete.Line2", 2, 0),
                    new("DialogueLibrary.Reawakening.TrainingComplete.Line3", 2, 0),
                    new("DialogueLibrary.Reawakening.TrainingComplete.Line4", 2, 0),
                ]),

                [DialogueID.Reawakening_MagicMirror] = new(
                DialogueType.Mission,
                [
                    new("DialogueLibrary.Reawakening.MagicMirror.Line1", 2, 0),
                    new("DialogueLibrary.Reawakening.MagicMirror.Line2", 2, 0),
                    new("DialogueLibrary.Reawakening.MagicMirror.Line3", 2, 0),
                    new("DialogueLibrary.Reawakening.MagicMirror.Line4", 2, 0),
                    new("DialogueLibrary.Reawakening.MagicMirror.Line5", 2, 0),
                    new("DialogueLibrary.Reawakening.MagicMirror.Line6", 2, 0),
                    new("DialogueLibrary.Reawakening.MagicMirror.Line7", 2, 0),
                ]),

                [DialogueID.Reawakening_EyeReturn] = new(
                DialogueType.Mission,
                [
                    new("DialogueLibrary.Reawakening.EyeReturn.Line1", 2, 0),
                    new("DialogueLibrary.Reawakening.EyeReturn.Line2", 2, 0),
                    new("DialogueLibrary.Reawakening.EyeReturn.Line3", 2, 0),
                    new("DialogueLibrary.Reawakening.EyeReturn.Line4", 2, 0),
                    new("DialogueLibrary.Reawakening.EyeReturn.Line5", 2, 0),
                    new("DialogueLibrary.Reawakening.EyeReturn.Line6", 2, 0),
                    new("DialogueLibrary.Reawakening.EyeReturn.Line7", 2, 0),
                      new("DialogueLibrary.Reawakening.EyeReturn.Line8", 2, 0),
                ]),

                [DialogueID.Reawakening_SylvanwaldeTeaser] = new(
                DialogueType.Mission,
                [
                    new("DialogueLibrary.Reawakening.SylvanwaldeTeaser.Line1", 2, 0),
                    new("DialogueLibrary.Reawakening.SylvanwaldeTeaser.Line2", 2, 0),
                    new("DialogueLibrary.Reawakening.SylvanwaldeTeaser.Line3", 2, 0),
                    new("DialogueLibrary.Reawakening.SylvanwaldeTeaser.Line4", 2, 0),
                    new("DialogueLibrary.Reawakening.SylvanwaldeTeaser.Line5", 2, 0),
                    new("DialogueLibrary.Reawakening.SylvanwaldeTeaser.Line6", 2, 0),
                    new("DialogueLibrary.Reawakening.SylvanwaldeTeaser.Line7", 2, 0),
                      new("DialogueLibrary.Reawakening.SylvanwaldeTeaser.Line8", 2, 0),
                ]),

                [DialogueID.Reawakening_Closing] = new(
                DialogueType.Mission,
                [
                    new("DialogueLibrary.Reawakening.Closing.Line1", 2, 0),
                    new("DialogueLibrary.Reawakening.Closing.Line2", 2, 0),
                    new("DialogueLibrary.Reawakening.Closing.Line3", 2, 0),
                    new("DialogueLibrary.Reawakening.Closing.Line4", 2, 0),
                    new("DialogueLibrary.Reawakening.Closing.Line5", 2, 0),
                    new("DialogueLibrary.Reawakening.Closing.Line6", 2, 0),
                    new("DialogueLibrary.Reawakening.Closing.Line7", 2, 0),
                ]),
            };
        }

        public static DialogueSequence GetDialogueById(DialogueID id) => _dialogues.TryGetValue(id, out var dialogue) ? dialogue : null;

        public static IEnumerable<DialogueSequence> GetAllDialogues() => _dialogues.Values;    
    }
}