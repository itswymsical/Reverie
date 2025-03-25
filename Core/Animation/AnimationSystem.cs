using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Reverie.Common.Animation;

public class AnimationFrame
{
    public float TimePosition { get; set; }
    public Dictionary<JointType, JointState> Joints { get; } = new Dictionary<JointType, JointState>();

    public AnimationFrame(float timePosition)
    {
        TimePosition = timePosition;
    }
}

public class ReverieAnimation
{
    public string Id { get; }
    public string Name { get; }
    public float Duration { get; }
    public bool Looping { get; set; } = true;
    public List<AnimationFrame> Frames { get; } = new List<AnimationFrame>();
    public bool UseVanillaFramesWhenRunning { get; set; } = false;

    public ReverieAnimation(string id, string name, float duration)
    {
        Id = id;
        Name = name;
        Duration = duration;
    }

    public void AddFrame(AnimationFrame frame)
    {
        Frames.Add(frame);
        // Sort frames by time position
        Frames.Sort((a, b) => a.TimePosition.CompareTo(b.TimePosition));
    }

    public Dictionary<JointType, JointState> GetStateAtTime(float time)
    {
        // Handle empty animation
        if (Frames.Count == 0)
            return new Dictionary<JointType, JointState>();

        // Handle time looping
        if (Looping && time > Duration)
            time %= Duration;

        // Clamp time
        time = MathHelper.Clamp(time, 0, Duration);

        // Find surrounding frames
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

        // If we're at the beginning
        if (prevFrame == null)
            return nextFrame.Joints;

        // If we're at the end
        if (nextFrame == null)
            return prevFrame.Joints;

        // Interpolate between frames
        float frameDuration = nextFrame.TimePosition - prevFrame.TimePosition;
        float t = frameDuration > 0 ? (time - prevFrame.TimePosition) / frameDuration : 0;

        Dictionary<JointType, JointState> result = new Dictionary<JointType, JointState>();

        // Get all joint types in either frame
        HashSet<JointType> allJointTypes = new HashSet<JointType>();
        foreach (var type in prevFrame.Joints.Keys)
            allJointTypes.Add(type);
        foreach (var type in nextFrame.Joints.Keys)
            allJointTypes.Add(type);

        // Interpolate each joint
        foreach (JointType jointType in allJointTypes)
        {
            bool hasPrev = prevFrame.Joints.TryGetValue(jointType, out var prevState);
            bool hasNext = nextFrame.Joints.TryGetValue(jointType, out var nextState);

            if (hasPrev && hasNext)
            {
                // Interpolate
                result[jointType] = JointState.Lerp(prevState, nextState, t);
            }
            else if (hasPrev)
            {
                result[jointType] = prevState.Clone();
            }
            else if (hasNext)
            {
                result[jointType] = nextState.Clone();
            }
        }

        return result;
    }
}