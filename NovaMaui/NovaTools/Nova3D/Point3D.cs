using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace NovaTools.Nova3D;

public struct Point3D
{
    public Vector3 XYZ = new();
    public Vector3 UVW = new();
    public Vector3 OriginalPosition = new();

    public Point3D() 
    {
    }
}
