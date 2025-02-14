using System.Linq;

namespace Reverie.Core.Missions;

public static class MissionExtensions
{
    public static MissionDataContainer ToState(this Mission mission)
    {
        return new MissionDataContainer
        {
            ID = mission.ID,
            Progress = mission.Progress,
            State = mission.State,
            Unlocked = mission.Unlocked,
            CurObjectiveIndex = mission.CurObjectiveIndex,
            ObjectiveIndex = mission.ObjectiveIndex
                .Select(set => new ObjectiveIndextate
                {
                    Objectives = set.Objectives
                        .Select(obj => new ObjectiveState
                        {
                            Description = obj.Description,
                            IsCompleted = obj.IsCompleted,
                            RequiredCount = obj.RequiredCount,
                            CurrentCount = obj.CurrentCount
                        }).ToList()
                }).ToList(),
            NextMissionID = mission.NextMissionID
        };
    }

    public static void LoadState(this Mission mission, MissionDataContainer state)
    {
        if (state == null) return;

        mission.Progress = state.Progress;
        mission.State = state.State;
        mission.Unlocked = state.Unlocked;
        mission.CurObjectiveIndex = state.CurObjectiveIndex;


        for (var i = 0; i < Math.Min(mission.ObjectiveIndex.Count, state.ObjectiveIndex.Count); i++)
        {
            var savedSet = state.ObjectiveIndex[i];
            var currentSet = mission.ObjectiveIndex[i];

            for (var j = 0; j < Math.Min(currentSet.Objectives.Count, savedSet.Objectives.Count); j++)
            {
                var savedObj = savedSet.Objectives[j];
                var currentObj = currentSet.Objectives[j];

                if (savedObj.Description == currentObj.Description)
                {
                    currentObj.IsCompleted = savedObj.IsCompleted;
                    currentObj.CurrentCount = savedObj.CurrentCount;
                }
            }
        }
    }
}