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

namespace Reverie.Core.Cinematics;

public abstract class EaseFunction
{
    public static readonly EaseFunction Linear = new PolynomialEase((x) => x);

    public static readonly EaseFunction EaseQuadIn = new PolynomialEase((x) => x * x);
    public static readonly EaseFunction EaseQuadOut = new PolynomialEase((x) => 1f - EaseQuadIn.Ease(1f - x));
    public static readonly EaseFunction EaseQuadInOut = new PolynomialEase((x) => x < 0.5f ? 2f * x * x : -2f * x * x + 4f * x - 1f);

    public static readonly EaseFunction EaseCubicIn = new PolynomialEase((x) => x * x * x);
    public static readonly EaseFunction EaseCubicOut = new PolynomialEase((x) => 1f - EaseCubicIn.Ease(1f - x));
    public static readonly EaseFunction EaseCubicInOut = new PolynomialEase((x) => x < 0.5f ? 4f * x * x * x : 4f * x * x * x - 12f * x * x + 12f * x - 3f);

    public static readonly EaseFunction EaseQuarticIn = new PolynomialEase((x) => x * x * x * x);
    public static readonly EaseFunction EaseQuarticOut = new PolynomialEase((x) => 1f - EaseQuarticIn.Ease(1f - x));
    public static readonly EaseFunction EaseQuarticInOut = new PolynomialEase((x) => x < 0.5f ? 8f * x * x * x * x : -8f * x * x * x * x + 32f * x * x * x - 48f * x * x + 32f * x - 7f);

    public static readonly EaseFunction EaseQuinticIn = new PolynomialEase((x) => x * x * x * x * x);
    public static readonly EaseFunction EaseQuinticOut = new PolynomialEase((x) => 1f - EaseQuinticIn.Ease(1f - x));
    public static readonly EaseFunction EaseQuinticInOut = new PolynomialEase((x) => x < 0.5f ? 16f * x * x * x * x * x : 16f * x * x * x * x * x - 80f * x * x * x * x + 160f * x * x * x - 160f * x * x + 80f * x - 15f);

    public static readonly EaseFunction EaseCircularIn = new PolynomialEase((x) => 1f - (float)Math.Sqrt(1.0 - Math.Pow(x, 2)));
    public static readonly EaseFunction EaseCircularOut = new PolynomialEase((x) => (float)Math.Sqrt(1.0 - Math.Pow(x - 1.0, 2)));
    public static readonly EaseFunction EaseCircularInOut = new PolynomialEase((x) => x < 0.5f ? (1f - (float)Math.Sqrt(1.0 - Math.Pow(x * 2, 2))) * 0.5f : (float)((Math.Sqrt(1.0 - Math.Pow(-2 * x + 2, 2)) + 1) * 0.5));

    public virtual float Ease(float time)
    {
        throw new NotImplementedException();
    }
}

public class PolynomialEase(Func<float, float> func) : EaseFunction
{
    private readonly Func<float, float> fun = func;

    public override float Ease(float time)
    {
        return fun(time);
    }
}

public class EaseBuilder : EaseFunction
{
    private readonly List<EasePoint> points;

    public EaseBuilder()
    {
        points = [];
    }

    public void AddPoint(float x, float y, EaseFunction function)
    {
        AddPoint(new Vector2(x, y), function);
    }

    public void AddPoint(Vector2 vector, EaseFunction function)
    {
        if (vector.X < 0f)
            throw new ArgumentException("X value of point is not in valid range!");

        var newPoint = new EasePoint(vector, function);
        if (points.Count == 0)
        {
            points.Add(newPoint);
            return;
        }

        var last = points[^1];

        if (last.Point.X > vector.X)
            throw new ArgumentException("New point has an x value less than the previous point when it should be greater or equal");

        points.Add(newPoint);
    }

    public override float Ease(float time)
    {
        var prevPoint = Vector2.Zero;
        var usePoint = points[0];

        for (var i = 0; i < points.Count; i++)
        {
            usePoint = points[i];

            if (time <= usePoint.Point.X)
                break;

            prevPoint = usePoint.Point;
        }

        var dist = usePoint.Point.X - prevPoint.X;
        var progress = (time - prevPoint.X) / dist;

        if (progress > 1f)
            progress = 1f;

        return MathHelper.Lerp(prevPoint.Y, usePoint.Point.Y, usePoint.Function.Ease(progress));
    }

    private struct EasePoint(Vector2 p, EaseFunction func)
    {
        public Vector2 Point = p;
        public EaseFunction Function = func;
    }
}