using Reverie.Core.Animation;

namespace Reverie.Common.Players.Animation
{
    /// <summary>
    /// Represents an animation that is currently playing on a specific layer
    /// </summary>
    public class ActiveAnimation
    {
        /// <summary>
        /// The animation being played
        /// </summary>
        public ProceduralAnimation Animation { get; set; }

        /// <summary>
        /// Current time position in the animation
        /// </summary>
        public float Time { get; set; }

        /// <summary>
        /// The layer this animation is playing on
        /// </summary>
        public AnimationLayer Layer { get; set; }

        /// <summary>
        /// Duration for blending into this animation
        /// </summary>
        public float BlendInDuration { get; set; }

        /// <summary>
        /// Duration for blending out of this animation
        /// </summary>
        public float BlendOutDuration { get; set; }

        /// <summary>
        /// Current blend factor (0-1)
        /// </summary>
        public float BlendFactor { get; set; }

        /// <summary>
        /// Whether this animation is currently blending in
        /// </summary>
        public bool IsBlendingIn { get; set; } = true;

        /// <summary>
        /// Whether this animation is currently blending out
        /// </summary>
        public bool IsBlendingOut { get; set; } = false;
    }
}