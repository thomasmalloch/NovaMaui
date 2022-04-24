using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NovaTools;
using SkiaSharp;
using System.Numerics;


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
        foreach (var wall in this.Camera.GetCurrentNode.Walls)
        {
            if (!wall->IsPortal())
                continue;

            if (Math::CrossProduct(wall->p2_.x - wall->p1_.x, wall->p2_.y - wall->p1_.y, camera_->GetPosition().x - wall->p1_.x, camera_->GetPosition().y - wall->p1_.y) >= 0)
                continue;

            camera_->SetCurrentNode(current_map_->nodes_[wall->node_index_]);
            break;
        }
    }

    public void DrawFrame(float delta, object input) 
    {
        this.UpdateWorker(delta, input);

        this.update
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

    
}

