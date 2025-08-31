namespace Reverie.Content.Menus;
public partial class IllustriousMenu
{
    private class GalaxyObject
    {
        public Vector2 Position;
        public float Scale;
        public float Rotation;
        public float RotationSpeed;
        public float Alpha;
        public float Time;
        public Color TintColor;
        public float FadeIn;

        // Spiral arm data
        private Vector2[] spiralPoints;
        private float[] spiralAlphas;
        private const int SPIRAL_SEGMENTS = 24;
        private const int SPIRAL_ARMS = 3;

        public GalaxyObject(Vector2 position, float scale)
        {
            Position = position;
            Scale = scale * Main.rand.NextFloat(0.5f, 0.8f);
            Rotation = Main.rand.NextFloat(0, MathHelper.TwoPi);
            RotationSpeed = Main.rand.NextFloat(0.0002f, 0.0008f) * (Main.rand.NextBool() ? 1 : -1);
            Alpha = 0f;
            FadeIn = 1f;
            Time = 0f;

            // Random galaxy colors - purple, blue, teal variations
            TintColor = Main.rand.Next(4) switch
            {
                0 => new Color(0.6f, 0.3f, 1.0f),   // Purple
                1 => new Color(0.3f, 0.6f, 1.0f),   // Blue  
                2 => new Color(0.4f, 0.8f, 0.9f),   // Teal
                _ => new Color(0.8f, 0.4f, 1.0f)    // Pink
            };

            GenerateSpiralArms();
        }

        private void GenerateSpiralArms()
        {
            spiralPoints = new Vector2[SPIRAL_SEGMENTS * SPIRAL_ARMS];
            spiralAlphas = new float[SPIRAL_SEGMENTS * SPIRAL_ARMS];

            for (int arm = 0; arm < SPIRAL_ARMS; arm++)
            {
                float armOffset = (arm / (float)SPIRAL_ARMS) * MathHelper.TwoPi;

                for (int i = 0; i < SPIRAL_SEGMENTS; i++)
                {
                    int index = arm * SPIRAL_SEGMENTS + i;
                    float t = i / (float)SPIRAL_SEGMENTS;

                    // Spiral equation: radius increases with angle
                    float angle = armOffset + t * MathHelper.TwoPi * 2f; // 2 full rotations
                    float radius = t * Scale * 80f; // Max radius based on scale

                    // Create spiral shape
                    float spiralAngle = angle + radius * 0.02f;
                    spiralPoints[index] = new Vector2(
                        (float)Math.Cos(spiralAngle) * radius,
                        (float)Math.Sin(spiralAngle) * radius
                    );

                    // Fade towards outer edges
                    spiralAlphas[index] = 1f - (t * t); // Quadratic falloff
                }
            }
        }

        public void Update()
        {
            Time += 1f / 60f;
            Rotation += RotationSpeed;

            // Fade in effect
            if (FadeIn > 0f)
            {
                FadeIn -= 0.01f;
                Alpha = 1f - FadeIn;
            }
            else
            {
                Alpha = 1f;
            }

            // Slowly regenerate spiral arms for animation
            if (Main.GameUpdateCount % 30 == 0) // Every half second
            {
                GenerateSpiralArms();
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D glowTexture, Texture2D starTexture)
        {
            if (Alpha <= 0f) return;

            Matrix rotationMatrix = Matrix.CreateRotationZ(Rotation);

            // Draw central core (bright center)
            float coreScale = Scale * 0.8f;
            spriteBatch.Draw(
                glowTexture,
                Position,
                null,
                TintColor * Alpha * 0.9f,
                Rotation,
                new Vector2(glowTexture.Width / 2, glowTexture.Height / 2),
                coreScale,
                SpriteEffects.None,
                0f
            );

            // Draw spiral arms using multiple small sprites
            for (int i = 0; i < spiralPoints.Length; i += 2) // Skip every other point for performance
            {
                Vector2 rotatedPoint = Vector2.Transform(spiralPoints[i], rotationMatrix);
                Vector2 worldPos = Position + rotatedPoint;

                // Skip if off-screen (basic culling)
                if (worldPos.X < -50 || worldPos.X > Main.screenWidth + 50 ||
                    worldPos.Y < -50 || worldPos.Y > Main.screenHeight + 50)
                    continue;

                float armAlpha = spiralAlphas[i] * Alpha * 0.6f;
                float armScale = Scale * 0.15f * (spiralAlphas[i] + 0.2f);

                // Add some spiral animation
                float timeOffset = Time * 0.5f + (i * 0.1f);
                armAlpha *= (0.7f + 0.3f * (float)Math.Sin(timeOffset));

                spriteBatch.Draw(
                    starTexture,
                    worldPos,
                    null,
                    TintColor * armAlpha,
                    Rotation + (i * 0.2f),
                    new Vector2(starTexture.Width / 2, starTexture.Height / 2),
                    armScale,
                    SpriteEffects.None,
                    0f
                );
            }

            // Draw outer glow halo
            float haloScale = Scale * 1.5f;
            spriteBatch.Draw(
                glowTexture,
                Position,
                null,
                TintColor * Alpha * 0.3f,
                Rotation * 0.5f,
                new Vector2(glowTexture.Width / 2, glowTexture.Height / 2),
                haloScale,
                SpriteEffects.None,
                0f
            );
        }

        public bool ShouldRemove()
        {
            // Remove if moved too far off screen
            return Position.X < -200 || Position.X > Main.screenWidth + 200 ||
                   Position.Y < -200 || Position.Y > Main.screenHeight + 200;
        }
    }
}