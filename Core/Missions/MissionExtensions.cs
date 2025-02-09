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
            CurrentSetIndex = mission.CurrentSetIndex,
            ObjectiveSets = mission.ObjectiveSets
                .Select(set => new ObjectiveSetState
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
        mission.CurrentSetIndex = state.CurrentSetIndex;


        for (var i = 0; i < Math.Min(mission.ObjectiveSets.Count, state.ObjectiveSets.Count); i++)
        {
            var savedSet = state.ObjectiveSets[i];
            var currentSet = mission.ObjectiveSets[i];

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