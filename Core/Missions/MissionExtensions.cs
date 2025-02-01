using System.Linq;
using System;
using Reverie.Core.Missions;

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
            ObjectiveSets = mission.MissionData.ObjectiveSets
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
            NextMissionID = mission.MissionData.NextMissionID
        };
    }

    public static void LoadState(this Mission mission, MissionDataContainer state)
    {
        if (state == null) return;

        mission.Progress = state.Progress;
        mission.State = state.State;
        mission.Unlocked = state.Unlocked;
        mission.CurrentSetIndex = state.CurrentSetIndex;


        for (int i = 0; i < Math.Min(mission.MissionData.ObjectiveSets.Count, state.ObjectiveSets.Count); i++)
        {
            var savedSet = state.ObjectiveSets[i];
            var currentSet = mission.MissionData.ObjectiveSets[i];

            for (int j = 0; j < Math.Min(currentSet.Objectives.Count, savedSet.Objectives.Count); j++)
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