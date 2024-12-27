using Microsoft.Xna.Framework;
using Reverie.Core.Abstraction;
using System;
using Terraria;
using Terraria.Graphics.CameraModifiers;
using Terraria.ModLoader;

namespace Reverie.Core.Mechanics
{
    public class CameraSystem : ModSystem
    {
        public static int Shake = 0;
        private static PanModifier PanModifier = new();
        private static MoveModifier MoveModifier = new();


        public override void PostUpdateEverything()
        {
            PanModifier.PassiveUpdate();
            MoveModifier.PassiveUpdate();
        }

        public override void ModifyScreenPosition()
        {
            if (PanModifier.TotalDuration > 0 && PanModifier.PrimaryTarget != Vector2.Zero)
                Main.instance.CameraModifiers.Add(PanModifier);

            if (MoveModifier.MovementDuration > 0 && MoveModifier.Target != Vector2.Zero)
                Main.instance.CameraModifiers.Add(MoveModifier);

            if (Shake > 0)
            {
                float mult = 1f; // Adjust this value to control the strength of the screen shake
                Main.instance.CameraModifiers.Add(new PunchCameraModifier(Main.LocalPlayer.position, Main.rand.NextFloat(3.14f).ToRotationVector2(), Shake * mult, 15f, 30, 2000, "Reverie Shake"));
                Shake--;
            }
        }

        public static void DoPanAnimation(int duration, Vector2 target, Vector2 secondaryTarget = default, bool useOffsetOrigin = false, Func<Vector2, Vector2, float, Vector2> easeIn = null, Func<Vector2, Vector2, float, Vector2> easeOut = null, Func<Vector2, Vector2, float, Vector2> easePan = null)
        {
            PanModifier.TotalDuration = duration;
            PanModifier.PrimaryTarget = target;
            PanModifier.SecondaryTarget = secondaryTarget;
            PanModifier.UseOffsetOrigin = useOffsetOrigin; // Set the UseOffsetOrigin property

            PanModifier.EaseInFunction = easeIn ?? Vector2.SmoothStep;
            PanModifier.EaseOutFunction = easeOut ?? Vector2.SmoothStep;
            PanModifier.PanFunction = easePan ?? Vector2.Lerp;
        }

        public static void MoveCameraOut(int duration, Vector2 target, Func<Vector2, Vector2, float, Vector2> ease = null)
        {
            MoveModifier.Timer = 0;
            MoveModifier.MovementDuration = duration;
            MoveModifier.Target = target;
            MoveModifier.Returning = false;

            MoveModifier.EaseFunction = ease ?? Vector2.SmoothStep;
        }

        public static void ReturnCamera(int duration, Func<Vector2, Vector2, float, Vector2> ease = null)
        {
            MoveModifier.Timer = 0;
            MoveModifier.MovementDuration = duration;
            MoveModifier.Returning = true;

            MoveModifier.EaseFunction = ease ?? Vector2.SmoothStep;
        }

        public static void Reset()
        {
            Shake = 0;
            PanModifier.Reset();
            MoveModifier.Reset();
        }

        public override void OnWorldLoad()
        {
            Reset();
        }

        public override void Unload()
        {
            PanModifier = null;
            MoveModifier = null;
        }
    }
}