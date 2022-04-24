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
    List<Vector3> SpawnPoints;
    List<Quaternion> SpawnRotations;
    int PlayerNodeIndex;
    List<Node> Nodes;
    float AmbientLight;
}
