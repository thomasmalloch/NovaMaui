using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using NovaTools;

namespace NovaTools.Nova3D;

public class Map
{
    public List<Vector3> SpawnPoints { get; set; } = new();
    public List<float> SpawnRotations { get; set; } = new();
    public int PlayerNodeIndex { get; set; }
    public List<Node> Nodes { get; set; } = new();
    public float AmbientLight { get; set; }
}
