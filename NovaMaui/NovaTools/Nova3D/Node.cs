using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NovaTools;
using SkiaSharp;
using NovaTools;
using System.Numerics;

namespace NovaTools.Nova3D;

public class Node
{
    public SKBitmap CeilingTexture { get; set; }
    public SKBitmap FloorTexture { get; set; }
    public List<Wall> Walls { get; set; }
    public List<IActor> Actors { get; set; }
    public List<Vector2> PlaneXY { get; set; }
    public List<Vector2> PlaneUV { get; set; }
    public float FloorHeight { get; set; }
    public float CeilingHeight { get; set; }
}
