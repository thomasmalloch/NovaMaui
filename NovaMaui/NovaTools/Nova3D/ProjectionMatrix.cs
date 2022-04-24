using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace NovaTools.Nova3D;

public class ProjectionMatrix
{
	private Matrix4x4 Internal = new();

	public ProjectionMatrix(int width, int height, int fov_degrees, float frutstrum_near, float frutstrum_far) 
		: this(width, height, fov_degrees * 0.5f / 180.0f * MathF.PI, frutstrum_near, frutstrum_far)
	{
	}

	public ProjectionMatrix(int width, int height, float fov_rad, float frutstrum_near, float frutstrum_far)
	{
		var fov = 1.0f / MathF.Tan(fov_rad);
		var aspect = (float)height / (float)width;

		this.Internal.M11 = aspect * fov;
		this.Internal.M22 = fov;
		// TODO could be a mistake here?
		this.Internal.M33 = frutstrum_far / (frutstrum_far - frutstrum_near);
		this.Internal.M34 = 1.0f;
		this.Internal.M43 = (-frutstrum_far * frutstrum_near) / (frutstrum_far - frutstrum_near);
		this.Internal.M44 = 0.0f;
	}
}
