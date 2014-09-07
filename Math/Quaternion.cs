using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

namespace SharpRift.Math
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Quaternion
    {
        public float X;
        public float Y;
        public float Z;
        public float W;
    }
}
