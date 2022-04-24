using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NovaTools;
using SkiaSharp;
using NovaTools;

namespace NovaTools.Nova3D;

public class Node
{
    public SKBitmap CeilingTexture { get; set; }
    public SKBitmap FloorTexture { get; set; }
    public List<Wall> Walls;
    public List<IActor> Actors;
    public SKPoint PlaneXY;
    public SKPoint PlaneUV;
    public float FloorHeight;
    public float CeilingHeight;
}
