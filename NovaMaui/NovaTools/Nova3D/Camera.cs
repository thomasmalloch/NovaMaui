using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using System.Numerics;
using NovaTools;
using System.Runtime.CompilerServices;

namespace NovaTools.Nova3D;

public class Camera
{
	public float x_scale_{ get; set; }
	public float y_scale_{ get; set; }
	public float screen_width_{ get; set; }
	public float screen_height_{ get; set; }
	public float fov_{ get; set; }
	public float fov_half_{ get; set; }
	public float near_{ get; set; }
	public float far_{ get; set; }
	public Vector3 position_{ get; set; }
	public float angle_{ get; set; }
	public float cos_angle_{ get; set; }
	public float sin_angle_{ get; set; }
	public Node CurrentNode { get; set; }
	public List<Plane> clipping_planes_{ get; set; }
	public Vector2 Scale => new(x_scale_, y_scale_);

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private void UpdateClippingPlanes()
	{
		float x1, x2, z1, z2;
		(z2, x2) = MathF.SinCos(fov_half_ + (MathF.PI / 2f));
		x1 = x2 * 8f;
		z1 = z2 * 8f;

		var left = NovaMathF.CreatePlane(new Vector3(x1, 0, z1), new Vector3(-(z2 - z1), 0, x2 - x1));
		(z1, x1) = MathF.SinCos(-fov_half_ + (MathF.PI / 2f));
		z2 = (z1 * 8f);
		x2 = (x1 * 8f);

		var right = NovaMathF.CreatePlane(new Vector3(x2, 0, z2), new Vector3(-(z2 - z1), 0, x2 - x1));
		var plane_near = NovaMathF.CreatePlane(new Vector3(0, 0, near_), new Vector3(0, 0, 1));
		var plane_far = NovaMathF.CreatePlane(new Vector3(0, 0, 250), new Vector3(0, 0, -1));

		//vFov = Mathf.Atan(Mathf.Tan(hFov / 2) / c.aspect) * 2;
		/*float vfov_half = atanf(tanf(fov_half_) / (screen_width_ / screen_height_));

		Plane top = Plane({ 0, 1, 1 }, { 0, -1, 1 });
		Plane bottom = Plane({ 0, -1, 1 }, { 0, 1, 1 });*/

		clipping_planes_.Clear();

		//clipping_planes_.push_back(plane_far);
		clipping_planes_.Add(right);
		clipping_planes_.Add(left);

		clipping_planes_.Add(plane_near);
	}

	public Camera(float screen_width, float screen_height)
	{
		position_ = new();
		angle_ = 0;
		(sin_angle_, cos_angle_) = MathF.SinCos(angle_);
		screen_width_ = screen_width;
		screen_height_ = screen_height;
		near_ = 0.1f;
		far_ = 1e5f;
		SetFieldOfView(MathF.PI / 2f); // 90 degrees
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public Vector2 Project(Vector3 p)
	{
		return new Vector2
			(
				0.5f * screen_width_ + x_scale_ * p.X / p.Z,
				0.5f * screen_height_ + y_scale_ * p.Y / p.Z
			);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public Vector3 InverseProject(Vector2 p, float z)
	{
		return new Vector3
			(
				(p.X - 0.5f * screen_width_) * z / x_scale_,
				(p.Y - 0.5f * screen_height_) * z / y_scale_,
				z
			);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void SetFieldOfView(float fov_rad) 
	{
		fov_ = fov_rad;
		fov_half_ = 0.5f * fov_rad;
		var scale = 1f / MathF.Tan(fov_half_);
		var aspect = screen_width_ / screen_height_;
		x_scale_ = 0.5f * screen_width_ * scale / MathF.Min(1f, aspect);
		y_scale_ = 0.5f * screen_height_ * scale / MathF.Min(1f, aspect);
		UpdateClippingPlanes();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetFieldOfView(int degrees)
	{
		SetFieldOfView(degrees * MathF.PI / 180f);
	}
}
