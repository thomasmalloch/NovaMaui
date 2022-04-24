using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NovaTools;
using SkiaSharp;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace NovaTools.Nova3D;

public abstract class PortalRenderingEngine : IDisposable
{
	Map current_map_;
	Camera camera_;


	int window_width_;
	int window_height_;
	int pixel_scale_;
	int width_;
	int height_;
	bool is_fullscreen_;
	float[] depth_buffer_;

	SKColor fog_colour_;
	float fog_occlusion_distance_;
	float fog_density_;

	List<Light> lights_;
	List<Light> translated_lights_;

	bool is_running_ = false;

	public PortalRenderingEngine()
	{
	}

	public void Dispose()
	{
		this.DisposeWorker();
	}

	public abstract void LoadWorker();
	public abstract void DisposeWorker();
	public abstract void UpdateWorker(float delta, object input);


	public void Setup(int width, int height, int pixel_scale)
	{
		window_width_ = width;
		window_height_ = height;
		pixel_scale_ = pixel_scale;
		width_ = window_width_ / pixel_scale_;
		height_ = window_height_ / pixel_scale_;
		depth_buffer_ = new float[width * height];

		for (int i = 0; i < width_ * height_; i++)
			depth_buffer_[i] = float.MaxValue;

		camera_ = new Camera(width_, height_);
		fog_colour_ = new SKColor(0x11, 0x11, 0x22);
		fog_occlusion_distance_ = 150;
		fog_density_ = MathF.Log(255) / MathF.Log(fog_occlusion_distance_);
	}

	public void Run()
	{
		is_running_ = true;


	}


	public void UpdateActorPositions()
	{
		// portal traversal
		foreach (var wall in this.camera_.CurrentNode.Walls)
		{
			if (!wall.IsPortal)
				continue;

			if (NovaMathF.CrossProduct(wall.P2.X - wall.P1.X, wall.P2.Y - wall.P1.Y, camera_.position_.X - wall.P1.X, camera_.position_.Y - wall.P1.Y) >= 0)
				continue;

			camera_.CurrentNode = (current_map_.Nodes[wall.NodeIndex]);
			break;
		}
	}

	private void Clear()
	{
	}

	public void RenderMinimap()
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void DrawPixel(
		SKBitmap pixels, int x, int y,
		float u, float v, SKBitmap texture,
		float z, float map_x, float map_y, float map_z)
	{
		if (depth_buffer_[x + y * width_] < z)
			return;

		SKSizeI size = pixels.Info.Size;
		int uu = Math.Clamp(0, size.Width, ((int)(u * size.Width)) % size.Width);
		int vv = Math.Clamp(0, size.Height, ((int)(v * size.Width)) % size.Width);

		var colour = texture.GetPixel(uu, vv);
		if (colour.Alpha == 0)
			return;

		depth_buffer_[x + y * width_] = z;
		pixels.SetPixel(x, y, colour);
	}

	public void DrawLine()
	{
	}

	public void DrawFrame(float delta, object input)
	{
		this.UpdateWorker(delta, input);

		this.UpdateActorPositions();

		this.Clear();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private int ClipPolygon(List<Point3D> points, int num_points)
	{
		List<Point3D> new_points = new List<Point3D>(num_points * 2);
		var poly_count = num_points;
		var clipped_count = 0;

		foreach (Plane plane in camera_.clipping_planes_)
		{
			clipped_count = 0;
			for (int i = 0; i < poly_count; i++)
			{
				var current = points[i];
				var next = points[(i + 1) % poly_count];


				var cross1 = Plane.DotCoordinate(plane, current.XYZ);
				var cross2 = Plane.DotCoordinate(plane, next.XYZ);

				if (cross1 >= 0 && cross2 >= 0)
				{
					new_points[clipped_count++] = next;
					continue;
				}
				else if (cross1 < 0 && cross2 >= 0)
				{
					var intersect = NovaMathF.Intersect(plane, current.XYZ, next.XYZ);
					var u = new Slope(next.UVW.X - current.UVW.X, next.XYZ.X - current.XYZ.X);
					var v = new Slope(next.UVW.Y - current.UVW.Y, next.XYZ.Z - current.XYZ.Z);
					var x = new Slope(next.OriginalPosition.X - current.OriginalPosition.X, next.XYZ.X - current.XYZ.X);
					var y = new Slope(next.OriginalPosition.Y - current.OriginalPosition.Y, next.XYZ.Z - current.XYZ.Z);

					Point3D intersecting_point = new();
					intersecting_point.UVW.X = u.Interpolate(current.UVW.X, intersect.X - current.XYZ.X);
					intersecting_point.UVW.Y = v.Interpolate(current.UVW.Y, intersect.Z - current.XYZ.Z);
					intersecting_point.OriginalPosition.X = x.Interpolate(current.OriginalPosition.X, intersect.X - current.XYZ.X);
					intersecting_point.OriginalPosition.Y = y.Interpolate(current.OriginalPosition.Y, intersect.Z - current.XYZ.Z);
					intersecting_point.XYZ.X = intersect.X;
					intersecting_point.XYZ.Z = intersect.Z;
					intersecting_point.XYZ.Y = current.XYZ.Y;

					new_points[clipped_count++] = intersecting_point;
					new_points[clipped_count++] = next;
				}
				else if (cross1 >= 0 && cross2 < 0)
				{
					var intersect = NovaMathF.Intersect(plane, current.XYZ, next.XYZ);
					var u = new Slope(next.UVW.X - current.UVW.X, next.XYZ.X - current.XYZ.X);
					var v = new Slope(next.UVW.Y - current.UVW.Y, next.XYZ.Z - current.XYZ.Z);
					var x = new Slope(next.OriginalPosition.X - current.OriginalPosition.X, next.XYZ.X - current.XYZ.X);
					var y = new Slope(next.OriginalPosition.Y - current.OriginalPosition.Y, next.XYZ.Z - current.XYZ.Z);

					Point3D intersecting_point = new();
					intersecting_point.UVW.X = u.Interpolate(current.UVW.X, intersect.X - current.XYZ.X);
					intersecting_point.UVW.Y = v.Interpolate(current.UVW.Y, intersect.Z - current.XYZ.Z);
					intersecting_point.OriginalPosition.X = x.Interpolate(current.OriginalPosition.X, intersect.X - current.XYZ.X);
					intersecting_point.OriginalPosition.Y = y.Interpolate(current.OriginalPosition.Y, intersect.Z - current.XYZ.Z);
					intersecting_point.XYZ.X = intersect.X;
					intersecting_point.XYZ.Z = intersect.Z;
					intersecting_point.XYZ.Y = current.XYZ.Y;

					new_points[clipped_count++] = intersecting_point;
				}
			}

			poly_count = clipped_count;
			for (var i = 0; i < poly_count; i++)
				points[i] = new_points[i];
		}

		//delete[] new_points;
		return clipped_count;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private void RasterizeVerticalSlice(SKBitmap pixels, SKBitmap texture, Point3D[] points, SKRectI portal_screen_space)
	{
		if (points[0].XYZ.Y == int.MinValue || points[1].XYZ.Y == int.MaxValue)
			return;

		int x = (int)points[0].XYZ.X;
		var u = points[0].UVW.X;
		int y1 = Math.Clamp(0, height_, (int)points[0].XYZ.Y);
		int y2 = Math.Clamp(0, height_, (int)points[1].XYZ.Y);

		var v_slope = new Slope(points[1].UVW.Y - points[0].UVW.Y, points[1].XYZ.Y - points[0].XYZ.Y);
		var z_map = new Slope(points[1].OriginalPosition.Z - points[0].OriginalPosition.Z, points[1].XYZ.Y - points[0].XYZ.Y);

		for (var y = y1; y < y2; y++)
		{
			if ((y >= portal_screen_space.Top) && (y <= (portal_screen_space.Top + portal_screen_space.Height)))
				y = portal_screen_space.Top + portal_screen_space.Height + 1;

			if ((y < y1) || (y >= y2))
				break;

			var v = v_slope.Interpolate(points[0].UVW.Y, y - points[0].XYZ.Y);
			DrawPixel(
				pixels, x, y,
				u, v, texture,
				points[0].XYZ.Z,
				points[0].OriginalPosition.X,
				points[0].OriginalPosition.Y,
				z_map.Interpolate(points[0].OriginalPosition.Z, y - points[0].XYZ.Y));

		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private void RenderPlanes(SKBitmap pixels, Node render_node/*, SKBitmap minimap*/)
	{
		// render the floor/ceiling. this uses actual 3d projection!
		// floor

		List<Point3D> plane_polygon = new(render_node.PlaneXY.Count * 2);
		for (var i = 0; i < render_node.PlaneXY.Count; i++)
		{
			plane_polygon.Add(new Point3D()
			{
				XYZ = new( render_node.PlaneXY[i].X, render_node.PlaneXY[i].Y, 0 ),
				UVW = new( render_node.PlaneUV[i].X, render_node.PlaneUV[i].Y, 0 ),
				OriginalPosition = new( render_node.PlaneXY[i].X, render_node.PlaneXY[i].Y, 0 ),
			});
		}

		// translate and rotate
		for (int i = 0; i < render_node.PlaneXY.Count; i++)
		{
			Vector3 xyz = plane_polygon[i].XYZ;
			xyz.X = plane_polygon[i].XYZ.X - camera_.position_.X;
			xyz.Y = plane_polygon[i].XYZ.Y - camera_.position_.Y;
			plane_polygon[i] = new Point3D() { XYZ = xyz, UVW = plane_polygon[i].UVW, OriginalPosition = plane_polygon[i].OriginalPosition };

			xyz = plane_polygon[i].XYZ;
			xyz.Z = plane_polygon[i].XYZ.X * camera_.cos_angle_ + plane_polygon[i].XYZ.Y * camera_.sin_angle_;
			xyz.X = plane_polygon[i].XYZ.X * camera_.sin_angle_ - plane_polygon[i].XYZ.Y * camera_.cos_angle_;
			plane_polygon[i] = new Point3D() { XYZ = xyz, UVW = plane_polygon[i].UVW, OriginalPosition = plane_polygon[i].OriginalPosition };
		}

		// clip
		 var vertices = ClipPolygon(plane_polygon, render_node.PlaneXY.Count);
		if (vertices == 0)
			return;

		// draw floor on minimap
		/*sf::Vertex tri[6];
		for (var i = 0; i < vertices - 3 + 1; i++)
		{
			tri[0] = sf::Vertex(sf::Vector2f(width_ / 8.f - plane_polygon[0].XYZ.X, height_ / 4.f - plane_polygon[0].XYZ.Z), sf::Color(255, 255, 0, 50));
			tri[1] = sf::Vertex(sf::Vector2f(width_ / 8.f - plane_polygon[1 + i].XYZ.X, height_ / 4.f - plane_polygon[1 + i].XYZ.Z), sf::Color(255, 255, 0, 50));

			tri[2] = sf::Vertex(sf::Vector2f(width_ / 8.f - plane_polygon[1 + i].XYZ.X, height_ / 4.f - plane_polygon[1 + i].XYZ.Z), sf::Color(255, 255, 0, 50));
			tri[3] = sf::Vertex(sf::Vector2f(width_ / 8.f - plane_polygon[2 + i].XYZ.X, height_ / 4.f - plane_polygon[2 + i].XYZ.Z), sf::Color(255, 255, 0, 50));

			tri[4] = sf::Vertex(sf::Vector2f(width_ / 8.f - plane_polygon[2 + i].XYZ.X, height_ / 4.f - plane_polygon[2 + i].XYZ.Z), sf::Color(255, 255, 0, 50));
			tri[5] = sf::Vertex(sf::Vector2f(width_ / 8.f - plane_polygon[0].XYZ.X, height_ / 4.f - plane_polygon[0].XYZ.Z), sf::Color(255, 255, 0, 50));
			minimap.draw(tri, 6, sf::PrimitiveType::Lines);
		}*/

		 var scale = camera_.Scale;
		 var floor = render_node.FloorHeight - camera_.position_.Z;
		 var ceiling = render_node.FloorHeight - camera_.position_.Z;
		 var half_height = 0.5f * height_;
		 var half_width = 0.5f * width_;

		List<float> ceiling_y = new();
		for (var i = 0; i < vertices; i++)
		{
			Vector3 xyz = plane_polygon[i].XYZ;
			Vector3 uvw = plane_polygon[i].UVW;
			Vector3 originalPosition = plane_polygon[i].OriginalPosition;

			// XYZ
			xyz.Y = (half_height - floor * scale.Y / plane_polygon[i].XYZ.Z) / height_;
			ceiling_y.Add((half_height - ceiling * scale.Y / plane_polygon[i].XYZ.Z) / height_);
			xyz.X = (half_width - plane_polygon[i].XYZ.X * scale.X / plane_polygon[i].XYZ.Z) / width_;
			originalPosition.Z = floor;

			// UVW
			uvw.X /= plane_polygon[i].XYZ.Z;
			uvw.Y /= plane_polygon[i].XYZ.Z;
			uvw.Z = 1f / plane_polygon[i].XYZ.Z;

			// original
			originalPosition.X /= plane_polygon[i].XYZ.Z;
			originalPosition.Y /= plane_polygon[i].XYZ.Z;

			plane_polygon[i] = new Point3D() { XYZ = xyz, UVW = uvw, OriginalPosition = originalPosition };
		}

		// render floor
		if (vertices > 0)
			RasterizePolygon(pixels, plane_polygon, vertices, render_node.FloorTexture);

		// render ceiling
		for (var i = 0; i < vertices; i++)
		{
			Vector3 xyz = plane_polygon[i].XYZ;
			Vector3 uvw = plane_polygon[i].UVW;
			Vector3 originalPosition = plane_polygon[i].OriginalPosition;

			xyz.Y = ceiling_y[i];
			originalPosition.Z = ceiling;

			plane_polygon[i] = new Point3D() { XYZ = xyz, UVW = uvw, OriginalPosition = originalPosition };
		}

		if (vertices > 0)
			RasterizePolygon(pixels, plane_polygon, vertices, render_node.CeilingTexture);
	}


	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private void RenderMap(Node render_node, Node last_node, Vector2[] normalized_bounds, SKBitmap pixels)
	{

		// render walls and minimap
		//List<Vector4> lines = new();
		foreach (var wall in render_node.Walls)
		{
			if (wall.NodeIndex == 100)
				continue;

			// transform wall relative to player
			var tx1 = wall.P1.X - camera_.position_.X;
			var ty1 = wall.P1.Y - camera_.position_.Y;
			var tx2 = wall.P2.X - camera_.position_.X;
			var ty2 = wall.P2.Y - camera_.position_.Y;

			// rotate points around the player's view
			var tz1 = tx1 * camera_.cos_angle_ + ty1 * camera_.sin_angle_;
			var tz2 = tx2 * camera_.cos_angle_ + ty2 * camera_.sin_angle_;
			tx1 = tx1 * camera_.sin_angle_ - ty1 * camera_.cos_angle_;
			tx2 = tx2 * camera_.sin_angle_ - ty2 * camera_.cos_angle_;

			// if wall fully behind us, don't render
			if (tz1 <= 0 && tz2 <= 0)
				continue;

			// wall facing the wrong way
			/*if (tx1 > tx2)
				continue;*/

			Point3D[] points = new Point3D[4]
			{
				new Point3D(){ XYZ = new Vector3(tx1, ty1, tz1), UVW = new Vector3(wall.UV1.X, wall.UV1.Y, 1f), OriginalPosition = new Vector3(wall.P1.X, wall.P1.Y, 0) },
				new Point3D(){ XYZ = new Vector3(tx2, ty2, tz2), UVW = new Vector3(wall.UV2.X, wall.UV2.Y, 1f), OriginalPosition = new Vector3(wall.P2.X, wall.P2.Y, 0)},
				new Point3D(){ XYZ = new Vector3(0, 0, 0),       UVW = new Vector3(wall.UV2.X, wall.UV2.Y, 1f), OriginalPosition = new Vector3(wall.P2.X, wall.P2.Y, 0)},
				new Point3D(){ XYZ = new Vector3(0, 0, 0),       UVW = new Vector3(wall.UV1.X, wall.UV1.Y, 1f), OriginalPosition = new Vector3(wall.P1.X, wall.P1.Y, 0)}
			};


			// draw potentially out of frustum walls on minimap
			var wall_colour = (wall.IsPortal) ? new SKColor(255, 0, 0, 25) : new SKColor(255, 255, 255, 50);
			//lines.Add(new Vector4(sf::Vector2f(width_ / 8f - points[0].Xyz.X, height_ / 4f - points[0].Xyz.Z), wall_colour));
			//lines.Add(new Vector4(sf::Vector2f(width_ / 8f - points[1].Xyz.X, height_ / 4f - points[1].Xyz.Z), wall_colour));

			// clip
			var is_needing_to_render = true;
			foreach (var plane in camera_.clipping_planes_)
			{
				// see if we need to clip

				var cross1 = Plane.DotCoordinate(plane, points[0].XYZ);
				var cross2 = Plane.DotCoordinate(plane, points[1].XYZ);

				// wall outside of plane, don't render
				if ((cross1 < 0) && (cross2 < 0))
				{
					is_needing_to_render = false;
					break;
				}

				if (cross1 < 0 || cross2 < 0)
				{
					var intersect = NovaMathF.Intersect(plane, points[0].XYZ, points[1].XYZ);
					var u = new Slope(points[1].UVW.X - points[0].UVW.X, points[1].XYZ.X - points[0].XYZ.X);
					var x = new Slope(points[1].OriginalPosition.X - points[0].OriginalPosition.X, points[1].XYZ.X - points[0].XYZ.X);
					var y = new Slope(points[1].OriginalPosition.Y - points[0].OriginalPosition.Y, points[1].XYZ.Z - points[0].XYZ.Z);
					if (cross1 < 0)
					{
						points[0].UVW.X = u.Interpolate(points[0].UVW.X, intersect.X - points[0].XYZ.X);
						points[0].OriginalPosition.X = x.Interpolate(points[0].OriginalPosition.X, intersect.X - points[0].XYZ.X);
						points[0].OriginalPosition.Y = y.Interpolate(points[0].OriginalPosition.Y, intersect.Z - points[0].XYZ.Z);
						points[0].XYZ.X = intersect.X;
						points[0].XYZ.Z = intersect.Z;
					}
					else if (cross2 < 0)
					{
						points[1].UVW.X = u.Interpolate(points[0].UVW.X, intersect.X - points[0].XYZ.X);
						points[1].OriginalPosition.X = x.Interpolate(points[0].OriginalPosition.X, intersect.X - points[0].XYZ.X);
						points[1].OriginalPosition.Y = y.Interpolate(points[0].OriginalPosition.Y, intersect.Z - points[0].XYZ.Z);
						points[1].XYZ.X = intersect.X;
						points[1].XYZ.Z = intersect.Z;
					}
				}
			}

			if (!is_needing_to_render)
				continue;

			points[3].XYZ = points[0].XYZ;
			points[2].XYZ = points[1].XYZ;

			// draw clipped walls on minimap
			/*wall_colour = (wall.IsPortal()) ? sf::Color(255, 0, 0, 50) : sf::Color::White;
			lines.emplace_back(sf::Vertex(sf::Vector2f(width_ / 8.f - points[0].Xyz.X, height_ / 4.f - points[0].Xyz.Z), wall_colour));
			lines.emplace_back(sf::Vertex(sf::Vector2f(width_ / 8.f - points[1].Xyz.X, height_ / 4.f - points[1].Xyz.Z), wall_colour));*/

			// perspective transform and normalize
			if (wall.TextureHeight == 0)
				wall.TextureHeight = 1;

			var scale = camera_.Scale;
			var floor = render_node.FloorHeight - camera_.position_.Z;
			var ceiling = render_node.CeilingHeight - camera_.position_.Z;
			var half_height = 0.5f * height_;
			var half_width = 0.5f * width_;

			points[3].UVW.Y = (ceiling - floor) / wall.TextureHeight;
			points[2].UVW.Y = (ceiling - floor) / wall.TextureHeight;

			// wall projection
			points[0].XYZ.Y = (half_height - ceiling * scale.Y / points[0].XYZ.Z) / height_;
			points[1].XYZ.Y = (half_height - ceiling * scale.Y / points[1].XYZ.Z) / height_;
			points[2].XYZ.Y = (half_height - floor * scale.Y / points[2].XYZ.Z) / height_;
			points[3].XYZ.Y = (half_height - floor * scale.Y / points[3].XYZ.Z) / height_;

			points[0].XYZ.X = (half_width - points[0].XYZ.X * scale.X / points[0].XYZ.Z) / width_;
			points[1].XYZ.X = (half_width - points[1].XYZ.X * scale.X / points[1].XYZ.Z) / width_;
			points[2].XYZ.X = (half_width - points[2].XYZ.X * scale.X / points[2].XYZ.Z) / width_;
			points[3].XYZ.X = (half_width - points[3].XYZ.X * scale.X / points[3].XYZ.Z) / width_;

			// wall uv perspective correction
			for (int i = 0; i < points.Length; i++)
			{
				points[i].UVW.X /= points[i].XYZ.Z;
				points[i].OriginalPosition.X /= points[i].XYZ.Z;
				points[i].OriginalPosition.Y /= points[i].XYZ.Z;
			}

			points[0].UVW.Z = 1f / points[0].XYZ.Z;
			points[1].UVW.Z = 1f / points[1].XYZ.Z;



			int x1 = Math.Max((int)(points[0].XYZ.X * width_), (int)(normalized_bounds[0].X * width_));
			x1 = Math.Clamp(0, width_, x1);
			int x2 = Math.Min((int)(points[1].XYZ.X * width_), (int)(normalized_bounds[1].X * width_));
			x2 = Math.Clamp(0, width_, x2);

			var z_start_inv = 1f / points[0].XYZ.Z;
			var z_end_inv = 1f / points[1].XYZ.Z;

			// check if this is a portal
			float[] portal_y_boundaries = new float[4];
			if ((wall.IsPortal) && (current_map_.Nodes[wall.NodeIndex] != last_node))
			{
				var next_node = current_map_.Nodes[wall.NodeIndex];
				var next_node_ceiling = next_node.CeilingHeight - camera_.position_.Z;
				var next_node_floor = next_node.FloorHeight - camera_.position_.Z;
				float[] next_node_y =
				{
				(half_height - next_node_ceiling * scale.Y / points[0].XYZ.Z) / height_,
				(half_height - next_node_ceiling * scale.Y / points[1].XYZ.Z) / height_,
				(half_height - next_node_floor * scale.Y / points[2].XYZ.Z) / height_,
				(half_height - next_node_floor * scale.Y / points[3].XYZ.Z) / height_
			};

				portal_y_boundaries[0] = Math.Max(next_node_y[0], points[0].XYZ.Y);
				portal_y_boundaries[1] = Math.Max(next_node_y[1], points[1].XYZ.Y);
				portal_y_boundaries[2] = Math.Min(next_node_y[2], points[2].XYZ.Y);
				portal_y_boundaries[3] = Math.Min(next_node_y[3], points[3].XYZ.Y);
				Vector2[] render_bounds =
				{
				new Vector2(points[0].XYZ.X, portal_y_boundaries[0]),
				new Vector2(points[1].XYZ.X, portal_y_boundaries[1]),
				new Vector2(points[2].XYZ.X, portal_y_boundaries[2]),
				new Vector2(points[3].XYZ.X, portal_y_boundaries[3])
			};

				RenderMap(next_node, render_node, render_bounds, pixels/*, minimap*/);
			}

			var dx = points[1].XYZ.X - points[0].XYZ.X;

			var ya = new Slope(points[1].XYZ.Y - points[0].XYZ.Y, dx);
			var yb = new Slope(points[2].XYZ.Y - points[3].XYZ.Y, dx);
			var z_slope = new Slope(points[1].XYZ.Z - points[0].XYZ.Z, dx);

			var y_top = new Slope(normalized_bounds[1].Y - normalized_bounds[0].Y, normalized_bounds[1].X - normalized_bounds[0].X);
			var y_bottom = new Slope(normalized_bounds[2].Y - normalized_bounds[3].Y, normalized_bounds[1].X - normalized_bounds[0].X);

			var w_slope = new Slope(points[1].UVW.Z - points[0].UVW.Z, dx);
			var u_slope = new Slope(points[1].UVW.X - points[0].UVW.X, dx);

			var x_map = new Slope(points[1].OriginalPosition.X - points[0].OriginalPosition.X, x2 - x1);
			var y_map = new Slope(points[1].OriginalPosition.Y - points[0].OriginalPosition.Y, x2 - x1);

			for (var x = x1; x < x2; x++)
			{
				// normalize x
				var nx = x / (float)(width_);

				// find y values
				var dnx = nx - points[0].XYZ.X;
				int y1 = (int)(ya.Interpolate(points[0].XYZ.Y, dnx) * height_);
				int y2 = (int)(yb.Interpolate(points[3].XYZ.Y, dnx) * height_);
				int y_min = (int)(y_top.Interpolate(normalized_bounds[0].Y, nx - normalized_bounds[0].X) * height_);
				int y_max = (int)(y_bottom.Interpolate(normalized_bounds[3].Y, nx - normalized_bounds[0].X) * height_);

				// perspective correct u
				var z = z_slope.Interpolate(points[0].XYZ.Z, dnx);
				var w = w_slope.Interpolate(points[0].UVW.Z, dnx);
				var u = u_slope.Interpolate(points[0].UVW.X, dnx) / w;

				var uv = new SKRect(u, 0, points[0].UVW.Y, points[3].UVW.Y);

				// fix wall tex coords
				var v = new Slope(uv.Height - uv.Top, (float)(y2 - y1));
				if (y_min > y1)
				{
					uv.Top = v.Interpolate(uv.Top, (float)(y_min - y1));
					y1 = y_min;
				}

				if (y_max < y2)
				{
					uv.Bottom = v.Interpolate(uv.Top, (float)(y_max - y1));
					y2 = y_max;
				}

				// figure out where we should be drawing
				SKRectI wall_screen_space = new(x, y1, 1, y2 - y1);

				// interpolate portal y boundaries
				int portal_y_min = (int)((portal_y_boundaries[0] + ((portal_y_boundaries[1] - portal_y_boundaries[0]) / (points[1].XYZ.X - points[0].XYZ.X)) * (nx - points[0].XYZ.X)) * height_);
				int portal_y_max = (int)((portal_y_boundaries[3] + ((portal_y_boundaries[2] - portal_y_boundaries[3]) / (points[1].XYZ.X - points[0].UVW.X)) * (nx - points[0].XYZ.X)) * height_);
				if (portal_y_min < y_min)
					portal_y_min = y_min;

				if (portal_y_max > y_max)
					portal_y_max = y_max;

				var x_origin = x_map.Interpolate(points[0].OriginalPosition.X, x - x1) / w;
				var y_origin = y_map.Interpolate(points[0].OriginalPosition.Y, x - x1) / w;

				// render wall			
				Point3D[] vertical_points =
				{
				new Point3D(){ XYZ = new((float)(x), (float)(y1), z), UVW = new (uv.Left, uv.Top, 1f ), OriginalPosition = new Vector3( x_origin, y_origin, ceiling ) },
				new Point3D(){ XYZ = new((float)(x), (float)(y2) + 2, z), UVW = new(uv.Width, uv.Height, 1f), OriginalPosition = new( x_origin, y_origin, floor ) }
			};

				RasterizeVerticalSlice(
					pixels,
					wall.Texture,
					vertical_points,
					new SKRectI(0, portal_y_min + 1, 0, portal_y_max - portal_y_min - 2));
			}
		}
	}

	private void RenderNodeActors()
	{
	}

	public void End()
	{
	}

	public void LoadMap(Map map)
	{
	}


	public Camera Camera { get; private set; }

	public void AddPlayer(IPlayer player)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private void RasterizePolygon(SKBitmap pixels, List<Point3D> vertices, int vertex_count, SKBitmap texture)
	{
		for (int i = 0; i < vertex_count - 3 + 1; i++)
		{
			Point3D[] points =
			{
				vertices[0],
				vertices[1 + i],
				vertices[2 + i],
			};

			for (int j = 0; j < 3; j++)
			{
				bool swapped = false;
				for (int k = 0; k < 3 - 1 - j; k++)
				{
					if (points[k].XYZ.Y < points[k + 1].XYZ.Y)
						continue;

					(points[k], points[k + 1]) = (points[k + 1], points[k]);
					swapped = true;
				}

				if (!swapped)
					break;
			}

			// rasterize
			int y1 = (int)(points[0].XYZ.Y * height_);
			int y2 = (int)(points[1].XYZ.Y * height_);
			int y3 = (int)(points[2].XYZ.Y * height_);

			var ax = new Slope(points[1].XYZ.X - points[0].XYZ.X, (float)(Math.Abs(y2 - y1)));
			var ax_map = new Slope(points[1].OriginalPosition.X - points[0].OriginalPosition.X, (float)(Math.Abs(y2 - y1)));
			var ay_map = new Slope(points[1].OriginalPosition.Y - points[0].OriginalPosition.Y, (float)(Math.Abs(y2 - y1)));
			var au = new Slope(points[1].UVW.X - points[0].UVW.X, (float)(Math.Abs(y2 - y1)));
			var av = new Slope(points[1].UVW.Y - points[0].UVW.Y, (float)(Math.Abs(y2 - y1)));
			var aw = new Slope(points[1].UVW.Z - points[0].UVW.Z, (float)(Math.Abs(y2 - y1)));

			var bx = new Slope(points[2].XYZ.X - points[0].XYZ.X, (float)(Math.Abs(y3 - y1)));
			var bx_map = new Slope(points[2].OriginalPosition.X - points[0].OriginalPosition.X, (float)(Math.Abs(y3 - y1)));
			var by_map = new Slope(points[2].OriginalPosition.Y - points[0].OriginalPosition.Y, (float)(Math.Abs(y3 - y1)));
			var bu = new Slope(points[2].UVW.X - points[0].UVW.X, (float)(Math.Abs(y3 - y1)));
			var bv = new Slope(points[2].UVW.Y - points[0].UVW.Y, (float)(Math.Abs(y3 - y1)));
			var bw = new Slope(points[2].UVW.Z - points[0].UVW.Z, (float)(Math.Abs(y3 - y1)));


			if ((y2 - y1) != 0)
			{
				for (int y = Math.Clamp(0, height_, y1); (y < y2) && (y < height_); y++)
				{
					int x1 = (int)(ax.Interpolate(points[0].XYZ.X, y - y1) * width_);
					int x2 = (int)(bx.Interpolate(points[0].XYZ.X, y - y1) * width_);

					var u1 = au.Interpolate(points[0].UVW.X, y - y1);
					var v1 = av.Interpolate(points[0].UVW.Y, y - y1);
					var w1 = aw.Interpolate(points[0].UVW.Z, y - y1);
					var x1_map = ax_map.Interpolate(points[0].OriginalPosition.X, y - y1);
					var y1_map = ay_map.Interpolate(points[0].OriginalPosition.Y, y - y1);

					var u2 = bu.Interpolate(points[0].UVW.X, y - y1);
					var v2 = bv.Interpolate(points[0].UVW.Y, y - y1);
					var w2 = bw.Interpolate(points[0].UVW.Z, y - y1);
					var x2_map = bx_map.Interpolate(points[0].OriginalPosition.X, y - y1);
					var y2_map = by_map.Interpolate(points[0].OriginalPosition.Y, y - y1);



					if (x1 > x2)
					{
						(x1, x2) = (x2, x1);
						(u1, u2) = (u2, u1);
						(v1, v2) = (v2, v1);
						(w1, w2) = (w2, w1);
						(x1_map, x2_map) = (x2_map, x1_map);
						(y1_map, y2_map) = (y2_map, y1_map);
					}

					var tex_u = u1;
					var tex_v = v1;
					var tex_w = w1;
					var x_map = x1_map;
					var y_map = y1_map;

					var tstep = 1.0f / ((float)(x2 - x1));
					var t = 0.0f;

					for (int x = Math.Clamp(0, width_, x1); (x < x2) && (x < width_); x++)
					{
						tex_u = (1.0f - t) * u1 + t * u2;
						tex_v = (1.0f - t) * v1 + t * v2;
						tex_w = (1.0f - t) * w1 + t * w2;
						x_map = (1.0f - t) * x1_map + t * x2_map;
						y_map = (1.0f - t) * y1_map + t * y2_map;

						 var z = 1f / tex_w;
						DrawPixel(
							pixels, x, y,
							tex_u / tex_w, tex_v / tex_w, texture,
							z,
							x_map / tex_w,
							y_map / tex_w,
							points[0].OriginalPosition.Z);

						t += tstep;
					}
				}
			}

			if ((y3 - y2) != 0)
			{
				ax = new Slope(points[2].XYZ.X - points[1].XYZ.X, (float)(Math.Abs(y3 - y2)));
				ax_map = new Slope(points[2].OriginalPosition.X - points[1].OriginalPosition.X, (float)(Math.Abs(y3 - y2)));
				ay_map = new Slope(points[2].OriginalPosition.Y - points[1].OriginalPosition.Y, (float)(Math.Abs(y3 - y2)));
				au = new Slope(points[2].UVW.X - points[1].UVW.X, (float)(Math.Abs(y3 - y2)));
				av = new Slope(points[2].UVW.Y - points[1].UVW.Y, (float)(Math.Abs(y3 - y2)));
				aw = new Slope(points[2].UVW.Z - points[1].UVW.Z, (float)(Math.Abs(y3 - y2)));

				bx = new Slope(points[2].XYZ.X - points[0].XYZ.X, (float)(Math.Abs(y3 - y1)));
				for (int y = Math.Clamp(0, height_, y2); (y < y3) && (y < height_); y++)
				{
					int x1 = (int)(ax.Interpolate(points[1].XYZ.X, y - y2) * width_);
					int x2 = (int)(bx.Interpolate(points[0].XYZ.X, y - y1) * width_);

					var u1 = au.Interpolate(points[1].UVW.X, y - y2);
					var v1 = av.Interpolate(points[1].UVW.Y, y - y2);
					var w1 = aw.Interpolate(points[1].UVW.Z, y - y2);
					var x1_map = ax_map.Interpolate(points[1].OriginalPosition.X, y - y2);
					var y1_map = ay_map.Interpolate(points[1].OriginalPosition.Y, y - y2);

					var u2 = bu.Interpolate(points[0].UVW.X, y - y1);
					var v2 = bv.Interpolate(points[0].UVW.Y, y - y1);
					var w2 = bw.Interpolate(points[0].UVW.Z, y - y1);
					var x2_map = bx_map.Interpolate(points[0].OriginalPosition.X, y - y1);
					var y2_map = by_map.Interpolate(points[0].OriginalPosition.Y, y - y1);

					if (x1 > x2)
					{
						(x1, x2) = (x2, x1);
						(u1, u2) = (u2, u1);
						(v1, v2) = (v2, v1);
						(w1, w2) = (w2, w1);
						(x1_map, x2_map) = (x2_map, x1_map);
						(y1_map, y2_map) = (y2_map, y1_map);
					}

					var tex_u = u1;
					var tex_v = v1;
					var tex_w = w1;
					var x_map = x1_map;
					var y_map = y1_map;

					var tstep = 1f / (float)(x2 - x1);
					var t = 0f;

					for (int x = Math.Clamp(0, width_, x1); (x < x2) && (x < width_); x++)
					{
						tex_u = (1.0f - t) * u1 + t * u2;
						tex_v = (1.0f - t) * v1 + t * v2;
						tex_w = (1.0f - t) * w1 + t * w2;
						x_map = (1.0f - t) * x1_map + t * x2_map;
						y_map = (1.0f - t) * y1_map + t * y2_map;

						 var z = 1f / tex_w;
						DrawPixel(
							pixels, x, y,
							tex_u / tex_w, tex_v / tex_w, texture,
							z,
							x_map / tex_w,
							y_map / tex_w,
							points[0].OriginalPosition.Z);

						t += tstep;
					}
				}
			}
		}
	}
}

