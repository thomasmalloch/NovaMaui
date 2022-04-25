using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NovaTools;
using NovaTools.Nova3D;
using SkiaSharp;
using System.Numerics;

namespace NovaMaui;

public class EngineTest : PortalRenderingEngine
{
    private NovaTools.Nova3D.Map Map;
    int player_height_ = 16;

    public override void DisposeWorker()
    {        
    }

    public override void LoadWorker()
    {
        Wall w1 = new (-10, 32, -10, -32, SKColors.White);
        Wall w2 = new (10, 32, 10, -32, SKColors.White);
        Wall w3 = new (-10, -32, -10, -64, SKColors.White);
        Wall w4 = new (10, -32, 10, -64, SKColors.White);

        Node n = new ();
        n.Walls.Add(w1);
        n.Walls.Add(w2);
        n.Walls.Add(w3);
        n.Walls.Add(w4);
        n.FloorHeight = 0;
        n.CeilingHeight = 32;

        this.Map = new NovaTools.Nova3D.Map();
        this.Map.Nodes.Add(n);
        this.Map.SpawnRotations.Add(MathF.PI);
        this.Map.SpawnPoints.Add(new Vector3(16, 0, 0));

        this.LoadMap(this.Map);
        
        this.Camera.CurrentNode = this.Map.Nodes[this.Map.PlayerNodeIndex];
        this.Camera.position_ = new Vector3 ( this.Map.SpawnPoints[0].X, this.Map.SpawnPoints[0].Y, this.Map.SpawnPoints[0].Z + player_height_);
        this.Camera.SetFieldOfView(MathF.PI / 2f);
        this.Camera.SetAngle(this.Map.SpawnRotations[0] + 0.001f);
    }

    public override void UpdateWorker(float delta, object o)
    {
        if (o is not Input input)
            return;

        var forward = (0.02f * delta);
        var angle = (0.001f * delta);
        if (input.RotateLeft)
        {
            var f = this.Camera.angle_;
            f -= angle;
            this.Camera.SetAngle(f);
        }

        if (input.RotateRight)
        {
            var f = this.Camera.angle_;
            f += angle;
            this.Camera.SetAngle(f);
        }

        Vector3 m = Vector3.Zero;
        bool isMovementPressed = false;
        if (input.Forward) 
        {
            m.X += this.Camera.cos_angle_ * forward;
            m.Y += this.Camera.sin_angle_ * forward;
            isMovementPressed = true;
        }

        if (input.Back) 
        {
            m.X -= this.Camera.cos_angle_ * forward;
            m.Y -= this.Camera.sin_angle_ * forward;
            isMovementPressed = true;
        }

        if (input.StrafeRight) 
        {
            (float sin, float cos) = MathF.SinCos(this.Camera.angle_ + MathF.PI / 2f);
            m.X += cos * forward;
            m.Y += sin * forward;
            isMovementPressed = true;
        }

        if (input.StrafeLeft)
        {
            (float sin, float cos) = MathF.SinCos(this.Camera.angle_ + MathF.PI / 2f);
            m.X -= cos * forward;
            m.Y -= sin * forward;
            isMovementPressed = true;
        }

        if (isMovementPressed)
        {
            //this->player_->position_ = sf::Vector3f(this->player_->position_.x + m.x, this->player_->position_.y + m.y, this->player_->position_.z);
            var postion = this.Camera.position_ + m;
            this.Camera.position_ = postion;
        }
    }
}
