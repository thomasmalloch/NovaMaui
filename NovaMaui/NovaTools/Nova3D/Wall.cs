using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace NovaTools.Nova3D;

public class Wall
{
    public Vector2 P1 { get; set; }
    public Vector2 P2 { get; set; }
    public Vector2 UV1 { get; set; }
    public Vector2 UV2 { get; set; }
    public SKColor Colour { get; set; }
    public SKBitmap Texture { get; set; }
    public float TextureHeight { get; set; }
    public int NodeIndex { get; set; }

    public bool IsPortal => (this.NodeIndex >= 0);
}
