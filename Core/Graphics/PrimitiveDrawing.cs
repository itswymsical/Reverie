/*
    * Copyright (C) 2024 Project Starlight River
    * 
    * This program is free software: you can redistribute it and/or modify
    * it under the terms of the GNU General Public License as published by
    * the Free Software Foundation, either version 3 of the License, or
    * (at your option) any later version.
    * 
    * This program is distributed in the hope that it will be useful,
    * but WITHOUT ANY WARRANTY; without even the implied warranty of
    * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    * GNU General Public License for more details.
    * 
    * You should have received a copy of the GNU General Public License
    * along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using System.Collections.Generic;
using Reverie.Core.Graphics.Interfaces;
using Reverie.Utilities;

namespace Reverie.Core.Graphics;

public class PrimitiveDrawing : HookGroup
{
    // Should not interfere with anything.
    public override void Load()
    {
        if (Main.dedServ)
            return;

        On_Main.DrawDust += DrawPrimitives;
    }

    private void DrawPrimitives(On_Main.orig_DrawDust orig, Main self)
    {
        orig(self);

        if (Main.gameMenu)
            return;

        Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

        for (var k = 0; k < Main.maxProjectiles; k++) // Projectiles.
        {
            if (Main.projectile[k].active && Main.projectile[k].ModProjectile != null && Main.projectile[k].ModProjectile is IDrawPrimitive)
                (Main.projectile[k].ModProjectile as IDrawPrimitive).DrawPrimitives();
        }

        for (var k = 0; k < Main.maxNPCs; k++) // NPCs.
        {
            if (Main.npc[k].active && Main.npc[k].ModNPC is IDrawPrimitive)
                (Main.npc[k].ModNPC as IDrawPrimitive).DrawPrimitives();
        }
    }
}

public class Primitives
{
    private readonly GraphicsDevice device;
    private ArraySegment<VertexPositionColorTexture> vertices;
    private ArraySegment<short> indices;

    public Primitives(GraphicsDevice device, int maxVertices, int maxIndices)
    {
        this.device = device;
    }

    public void Render(Effect effect)
    {
        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices.Array, vertices.Offset, vertices.Count, indices.Array, indices.Offset, indices.Count / 3);
        }
    }

    public void SetVertices(VertexPositionColorTexture[] vertices)
    {
        this.vertices = vertices;
    }

    public void SetIndices(short[] indices)
    {
        this.indices = indices;
    }

}
public interface ITrailTip
{
    int ExtraVertices { get; }

    int ExtraIndices { get; }

    void GenerateMesh(Vector2 trailTipPosition, Vector2 trailTipNormal, int startFromIndex, out VertexPositionColorTexture[] vertices, out short[] indices, TrailWidthFunction trailWidthFunction, TrailColorFunction trailColorFunction);
}

public delegate float TrailWidthFunction(float factorAlongTrail);

public delegate Color TrailColorFunction(Vector2 textureCoordinates);

public class Trail
{
    private readonly Primitives primitives;

    private readonly int maxPointCount;

    private readonly ITrailTip tip;

    private readonly TrailWidthFunction trailWidthFunction;

    private readonly TrailColorFunction trailColorFunction;

    /// <summary>
    /// Array of positions that define the trail. NOTE: Positions[Positions.Length - 1] is assumed to be the start (e.g. Projectile.Center) and Positions[0] is assumed to be the end.
    /// </summary>
    public Vector2[] Positions
    {
        get => positions;
        set
        {
            if (value.Length != maxPointCount)
            {
                throw new ArgumentException("Array of positions was a different length than the expected result!");
            }

            positions = value;
        }
    }

    private Vector2[] positions;

    /// <summary>
    /// Used in order to calculate the normal from the frontmost position, because there isn't a point after it in the original list.
    /// </summary>
    public Vector2 NextPosition { get; set; }

    private const float DEFAULT_WIDTH = 16;

    public Trail(GraphicsDevice device, int maxPointCount, ITrailTip tip, TrailWidthFunction trailWidthFunction, TrailColorFunction trailColorFunction)
    {
        this.tip = tip ?? new NoTip();

        this.maxPointCount = maxPointCount;

        this.trailWidthFunction = trailWidthFunction;

        this.trailColorFunction = trailColorFunction;

        /* A---B---C
         * |  /|  /|
         * D / E / F
         * |/  |/  |
         * G---H---I
         * 
         * Let D, E, F, etc. be the set of n points that define the trail.
         * Since each point generates 2 vertices, there are 2n vertices, plus the tip's count.
         * 
         * As for indices - in the region between 2 defining points there are 2 triangles.
         * The amount of regions in the whole trail are given by n - 1, so there are 2(n - 1) triangles for n points.
         * Finally, since each triangle is defined by 3 indices, there are 6(n - 1) indices, plus the tip's count.
         */

        primitives = new Primitives(device, maxPointCount * 2 + this.tip.ExtraVertices, 6 * (maxPointCount - 1) + this.tip.ExtraIndices);
    }

    private void GenerateMesh(out VertexPositionColorTexture[] vertices, out short[] indices, out int nextAvailableIndex)
    {
        var verticesTemp = new VertexPositionColorTexture[maxPointCount * 2];

        var indicesTemp = new short[maxPointCount * 6 - 6];

        // k = 0 indicates starting at the end of the trail (furthest from the origin of it).
        for (var k = 0; k < Positions.Length; k++)
        {
            // 1 at k = Positions.Length - 1 (start) and 0 at k = 0 (end).
            var factorAlongTrail = (float)k / (Positions.Length - 1);

            // Uses the trail width function to decide the width of the trail at this point (if no function, use 
            var width = trailWidthFunction?.Invoke(factorAlongTrail) ?? DEFAULT_WIDTH;

            var current = Positions[k];
            var next = k == Positions.Length - 1 ? Positions[^1] + (Positions[^1] - Positions[^2]) : Positions[k + 1];

            var normalToNext = (next - current).SafeNormalize(Vector2.Zero);
            var normalPerp = normalToNext.RotatedBy(MathHelper.PiOver2);

            /* A
             * |
             * B---D
             * |
             * C
             * 
             * Let B be the current point and D be the next one.
             * A and C are calculated based on the perpendicular vector to the normal from B to D, scaled by the desired width calculated earlier.
             */

            var a = current + normalPerp * width;
            var c = current - normalPerp * width;

            /* Texture coordinates are calculated such that the top-left is (0, 0) and the bottom-right is (1, 1).
             * To achieve this, we consider the Y-coordinate of A to be 0 and that of C to be 1, while the X-coordinate is just the factor along the trail.
             * This results in the point last in the trail having an X-coordinate of 0, and the first one having a Y-coordinate of 1.
             */
            var texCoordA = new Vector2(factorAlongTrail, 0);
            var texCoordC = new Vector2(factorAlongTrail, 1);

            // Calculates the color for each vertex based on its texture coordinates. This acts like a very simple shader (for more complex effects you can use the actual shader).
            var colorA = trailColorFunction?.Invoke(texCoordA) ?? Color.White;
            var colorC = trailColorFunction?.Invoke(texCoordC) ?? Color.White;

            /* 0---1---2
             * |  /|  /|
             * A / B / C
             * |/  |/  |
             * 3---4---5
             * 
             * Assuming we want vertices to be indexed in this format, where A, B, C, etc. are defining points and numbers are indices of mesh points:
             * For a given point that is k positions along the chain, we want to find its indices.
             * These indices are given by k for the above point and k + n for the below point.
             */

            verticesTemp[k] = new VertexPositionColorTexture(a.ToVector3(), colorA, texCoordA);
            verticesTemp[k + maxPointCount] = new VertexPositionColorTexture(c.ToVector3(), colorC, texCoordC);
        }

        /* Now, we have to loop through the indices to generate triangles.
         * Looping to maxPointCount - 1 brings us halfway to the end; it covers the top row (excluding the last point on the top row).
         */
        for (short k = 0; k < maxPointCount - 1; k++)
        {
            /* 0---1
             * |  /|
             * A / B
             * |/  |
             * 2---3
             * 
             * This illustration is the most basic set of points (where n = 2).
             * In this, we want to make triangles (2, 3, 1) and (1, 0, 2).
             * Generalising this, if we consider A to be k = 0 and B to be k = 1, then the indices we want are going to be (k + n, k + n + 1, k + 1) and (k + 1, k, k + n)
             */

            indicesTemp[k * 6] = (short)(k + maxPointCount);
            indicesTemp[k * 6 + 1] = (short)(k + maxPointCount + 1);
            indicesTemp[k * 6 + 2] = (short)(k + 1);
            indicesTemp[k * 6 + 3] = (short)(k + 1);
            indicesTemp[k * 6 + 4] = k;
            indicesTemp[k * 6 + 5] = (short)(k + maxPointCount);
        }

        // The next available index will be the next value after the count of points (starting at 0).
        nextAvailableIndex = verticesTemp.Length;

        vertices = verticesTemp;

        // Maybe we could use an array instead of a list for the indices, if someone figures out how to add indices to an array properly.
        indices = indicesTemp;
    }

    private void SetupMeshes()
    {
        GenerateMesh(out var mainVertices, out var mainIndices, out var nextAvailableIndex);

        var toNext = (NextPosition - Positions[^1]).SafeNormalize(Vector2.Zero);

        tip.GenerateMesh(Positions[^1], toNext, nextAvailableIndex, out var tipVertices, out var tipIndices, trailWidthFunction, trailColorFunction);

        primitives.SetVertices(mainVertices.FastUnion(tipVertices));
        primitives.SetIndices(mainIndices.FastUnion(tipIndices));
    }

    public void Render(Effect effect)
    {
        if (Positions == null)
            return;

        SetupMeshes();

        primitives.Render(effect);
    }
}

public class NoTip : ITrailTip
{
    public int ExtraVertices => 0;

    public int ExtraIndices => 0;

    public void GenerateMesh(Vector2 trailTipPosition, Vector2 trailTipNormal, int startFromIndex, out VertexPositionColorTexture[] vertices, out short[] indices, TrailWidthFunction trailWidthFunction, TrailColorFunction trailColorFunction)
    {
        vertices = Array.Empty<VertexPositionColorTexture>();
        indices = Array.Empty<short>();
    }
}

public class TriangularTip : ITrailTip
{
    private readonly float length;

    public int ExtraVertices => 3;

    public int ExtraIndices => 3;

    public TriangularTip(float length)
    {
        this.length = length;
    }

    public void GenerateMesh(Vector2 trailTipPosition, Vector2 trailTipNormal, int startFromIndex, out VertexPositionColorTexture[] vertices, out short[] indices, TrailWidthFunction trailWidthFunction, TrailColorFunction trailColorFunction)
    {
        /*     C
         *    / \
         *   /   \
         *  /     \
         * A-------B
         * 
         * This tip is arranged as the above shows.
         * Consists of a single triangle with indices (0, 1, 2) offset by the next available index.
         */

        var normalPerp = trailTipNormal.RotatedBy(MathHelper.PiOver2);

        var width = trailWidthFunction?.Invoke(1) ?? 1;
        var a = trailTipPosition + normalPerp * width;
        var b = trailTipPosition - normalPerp * width;
        var c = trailTipPosition + trailTipNormal * length;

        var texCoordA = Vector2.UnitX;
        var texCoordB = Vector2.One;
        var texCoordC = new Vector2(1, 0.5f);//this fixes the texture being skewed off to the side

        var colorA = trailColorFunction?.Invoke(texCoordA) ?? Color.White;
        var colorB = trailColorFunction?.Invoke(texCoordB) ?? Color.White;
        var colorC = trailColorFunction?.Invoke(texCoordC) ?? Color.White;

        vertices = new VertexPositionColorTexture[]
        {
            new(a.ToVector3(), colorA, texCoordA),
            new(b.ToVector3(), colorB, texCoordB),
            new(c.ToVector3(), colorC, texCoordC)
        };

        indices = new short[]
        {
            (short)startFromIndex,
            (short)(startFromIndex + 1),
            (short)(startFromIndex + 2)
        };
    }
}

// Note: Every vertex in this tip is drawn twice, but the performance impact from this would be very little
public class RoundedTip : ITrailTip
{
    // TriCount is the amount of tris the curve should have, higher means a better circle approximation. (Keep in mind each tri is drawn twice)
    private readonly int triCount;

    // The edge vextex count is count * 2 + 1, but one extra is added for the center, and there is one extra hidden vertex.
    public int ExtraVertices => triCount * 2 + 3;

    public int ExtraIndices => triCount * 2 * 3 + 5;

    public RoundedTip(int triCount = 2)//amount of tris
    {
        this.triCount = triCount;

        if (triCount < 2)
            throw new ArgumentException($"Parameter {nameof(triCount)} cannot be less than 2.");
    }

    public void GenerateMesh(Vector2 trailTipPosition, Vector2 trailTipNormal, int startFromIndex, out VertexPositionColorTexture[] vertices, out short[] indices, TrailWidthFunction trailWidthFunction, TrailColorFunction trailColorFunction)
    {
        /*   C---D
         *  / \ / \
         * B---A---E (first layer)
         * 
         *   H---G
         *  / \ / \
         * I---A---F (second layer)
         * 
         * This tip attempts to approximate a semicircle as shown.
         * Consists of a fan of triangles which share a common center (A).
         * The higher the tri count, the more points there are.
         * Point E and F are ontop of eachother to prevent a visual seam.
         */

        /// We want an array of vertices the size of the accuracy amount plus the center.
        vertices = new VertexPositionColorTexture[ExtraVertices];

        var fanCenterTexCoord = new Vector2(1, 0.5f);

        vertices[0] = new VertexPositionColorTexture(trailTipPosition.ToVector3(), (trailColorFunction?.Invoke(fanCenterTexCoord) ?? Color.White) * 0.75f, fanCenterTexCoord);

        var indicesTemp = new List<short>();

        for (var k = 0; k <= triCount; k++)
        {
            // Referring to the illustration: 0 is point B, 1 is point E, any other value represent the rotation factor of points in between.
            var rotationFactor = k / (float)triCount;

            // Rotates by pi/2 - (factor * pi) so that when the factor is 0 we get B and when it is 1 we get E.
            var angle = MathHelper.PiOver2 - rotationFactor * MathHelper.Pi;

            var circlePoint = trailTipPosition + trailTipNormal.RotatedBy(angle) * (trailWidthFunction?.Invoke(1) ?? 1);

            // Handily, the rotation factor can also be used as a texture coordinate because it is a measure of how far around the tip a point is.
            var circleTexCoord = new Vector2(rotationFactor, 1);

            // The transparency must be changed a bit so it looks right when overlapped
            var circlePointColor = (trailColorFunction?.Invoke(new Vector2(1, 0)) ?? Color.White) * rotationFactor * 0.85f;

            vertices[k + 1] = new VertexPositionColorTexture(circlePoint.ToVector3(), circlePointColor, circleTexCoord);

            //if (k == triCount)//leftover and not needed
            //{
            //    continue;
            //}

            var tri = new short[]
            {
                /* Because this is a fan, we want all triangles to share a common point. This is represented by index 0 offset to the next available index.
                 * The other indices are just pairs of points around the fan. The vertex k points along the circle is just index k + 1, followed by k + 2 at the next one along.
                 * The reason these are offset by 1 is because index 0 is taken by the fan center.
                 */

                //before the fix, I believe these being in the wrong order was what prevented it from drawing
                (short)startFromIndex,
                (short)(startFromIndex + k + 2),
                (short)(startFromIndex + k + 1)
            };

            indicesTemp.AddRange(tri);
        }

        // These 2 forloops overlap so that 2 points share the same location, this hidden point hides a tri that acts as a transition from one UV to another
        for (var k = triCount + 1; k <= triCount * 2 + 1; k++)
        {
            // Referring to the illustration: triCount + 1 is point F, 1 is point I, any other value represent the rotation factor of points in between.
            var rotationFactor = (k - 1) / (float)triCount - 1;

            // Rotates by pi/2 - (factor * pi) so that when the factor is 0 we get B and when it is 1 we get E.
            var angle = MathHelper.PiOver2 - rotationFactor * MathHelper.Pi;

            var circlePoint = trailTipPosition + trailTipNormal.RotatedBy(-angle) * (trailWidthFunction?.Invoke(1) ?? 1);

            // Handily, the rotation factor can also be used as a texture coordinate because it is a measure of how far around the tip a point is.
            var circleTexCoord = new Vector2(rotationFactor, 0);

            // The transparency must be changed a bit so it looks right when overlapped
            var circlePointColor = (trailColorFunction?.Invoke(new Vector2(1, 0)) ?? Color.White) * rotationFactor * 0.75f;

            vertices[k + 1] = new VertexPositionColorTexture(circlePoint.ToVector3(), circlePointColor, circleTexCoord);

            // Skip last point, since there is no point to pair with it.
            if (k == triCount * 2 + 1)
                continue;

            var tri = new short[]
            {
                /* Because this is a fan, we want all triangles to share a common point. This is represented by index 0 offset to the next available index.
                 * The other indices are just pairs of points around the fan. The vertex k points along the circle is just index k + 1, followed by k + 2 at the next one along.
                 * The reason these are offset by 1 is because index 0 is taken by the fan center.
                 */

                //The order of the indices is reversed since the direction is backwards
                (short)startFromIndex,
                (short)(startFromIndex + k + 1),
                (short)(startFromIndex + k + 2)
            };

            indicesTemp.AddRange(tri);
        }

        indices = indicesTemp.ToArray();
    }
}