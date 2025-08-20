using Reverie.Core.Graphics;
using Reverie.Core.Graphics.Interfaces;
using System;
using System.Collections.Generic;

namespace Reverie.Core.Tiles.Projectiles.Actors;

public abstract class StickyProjectileActor : ModProjectile, IDrawPrimitive
{
    protected NPC Target => Main.npc[(int)Projectile.ai[1]];

    protected bool stickToNPC;
    protected bool stickToTile;

    protected bool stickingToNPC;
    protected bool stickingToTile;

    private Vector2 offset;

    private float oldRotation;
    public abstract int MaxStickies { get; }
    private List<Vector2> cache;
    private Trail trail;
    private Trail trail2;
    private Color color = new(60, 100, 255);
    private readonly Vector2 Size = new(46, 50);

    public override void AI()
    {

        if (stickingToNPC)
        {
            if (Target.active && !Target.dontTakeDamage)
            {
                Projectile.tileCollide = false;

                Projectile.Center = Target.Center - offset;

                Projectile.gfxOffY = Target.gfxOffY;
            }
            else
            {
                Projectile.Kill();
            }
        }

        if (stickingToTile || stickingToNPC)
            Projectile.rotation = oldRotation;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (!stickingToNPC && !stickingToTile && stickToNPC)
        {
            Projectile.ai[1] = target.whoAmI;

            oldRotation = Projectile.rotation;

            offset = target.Center - Projectile.Center + Projectile.velocity * 0.75f;

            stickingToNPC = true;

            Projectile.netUpdate = true;
        }
        else
        {
            RemoveStackProjectiles();
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (!stickingToTile && !stickingToNPC && stickToTile)
        {
            oldRotation = Projectile.rotation;

            Projectile.velocity = Vector2.Zero;

            stickingToTile = true;
        }

        return false;
    }

    protected void RemoveStackProjectiles()
    {
        var sticking = new Point[MaxStickies];
        var index = 0;

        for (var i = 0; i < Main.maxProjectiles; i++)
        {
            var currentProjectile = Main.projectile[i];

            if (i != Projectile.whoAmI
                && currentProjectile.active
                && currentProjectile.owner == Main.myPlayer
                && currentProjectile.type == Projectile.type
                && currentProjectile.ai[0] == 1f
                && currentProjectile.ai[1] == Target.whoAmI
            )
            {
                sticking[index++] = new Point(i, currentProjectile.timeLeft);

                if (index >= sticking.Length)
                    break;
            }
        }

        if (index >= sticking.Length)
        {
            var oldIndex = 0;

            for (var i = 1; i < sticking.Length; i++)
                if (sticking[i].Y < sticking[oldIndex].Y)
                    oldIndex = i;

            Main.projectile[sticking[oldIndex].X].Kill();
        }
    }

    private void ManageCaches()
    {
        var player = Main.LocalPlayer;
        var pos = player.Center + player.DirectionTo(Projectile.Center) * (Size.Length() * Main.rand.NextFloat(0.5f, 1.1f)) + Main.rand.NextVector2Unit() * Main.rand.NextFloat(1.0f, 4.0f);

        if (cache == null)
        {
            cache = [];

            for (var i = 0; i < 15; i++)
            {
                cache.Add(pos);
            }
        }

        cache.Add(pos);

        while (cache.Count > 15)
        {
            cache.RemoveAt(0);
        }
    }

    private void ManageTrail()
    {
        var player = Main.LocalPlayer;
        var pos = player.Center + player.DirectionTo(Projectile.Center) * (Size.Length() * Main.rand.NextFloat(0.5f, 1.1f)) + Main.rand.NextVector2Unit() * Main.rand.NextFloat(1.0f, 4.0f);

        trail ??= new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(5), factor => factor * 16, factor =>
        {
            if (factor.X >= 0.98f)
                return Color.White * 0;
            return new Color(color.R, color.G, color.B) * 0.4f * (float)Math.Pow(factor.X, 2) * (float)Math.Sin(Projectile.timeLeft / 150f * 4);
        });
        trail.Positions = [.. cache];

        trail2 ??= new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(5), factor => factor * 16, factor =>
        {
            if (factor.X >= 0.98f)
                return Color.White * 0;
            return new Color(color.R, color.G, color.B) * 0.4f * (float)Math.Pow(factor.X, 2) * (float)Math.Sin(Projectile.timeLeft / 150f * 4);
        });
        trail2.Positions = [.. cache];

        trail.NextPosition = pos + Projectile.velocity;
        trail2.NextPosition = pos + Projectile.velocity;
    }

    public void DrawPrimitives()
    {

    }
}
