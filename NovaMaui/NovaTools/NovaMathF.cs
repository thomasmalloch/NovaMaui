using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;

namespace NovaTools;

public static class NovaMathF
{
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static float Clamp(float min, float max, float num)
	{
		if (num < min)
			return min;
		if (num > max)
			return max;

		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float CrossProduct(float x1, float y1, float x2, float y2)
	{
		return x1 * y2 - y1 * x2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DotProduct(float x1, float y1, float x2, float y2)
	{
		return x1 * x2 + y1 * y2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DotProduct(float x1, float y1, float z1, float x2, float y2, float z2)
	{
		return x1 * x2 + y1 * y2 + z1 * z2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DotProduct(Vector3 a, Vector3 b)
	{
		return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static Vector3 Intersect(Plane plane, Vector3 p1, Vector3 p2) 
	{
		var ad = Vector3.Dot(p1, plane.Normal);
		var bd = Vector3.Dot(p2, plane.Normal);
		var t = (-plane.D - ad) / (bd - ad);
		return (p2 - p1) * t + p1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static Vector2 Intersect(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
	{
		var d = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
		// If d is zero, there is no intersection
		if (d == 0)
		{
			return new Vector2
			{
				X = 0,
				Y = 0
			};
		}		

		// Get the x and y
		var pre = (x1 * y2 - y1 * x2);
		var post = (x3 * y4 - y3 * x4);
		var x = (pre * (x3 - x4) - (x1 - x2) * post) / d;
		var y = (pre * (y3 - y4) - (y1 - y2) * post) / d;

		// Return the point of intersection
		return new Vector2
		{
			X = 0,
			Y = 0
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static Plane CreatePlane(Vector3 point, Vector3 normal)
	{		
		normal = Vector3.Normalize(normal);
		float a = normal.X;
		float b = normal.Y;
		float c = normal.Z;		
		float d = -Vector3.Dot(point, normal);		
		return new Plane(a, b, c, d);
	}
}
