using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using SkiaSharp;
using System.Runtime.CompilerServices;

namespace NovaTools.Nova3D;

public class Light
{
	Vector3 Position;
	SKColor Color;
	float Intensity;

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	float GetDistance(Vector3 p)
	{
		return GetDistance(p.X, p.Y, p.Z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	float GetDistance(float x, float y, float z)
	{
		return MathF.Sqrt((this.Position.X - x) * (this.Position.X - x) + (this.Position.Y - y) * (this.Position.Y - y) + (this.Position.Z - z) * (this.Position.Z - z));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	float GetDistanceSquared(float x, float y, float z)
	{
		return (this.Position.X - x) * (this.Position.X - x) + (this.Position.Y - y) * (this.Position.Y - y) + (this.Position.Z - z) * (this.Position.Z - z);
	}
}
