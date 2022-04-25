using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace NovaTools.Nova3D;

public class Wall
{
    public Vector2 P1 = new();
    public Vector2 P2 = new();
    public Vector2 UV1 = new();
    public Vector2 UV2 = new();
    public SKColor Colour { get; set; } = SKColors.White;
    public SKBitmap Texture { get; set; } = null;
    public float TextureHeight { get; set; } = 1;
    public int NodeIndex { get; set; } = -1;
    
    public bool IsPortal => (this.NodeIndex >= 0);

    public Wall() 
    {
    }

    public Wall(float x1, float y1, float x2, float y2, SKColor colour) 
    {
        this.P1.X = x1;
        this.P1.Y = y1;
        this.P2.X = x2;
        this.P2.Y = y2;
        this.Colour = colour;
    }

    public Wall(float x1, float y1, float x2, float y2, int nodeIndex)
    {
        this.P1.X = x1;
        this.P1.Y = y1;
        this.P2.X = x2;
        this.P2.Y = y2;
        this.Colour = SKColors.Red;
        this.NodeIndex = nodeIndex;
    }
}
