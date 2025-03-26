using System.Collections.Generic;

namespace Reverie.Core.Animation;

public class AnimationFrame
{
    public float TimePosition { get; set; }
    public Dictionary<SegmentType, SegmentState> Parts { get; } = [];

    public AnimationFrame(float timePosition)
    {
        TimePosition = timePosition;
    }
}

public class ProceduralAnimation
{
    public string Id { get; }
    public string Name { get; }
    public float Duration { get; }
    public bool Looping { get; set; } = true;
    public List<AnimationFrame> Frames { get; } = [];
    public bool UseVanillaFramesWhenRunning { get; set; } = false;

    public ProceduralAnimation(string id, string name, float duration)
    {
        Id = id;
        Name = name;
        Duration = duration;
    }

    public void AddFrame(AnimationFrame frame)
    {
        Frames.Add(frame);
        Frames.Sort((a, b) => a.TimePosition.CompareTo(b.TimePosition));
    }

    public Dictionary<SegmentType, SegmentState> GetStateAtTime(float time)
    {

        if (Frames.Count == 0)
            return new Dictionary<SegmentType, SegmentState>();

        if (Looping && time > Duration)
            time %= Duration;

        time = MathHelper.Clamp(time, 0, Duration);

        AnimationFrame prevFrame = null;
        AnimationFrame nextFrame = null;

        foreach (var frame in Frames)
        {
            if (frame.TimePosition <= time)
                prevFrame = frame;
            else
            {
                nextFrame = frame;
                break;
            }
        }

        if (prevFrame == null)
            return nextFrame.Parts;

        if (nextFrame == null)
            return prevFrame.Parts;

        var frameDuration = nextFrame.TimePosition - prevFrame.TimePosition;
        var t = frameDuration > 0 ? (time - prevFrame.TimePosition) / frameDuration : 0;

        var result = new Dictionary<SegmentType, SegmentState>();

        var allSegmentTypes = new HashSet<SegmentType>();
        foreach (var type in prevFrame.Parts.Keys)
            allSegmentTypes.Add(type);
        foreach (var type in nextFrame.Parts.Keys)
            allSegmentTypes.Add(type);

        // Interpolate each segment
        foreach (var segmentType in allSegmentTypes)
        {
            var hasPrev = prevFrame.Parts.TryGetValue(segmentType, out var prevState);
            var hasNext = nextFrame.Parts.TryGetValue(segmentType, out var nextState);

            if (hasPrev && hasNext)
            {
                result[segmentType] = SegmentState.Lerp(prevState, nextState, t);
            }
            else if (hasPrev)
            {
                result[segmentType] = prevState.Clone();
            }
            else if (hasNext)
            {
                result[segmentType] = nextState.Clone();
            }
        }

        return result;
    }
}