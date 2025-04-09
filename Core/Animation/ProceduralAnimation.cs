using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;

namespace Reverie.Core.Animation
{
    /// <summary>
    /// Represents different segments of a player's body that can be animated.
    /// </summary>
    public enum AnimationSegment
    {
        HEAD,
        BODY,
        LEGS,
        ARM_FRONT,
        ARM_BACK
    }

    /// <summary>
    /// Interface for animation packages that can be applied to player.
    /// </summary>
    public interface IAnimationPackage
    {
        /// <summary>
        /// Gets the unique identifier for this animation package.
        /// </summary>
        string UniqueID { get; }

        /// <summary>
        /// Gets the priority of this animation package. Higher priority animations override lower ones.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Gets whether this animation is active.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Gets the segments this animation affects.
        /// </summary>
        IEnumerable<AnimationSegment> AffectedSegments { get; }

        /// <summary>
        /// Updates the animation state.
        /// </summary>
        /// <param name="player">The player to animate.</param>
        void Update(Player player);

        /// <summary>
        /// Applies animation transformations to the specified segment.
        /// </summary>
        /// <param name="segment">The segment to animate.</param>
        /// <param name="drawInfo">The drawing information.</param>
        /// <param name="gameTime">The current game time count.</param>
        void ApplySegmentTransformation(AnimationSegment segment, ref PlayerDrawSet drawInfo, int gameTime);

        /// <summary>
        /// Initializes the animation.
        /// </summary>
        /// <param name="player">The player to animate.</param>
        void Initialize(Player player);

        /// <summary>
        /// Resets the animation state.
        /// </summary>
        /// <param name="player">The player to animate.</param>
        void Reset(Player player);
    }

    /// <summary>
    /// Base class for animation packages.
    /// </summary>
    public abstract class AbstractAnimationPackage : IAnimationPackage
    {
        /// <summary>
        /// Gets the unique identifier for this animation package.
        /// </summary>
        public abstract string UniqueID { get; }

        /// <summary>
        /// Gets the priority of this animation package. Higher priority animations override lower ones.
        /// </summary>
        public virtual int Priority => 0;

        /// <summary>
        /// Gets whether this animation is active.
        /// </summary>
        public bool IsActive { get; protected set; }

        /// <summary>
        /// Gets the segments this animation affects.
        /// </summary>
        public abstract IEnumerable<AnimationSegment> AffectedSegments { get; }

        /// <summary>
        /// The time when this animation started.
        /// </summary>
        protected int StartTime { get; private set; }

        /// <summary>
        /// Gets the animation elapsed time.
        /// </summary>
        protected int ElapsedTime => (int)Main.GameUpdateCount - StartTime;

        /// <summary>
        /// Updates the animation state.
        /// </summary>
        /// <param name="player">The player to animate.</param>
        public virtual void Update(Player player)
        {
            // Default implementation does nothing
        }

        /// <summary>
        /// Applies animation transformations to the specified segment.
        /// </summary>
        /// <param name="segment">The segment to animate.</param>
        /// <param name="drawInfo">The drawing information.</param>
        /// <param name="gameTime">The current game time count.</param>
        public abstract void ApplySegmentTransformation(AnimationSegment segment, ref PlayerDrawSet drawInfo, int gameTime);

        /// <summary>
        /// Initializes the animation.
        /// </summary>
        /// <param name="player">The player to animate.</param>
        public virtual void Initialize(Player player)
        {
            StartTime = (int)Main.GameUpdateCount;
            IsActive = true;
        }

        /// <summary>
        /// Resets the animation state.
        /// </summary>
        /// <param name="player">The player to animate.</param>
        public virtual void Reset(Player player)
        {
            IsActive = false;
        }

        /// <summary>
        /// Calculates an oscillating value based on time.
        /// </summary>
        /// <param name="amplitude">The maximum value.</param>
        /// <param name="frequency">The frequency of oscillation.</param>
        /// <param name="offset">The phase offset.</param>
        /// <returns>An oscillating value between -amplitude and amplitude.</returns>
        protected float Oscillate(float amplitude, float frequency, float offset = 0f)
        {
            return amplitude * (float)Math.Cos(ElapsedTime * frequency + offset);
        }

        /// <summary>
        /// Calculates a pulse value based on time.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="frequency">The frequency of pulsation.</param>
        /// <returns>A value that pulses between min and max.</returns>
        protected float Pulse(float min, float max, float frequency)
        {
            var halfRange = (max - min) / 2f;
            var center = min + halfRange;
            return center + halfRange * (float)Math.Cos(ElapsedTime * frequency);
        }
    }

    /// <summary>
    /// Manages active animations for a player.
    /// </summary>
    public class AnimationController
    {
        private readonly List<IAnimationPackage> _animations = new List<IAnimationPackage>();
        private readonly Dictionary<AnimationSegment, IAnimationPackage> _activeAnimations = new Dictionary<AnimationSegment, IAnimationPackage>();

        // Track last frame positions to detect potential issues
        private Vector2 _lastHeadPosition;
        private float _lastHeadRotation;
        private Vector2 _lastBodyPosition;
        private float _lastBodyRotation;
        private Vector2 _lastLegPosition;
        private float _lastLegRotation;

        // Track when animation was applied in this frame
        private Dictionary<AnimationSegment, bool> _segmentAnimatedThisFrame = new Dictionary<AnimationSegment, bool>
        {
            { AnimationSegment.HEAD, false },
            { AnimationSegment.BODY, false },
            { AnimationSegment.LEGS, false },
            { AnimationSegment.ARM_FRONT, false },
            { AnimationSegment.ARM_BACK, false }
        };

        // Debug info
        public string LastDebugMessage { get; private set; } = "";

        /// <summary>
        /// Registers an animation package.
        /// </summary>
        /// <param name="animation">The animation to register.</param>
        public void RegisterAnimation(IAnimationPackage animation)
        {
            // Check if animation with same ID already exists
            for (var i = 0; i < _animations.Count; i++)
            {
                if (_animations[i].UniqueID == animation.UniqueID)
                {
                    // Replace existing animation
                    _animations[i] = animation;
                    return;
                }
            }

            // Add new animation
            _animations.Add(animation);
        }

        /// <summary>
        /// Updates the animation controller.
        /// </summary>
        /// <param name="player">The player to animate.</param>
        public void Update(Player player)
        {
            // Store previous positions/rotations for debugging
            _lastHeadPosition = player.headPosition;
            _lastHeadRotation = player.headRotation;
            _lastBodyPosition = player.bodyPosition;
            _lastBodyRotation = player.bodyRotation;
            _lastLegPosition = player.legPosition;
            _lastLegRotation = player.legRotation;

            // Reset segment animation tracking
            foreach (var key in _segmentAnimatedThisFrame.Keys.ToList())
            {
                _segmentAnimatedThisFrame[key] = false;
            }

            // Update all animations
            foreach (var animation in _animations)
            {
                animation.Update(player);
            }

            // Clear active animations
            _activeAnimations.Clear();

            // Build active animation list for each segment
            foreach (var animation in _animations)
            {
                if (!animation.IsActive)
                    continue;

                foreach (var segment in animation.AffectedSegments)
                {
                    // Check if segment already has an animation
                    if (_activeAnimations.TryGetValue(segment, out var existingAnimation))
                    {
                        // Replace if new animation has higher priority
                        if (animation.Priority > existingAnimation.Priority)
                        {
                            _activeAnimations[segment] = animation;
                        }
                    }
                    else
                    {
                        // Add animation for segment
                        _activeAnimations[segment] = animation;
                    }
                }
            }
        }

        /// <summary>
        /// Applies animation transformations to the specified segment.
        /// </summary>
        /// <param name="segment">The segment to animate.</param>
        /// <param name="drawInfo">The drawing information.</param>
        public void ApplySegmentTransformation(AnimationSegment segment, ref PlayerDrawSet drawInfo)
        {
            try
            {
                // Apply animation for segment if it exists
                if (_activeAnimations.TryGetValue(segment, out var animation))
                {
                    // Mark this segment as animated this frame
                    _segmentAnimatedThisFrame[segment] = true;

                    // Apply the animation
                    animation.ApplySegmentTransformation(segment, ref drawInfo, (int)Main.GameUpdateCount);

                    // Debug info - detect large changes that might indicate issues
                    var player = drawInfo.drawPlayer;
                    switch (segment)
                    {
                        case AnimationSegment.HEAD:
                            if (Vector2.Distance(player.headPosition, _lastHeadPosition) > 10f)
                            {
                                LastDebugMessage = $"Warning: Large head position change: {Vector2.Distance(player.headPosition, _lastHeadPosition)}";
                            }
                            break;
                        case AnimationSegment.BODY:
                            if (Vector2.Distance(player.bodyPosition, _lastBodyPosition) > 10f)
                            {
                                LastDebugMessage = $"Warning: Large body position change: {Vector2.Distance(player.bodyPosition, _lastBodyPosition)}";
                            }
                            break;
                        case AnimationSegment.LEGS:
                            if (Vector2.Distance(player.legPosition, _lastLegPosition) > 10f)
                            {
                                LastDebugMessage = $"Warning: Large leg position change: {Vector2.Distance(player.legPosition, _lastLegPosition)}";
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Catch any exceptions during animation
                LastDebugMessage = $"Error in animation: {ex.Message}";
            }
        }

        /// <summary>
        /// Gets an animation by its unique ID.
        /// </summary>
        /// <param name="uniqueID">The unique ID of the animation.</param>
        /// <returns>The animation package, or null if not found.</returns>
        public IAnimationPackage GetAnimation(string uniqueID)
        {
            return _animations.Find(a => a.UniqueID == uniqueID);
        }

        /// <summary>
        /// Initializes an animation.
        /// </summary>
        /// <param name="uniqueID">The unique ID of the animation.</param>
        /// <param name="player">The player to animate.</param>
        /// <returns>True if the animation was found and initialized, false otherwise.</returns>
        public bool InitializeAnimation(string uniqueID, Player player)
        {
            var animation = GetAnimation(uniqueID);
            if (animation != null)
            {
                animation.Initialize(player);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Resets all animations.
        /// </summary>
        /// <param name="player">The player to animate.</param>
        public void ResetAllAnimations(Player player)
        {
            foreach (var animation in _animations)
            {
                animation.Reset(player);
            }
            _activeAnimations.Clear();
        }

        /// <summary>
        /// Gets all active animations for debugging.
        /// </summary>
        public IEnumerable<IAnimationPackage> GetActiveAnimations()
        {
            return _activeAnimations.Values.Distinct();
        }

        /// <summary>
        /// Gets all registered animations for debugging.
        /// </summary>
        public IEnumerable<IAnimationPackage> GetAllAnimations()
        {
            return _animations;
        }

        /// <summary>
        /// Returns debug status about which segments are currently animated.
        /// </summary>
        public Dictionary<AnimationSegment, bool> GetAnimatedSegmentsStatus()
        {
            return new Dictionary<AnimationSegment, bool>(_segmentAnimatedThisFrame);
        }
    }
}