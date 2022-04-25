using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaMaui
{
    public class Input
    {
        public volatile bool Forward = false;
        public volatile bool StrafeLeft = false;
        public volatile bool StrafeRight = false;
        public volatile bool Back = false;
        public volatile bool RotateLeft = false;
        public volatile bool RotateRight = false;
        public volatile bool Action = false;
    }
}
