using Reverie.Core.Animation.Packages;
using Terraria.DataStructures;

namespace Reverie.Core.Animation
{
    /// <summary>
    /// ModPlayer class that manages player animations.
    /// </summary>
    public class AnimationPlayer : ModPlayer
    {
        private AnimationController _animationController;

        // Store original positions and rotations to restore them if needed
        private Vector2 _originalHeadPosition;
        private float _originalHeadRotation;
        private Vector2 _originalBodyPosition;
        private float _originalBodyRotation;
        private Vector2 _originalLegPosition;
        private float _originalLegRotation;

        // Flag to track if animations are enabled
        public bool AnimationsEnabled { get; set; } = true;

        public override void Initialize()
        {
            _animationController = new AnimationController();

            // Register default animations
            _animationController.RegisterAnimation(new IdleAnimation());
            _animationController.RegisterAnimation(new JumpAnimation());
            _animationController.RegisterAnimation(new MovingAnimation());
            _animationController.RegisterAnimation(new UseAnimation());

        }

        public override void ResetEffects()
        {
            if (!AnimationsEnabled)
                return;

            // Store original positions/rotations before animations
            _originalHeadPosition = Player.headPosition;
            _originalHeadRotation = Player.headRotation;
            _originalBodyPosition = Player.bodyPosition;
            _originalBodyRotation = Player.bodyRotation;
            _originalLegPosition = Player.legPosition;
            _originalLegRotation = Player.legRotation;

            // Update animation controller
            _animationController.Update(Player);
        }

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (!AnimationsEnabled)
                return;

            try
            {
                // Apply animations to each segment
                _animationController.ApplySegmentTransformation(AnimationSegment.HEAD, drawInfo);
                _animationController.ApplySegmentTransformation(AnimationSegment.BODY, drawInfo);
                _animationController.ApplySegmentTransformation(AnimationSegment.LEGS, drawInfo);
                _animationController.ApplySegmentTransformation(AnimationSegment.ARM_FRONT, drawInfo);
                _animationController.ApplySegmentTransformation(AnimationSegment.ARM_BACK, drawInfo);

                // Safety check for NaN values which could crash the game
                SafetyCheckDrawValues(ref drawInfo);
            }
            catch (Exception ex)
            {
                // If any error occurs, restore original values and disable animations temporarily
                RestoreOriginalValues(ref drawInfo);

                // Log error
                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    Main.NewText($"Animation error: {ex.Message}", Color.Red);
                }

                // Disable animations for a short time
                DisableAnimationsTemporarily();
            }
        }

        /// <summary>
        /// Checks for invalid values in draw info and fixes them.
        /// </summary>
        private void SafetyCheckDrawValues(ref PlayerDrawSet drawInfo)
        {
            var player = drawInfo.drawPlayer;

            // Check for NaN values in positions
            if (float.IsNaN(player.headPosition.X) || float.IsNaN(player.headPosition.Y))
                player.headPosition = _originalHeadPosition;

            if (float.IsNaN(player.bodyPosition.X) || float.IsNaN(player.bodyPosition.Y))
                player.bodyPosition = _originalBodyPosition;

            if (float.IsNaN(player.legPosition.X) || float.IsNaN(player.legPosition.Y))
                player.legPosition = _originalLegPosition;

            // Check for NaN values in rotations
            if (float.IsNaN(player.headRotation))
                player.headRotation = _originalHeadRotation;

            if (float.IsNaN(player.bodyRotation))
                player.bodyRotation = _originalBodyRotation;

            if (float.IsNaN(player.legRotation))
                player.legRotation = _originalLegRotation;

            // Check for extreme values that might cause issues
            const float MAX_POSITION = 1000f;
            const float MAX_ROTATION = MathHelper.TwoPi;

            if (Math.Abs(player.headPosition.X) > MAX_POSITION || Math.Abs(player.headPosition.Y) > MAX_POSITION)
                player.headPosition = _originalHeadPosition;

            if (Math.Abs(player.bodyPosition.X) > MAX_POSITION || Math.Abs(player.bodyPosition.Y) > MAX_POSITION)
                player.bodyPosition = _originalBodyPosition;

            if (Math.Abs(player.legPosition.X) > MAX_POSITION || Math.Abs(player.legPosition.Y) > MAX_POSITION)
                player.legPosition = _originalLegPosition;

            if (Math.Abs(player.headRotation) > MAX_ROTATION)
                player.headRotation = _originalHeadRotation;

            if (Math.Abs(player.bodyRotation) > MAX_ROTATION)
                player.bodyRotation = _originalBodyRotation;

            if (Math.Abs(player.legRotation) > MAX_ROTATION)
                player.legRotation = _originalLegRotation;
        }

        /// <summary>
        /// Restores original values if an error occurs.
        /// </summary>
        private void RestoreOriginalValues(ref PlayerDrawSet drawInfo)
        {
            var player = drawInfo.drawPlayer;

            player.headPosition = _originalHeadPosition;
            player.headRotation = _originalHeadRotation;
            player.bodyPosition = _originalBodyPosition;
            player.bodyRotation = _originalBodyRotation;
            player.legPosition = _originalLegPosition;
            player.legRotation = _originalLegRotation;
        }

        /// <summary>
        /// Temporarily disables animations and schedules re-enabling them.
        /// </summary>
        private void DisableAnimationsTemporarily()
        {
            AnimationsEnabled = false;

            // Use a timer to re-enable animations after a short delay
            // This would typically use ModContent.GetInstance<YourMod>().AddCallback() or similar
            // For simplicity, we'll use a simple counter in the update method
        }

        private int _disableCounter = 0;
        public override void PostUpdate()
        {
            // If animations are disabled, count down to re-enable them
            if (!AnimationsEnabled)
            {
                _disableCounter++;
                if (_disableCounter >= 60) // 1 second
                {
                    AnimationsEnabled = true;
                    _disableCounter = 0;

                    // Reset all animations to avoid stuck animations
                    _animationController.ResetAllAnimations(Player);
                }
            }
        }

        /// <summary>
        /// Gets the animation controller.
        /// </summary>
        public AnimationController AnimationController => _animationController;

        /// <summary>
        /// Helper method to initialize an animation.
        /// </summary>
        /// <param name="uniqueID">The unique ID of the animation.</param>
        /// <returns>True if the animation was found and initialized, false otherwise.</returns>
        public bool InitializeAnimation(string uniqueID)
        {
            return _animationController.InitializeAnimation(uniqueID, Player);
        }
    }
}