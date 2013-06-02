using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using GELib.Geometry.Geometric_Objects;

namespace GELib
{
    class Minkowski
    {

        public static Minkowski_Description Minkowskize_OBB_In_OBB(OBB obb1, OBB obb2)
        {
            var ret = new Minkowski_Description();
            //var Ta
            var tmp = obb1.Position - obb2.Position;

            var res = Vector3.Transform(tmp, obb2.TOrientation);
            ret.Position_LM = res;
            var res2 = new Vector4();
            var axisxinnewcords = Vector3.Transform(obb1.Axis(0), obb2.TOrientation);
            var axisyinnewcords = Vector3.Transform(obb1.Axis(1), obb2.TOrientation);

            var xradius = obb1.HalfAxisWidth.X * Math.Abs(axisxinnewcords.X) + obb1.HalfAxisWidth.Y * Math.Abs(axisyinnewcords.X);
            var yradius = obb1.HalfAxisWidth.X * Math.Abs(axisxinnewcords.Y) + obb1.HalfAxisWidth.Y * Math.Abs(axisyinnewcords.Y);


            res2.X = xradius;
            res2.Y = yradius;
            res2.Z = 0;
            ret.HalfAxis = res2;


            return ret;

        }

        public static Minkowski_Description Minkowskize_Sphere_In_OBB(Sphere sph, OBB obb)
        {
            var ret = new Minkowski_Description();
            //var Ta
            ret.HalfAxis.X = sph.Radius;
            ret.Position_LM = obb.Point_W_To_L(sph.Position);

            return ret;



        }

        public static Minkowski_Description Minkowskize_Triangle_In_OBB(Triangle trig, OBB obb)
        {
            var ret = new Minkowski_Description();
            ret.HalfAxis.X = trig.Extreme_In(obb.Normal(0)).dist_in_dir_w;
            ret.HalfAxis.Y = trig.Extreme_In(obb.Normal(1)).dist_in_dir_w;
            ret.HalfAxis.Z = trig.Extreme_In(obb.Normal(2)).dist_in_dir_w;
            ret.HalfAxis.W = trig.Extreme_In(obb.Normal(3)).dist_in_dir_w;
            ret.Position_LM = obb.Point_W_To_L(trig.Bary_center);
            return ret;


        }

        public static Minkowski_Description Minkowskize_OBB_In_Triangle(OBB obb, Triangle trig)
        {

            var ret = new Minkowski_Description();
            ret.HalfAxis.X = Math.Abs(obb.Extreme_In(trig.Normals[0]).dist_in_dir_w);
            ret.HalfAxis.Y = Math.Abs(obb.Extreme_In(trig.Normals[1]).dist_in_dir_w);
            ret.HalfAxis.Z = Math.Abs(obb.Extreme_In(trig.Normals[2]).dist_in_dir_w);
            ret.Position_LM = obb.Position - trig.Bary_center;
            return ret;

            //var ret = new Minkowski_Form_3();
            ////var Ta
            //var tmp = obb1.Position - obb2.Position;

            //var res = Vector3.Transform(tmp, obb2.TOrientation);
            //ret.Position = res;
            //var res2 = new Vector3();
            //var axisxinnewcords = Vector3.Transform(obb1.Axis(0), obb2.TOrientation);
            //var axisyinnewcords = Vector3.Transform(obb1.Axis(1), obb2.TOrientation);

            //var xradius = obb1.HalfAxisWidth.X * Math.Abs(axisxinnewcords.X) + obb1.HalfAxisWidth.Y * Math.Abs(axisyinnewcords.X);
            //var yradius = obb1.HalfAxisWidth.X * Math.Abs(axisxinnewcords.Y) + obb1.HalfAxisWidth.Y * Math.Abs(axisyinnewcords.Y);


            //res2.X = xradius;
            //res2.Y = yradius;
            //res2.Z = 0;
            //ret.HalfAxis = res2;


            //return ret;

        }
    }
}
