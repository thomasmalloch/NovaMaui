using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NovaTools.Nova3D;


public struct Slope
{
	private float M;

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public Slope(float dy, float dx)
	{
		this.M = dy / dx;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public float Interpolate(float start, float step)
	{
		return start + this.M * step;
	}
}
