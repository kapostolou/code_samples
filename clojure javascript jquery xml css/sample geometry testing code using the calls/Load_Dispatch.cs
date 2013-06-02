using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GELib.Physics.Rigid_Body_Structures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using GELib.Emitting;
using GELib.Command;
using GELib.Service;
using System.Xml;

namespace GELib.Loading
{
    //a class version of Dispatch_Pair 
    //used because the xml library needed a class for a certain function
    //while having a struct after the initialization phase would be more efficient
    class Dispatch_Pair_c
    {
       

        public Func<Geometry_Container, Geometry_Container, bool>  Coarse_Test;
        public Func<Geometry_Container, Geometry_Container, bool>  Test;
        public Func<Geometry_Container, Geometry_Container, bool> I_Test;
        public bool kill_first;
        public bool kill_second;
        public bool reverse_coarse;
        public bool reverse_test;
        public bool reverse_intersect;
    }
   
    class Load_Dispatch
    {
        public static List<Dispatch_Pair> dispatch_table = new List<Dispatch_Pair>();
        
        public static void Initialize()
        {
            XmlDocument doc = new XmlDocument();
            var file_name = @"C:\game-scripts\" + "few"+ ".xml";
            doc.Load(file_name);
            var root = doc.ChildNodes[0];
            var total = (int) Math.Sqrt(root.ChildNodes.Count);
            Dispatcher.Dispatch_array = new Dispatch_Pair[total, total];
            for (int i = 0; i < root.ChildNodes.Count; i++)
            {
                var item = Parse_Dispatch(root.ChildNodes[i]) as Dispatch_Pair_c;
                
                var add_this = new Dispatch_Pair();
                
                
                add_this.Coarse_Test = item.Coarse_Test;
                add_this.Test = item.Test;
                add_this.kill_first = item.kill_second;
                add_this.kill_second = item.kill_second;
                add_this.I_Test = item.I_Test;
                add_this.rever_test = item.reverse_test;
                add_this.rever_coarse = item.reverse_coarse;
                add_this.rever_intersect = item.reverse_intersect;
                var code1 = (int)Parse_Float(root.ChildNodes[i]["code1"]);
                var code2 = (int)Parse_Float(root.ChildNodes[i]["code2"]);
                Dispatcher.Dispatch_array[code1, code2] = add_this;
                
            }

        }


        public static Dispatch_Pair_c Parse_Dispatch(XmlNode command_node)
        {
            var ret = new Dispatch_Pair_c();
            var kill1 = Parse_Bool(command_node["kill-first"]);
            var kill2 = Parse_Bool(command_node["kill-second"]);
            var coarse = Parse_String(command_node["final-coarse"]);
            var test = Parse_String(command_node["final-test"]);
            var intersect = Parse_String(command_node["final-intersect"]);
            ret.kill_first = kill1;
            ret.kill_second = kill2;
            ret.I_Test = Parse_Intersection( intersect);
            ret.Coarse_Test = Parse_Coarse(coarse);
            ret.Test = Parse_Test(test);
            ret.reverse_coarse = Parse_Bool(command_node["reverse-coarse"]);
            ret.reverse_intersect = Parse_Bool(command_node["reverse-rigid"]);
            ret.reverse_test = Parse_Bool(command_node["reverse-rigid"]);
            return ret;
        }

        public static float Parse_Float(XmlNode e)
        {
            var time = float.Parse(e.Attributes["value"].Value, new System.Globalization.CultureInfo("en-US"));
            return time;
        }
        public static bool Parse_Bool(XmlNode e)
        {
            var time = bool.Parse(e.Attributes["value"].Value);
            return time;
        }
        public static string Parse_String(XmlNode e)
        {
            var command_ty = e.Attributes["value"].Value;
            return command_ty;
        }

        public static Func<Geometry_Container, Geometry_Container, bool> Parse_Coarse(string name)
        {
            if (name == "Nothing" || name == "NOTHING") return null;
            else
            {
                var ret = Geometry.GF_Wrappers.Get_Func("Coarse", name);
                return ret;
            }
        }

        public static Func<Geometry_Container, Geometry_Container, bool> Parse_Test(string name)
        {
            if (name == "Nothing" || name == "NOTHING") return null;
            else
            {
                var ret = Geometry.GF_Wrappers.Get_Func("Test", name);
                return ret;
            }
        }

        public static Func<Geometry_Container, Geometry_Container, bool> Parse_Intersection(string name)
        {
            if (name == "Nothing" || name == "NOTHING") return null;
            else
            {
                var ret = Geometry.GF_Wrappers.Get_Func("Intersect", name);
                return ret;
            }
        }


    }
}
