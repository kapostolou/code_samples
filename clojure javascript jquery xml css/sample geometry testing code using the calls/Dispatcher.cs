using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GELib.Emitting;
using GELib.Command;
using GELib.Service;
using GELib.Physics.Rigid_Body_Structures;

namespace GELib
{
    public struct Dispatch_Pair
    {
        public Func<Geometry_Container, Geometry_Container, bool> Coarse_Test;
        public Func<Geometry_Container, Geometry_Container, bool> Test;
        public Func<Geometry_Container, Geometry_Container, bool> I_Test;
        public bool rever_coarse;
        public bool rever_test;
        public bool rever_intersect;
        public bool kill_first;
        public bool kill_second;
    }
    public class Dispatcher
    {
        public static Dispatch_Pair[,] Dispatch_array;
    }
}