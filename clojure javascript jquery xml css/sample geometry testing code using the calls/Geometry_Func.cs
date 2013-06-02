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
using System.Diagnostics;
using GELib.Geometry.Geometric_Objects;
using GELib.Geometry.Geometric_Queries;
namespace GELib
{
    
    public static class Geometry_Func
    {
        

        
     
        public static bool CoarsePlaneSphere(Plane_ p, Sphere s)
        {
            Stats.TestPlaneSphere++;

            return Vector3.Dot(s.Position, p.Normal) < p.dist + s.Radius; }
        
        public static bool TestSphereSphere(Sphere s1, Sphere s2)
        {
            //Stats.TestSphereSphere++;

            var seperation = s2.Position - s1.Position;
            var distsq = seperation.LengthSquared();
            return distsq <= (s1.Radius + s2.Radius) * (s1.Radius + s2.Radius);
        }
       
        public static bool TestSphereOBB(Sphere s, OBB obb)
        {
            Stats.TestSphereOBB++;

            var positionloc = obb.Point_W_To_L(s.Position);

            var x1width = obb.HalfAxisWidth.X + s.Radius;
            var y1width = obb.HalfAxisWidth.Y + s.Radius;

            return ((Math.Abs(positionloc.X) <= x1width)
                &&
                (Math.Abs(positionloc.Y) <= y1width))
                ;
        }

        public static bool TestOBBOBB(OBB obb1, OBB obb2)
        {

            Stats.TestOBBOBB++;
            var mink1 = Minkowski.Minkowskize_OBB_In_OBB(obb2, obb1);


            var x1width = obb1.HalfAxisWidth.X + mink1.HalfAxis.X;
            var y1width = obb1.HalfAxisWidth.Y + mink1.HalfAxis.Y;

            if (((Math.Abs(mink1.Position_LM.X) > x1width)
                 ||
                 (Math.Abs(mink1.Position_LM.Y) > y1width)))
                return false
                 ;
            else
            {
                var mink2 = Minkowski.Minkowskize_OBB_In_OBB(obb1, obb2);


                var x2width = obb2.HalfAxisWidth.X + mink2.HalfAxis.X;
                var y2width = obb2.HalfAxisWidth.Y + mink2.HalfAxis.Y;

                return (!(((Math.Abs(mink2.Position_LM.X) > x2width)
                     ||
                     (Math.Abs(mink2.Position_LM.Y) > y2width))));

            }

        }

       
        public static Geometry.Ray_Test_Result TestOBBLine(OBB obb, LineSegment ls, Geometry.Ray_Test_Mode mode)
        {
            //if (obb == null) return new Ray_Test_Result();
            var result = new Geometry.Ray_Test_Result();
            var pos2 = obb.Point_W_To_L(ls.end);
            var pos1 = obb.Point_W_To_L(ls.start);
            var dir = pos2 - pos1;
            if (Math.Abs(dir.X) < 0.01f)
            {
                if (Math.Abs(pos1.X) > obb.HalfAxisWidth.X)
                {
                    result.Did_It_Hit = false;
                    return result;
                }
            }

            var tleftx = (-obb.HalfAxisWidth.X - pos1.X) / dir.X;
            var trightx = (obb.HalfAxisWidth.X - pos1.X) / dir.X;
            var tenterx = tleftx < trightx ? tleftx : trightx;
            var texitx = tleftx < trightx ? trightx : tleftx;


            if (Math.Abs(dir.Y) < 0.01f)
            {
                if (Math.Abs(pos1.Y) > obb.HalfAxisWidth.Y)
                {
                    result.Did_It_Hit = false;
                    return result;
                }
            }

            var tlefty = (-obb.HalfAxisWidth.Y - pos1.Y) / dir.Y;
            var trighty = (obb.HalfAxisWidth.Y - pos1.Y) / dir.Y;
            var tentery = tlefty < trighty ? tlefty : trighty;
            var texity = tlefty < trighty ? trighty : tlefty;

            var tmaxmin = tentery > tenterx ? tentery : tenterx;
            var tminmax = texity < texitx ? texity : texitx;
            result.enter = tmaxmin;
            result.exit = tminmax;
            switch (mode)
            {
                case Geometry.Ray_Test_Mode.Line: result.Did_It_Hit = (tmaxmin < tminmax);
                    break;
                case Geometry.Ray_Test_Mode.Ray: result.Did_It_Hit = (tmaxmin < tminmax) && (tmaxmin >= 0);
                    break;

                case Geometry.Ray_Test_Mode.Segment: result.Did_It_Hit = (tmaxmin < tminmax) && (tmaxmin >= 0) && (tmaxmin <= 1);
                    break;
            }
            

            return result;
        }


     
      
       


        //***************************************************
        //INTERSECTIONS
      
         

        public static bool IntersectOBBOBB2(OBB obb1, OBB obb2, ref Geometry.Geometric_Queries.Distance_Query_Result result)
        {






            




            var mink2 = Minkowski.Minkowskize_OBB_In_OBB(obb2, obb1);// obb2.Minkowskize_Me(ref obb1.TOrientation, obb1.Position);
            var mink1 = Minkowski.Minkowskize_OBB_In_OBB(obb1, obb2); //obb1.Minkowskize_Me(ref obb2.TOrientation, obb2.Position);

            //Vector3 Seperating_Axis_of_1 = Vector3.Zero;
            //Vector3 Seperating_Edge_of_1 = Vector3.Zero;
            //int Seperating_Edge_index_of_1 = -1;
            var dist_of_2_from_1 = obb1.What_Faces(mink2);

            //Vector3 Seperating_Axis_of_2 = Vector3.Zero;
            //Vector3 Seperating_Edge_of_2 = Vector3.Zero;
            //int Seperating_Edge_of_2_index = -1;
            var dist_of_1_from_2 = obb2.What_Faces(mink1);

            if (Math.Max(dist_of_1_from_2.Signed_Dist, dist_of_2_from_1.Signed_Dist) > 0.1) return false;

            Minkowski_Description selected_mink;
            Facing_Point_Result selected_data;

            //pernoume os normal owning afto pou exei to allo pio exo
            bool normal_owning_first = dist_of_2_from_1.Signed_Dist > dist_of_1_from_2.Signed_Dist;
            var selected_obb = (!normal_owning_first) ? obb1 : obb2;
            var normal_owning_obb = (normal_owning_first) ? obb1 : obb2;

            selected_mink = (normal_owning_first) ? mink2: mink1;
            //if (!(normal_owning_obb.Am_I_In(selected_obb.Position, selected_radiuses, axis))) return false;



            //var selected_normal = (normal_owning_first) ? Seperating_Axis_of_1 : Seperating_Axis_of_2;
            //var selected_edge = (normal_owning_first) ? Seperating_Edge_of_1 : Seperating_Edge_of_2;
            //var selected_edge_index = (normal_owning_first) ? Seperating_Edge_index_of_1 : Seperating_Edge_of_2_index;

            selected_data = (normal_owning_first) ? dist_of_2_from_1 : dist_of_1_from_2;

            bool diplo; Vector3 point1; Vector3 point2, Extreme_Point_In_Normal_Of_Body_1, Extreme_Point_In_Normal_Of_Body_2;
            var sel_norm = normal_owning_obb.Normal(selected_data.Normal_Code);
            OBB.Extraction_of_inner_contact_points(normal_owning_obb, selected_obb, sel_norm,
                normal_owning_obb.Normal((selected_data.Normal_Code + 1) % 4),
                selected_data.Normal_Code, out diplo, out point1, out point2, out Extreme_Point_In_Normal_Of_Body_1, out Extreme_Point_In_Normal_Of_Body_2);


            result = new Geometry.Geometric_Queries.Distance_Query_Result();
            //result.obb1 = normal_owning_obb;
            //result.obb2 = selected_obb;
            result.first_has_normal = normal_owning_first;
            result.Normal_W = sel_norm;
           
            //result.Just_the_extreme_distance = /*result.first_has_normal*/Vector3.Dot(result.Normal_W, Extreme_Point_In_Normal_Of_Body_2 - Extreme_Point_In_Normal_Of_Body_1);
            result.signed_distance = normal_owning_first ? dist_of_2_from_1.Signed_Dist : dist_of_1_from_2.Signed_Dist;
            if (!diplo)
            {
                result.double_contact = false;
                result.Point_W_0 = point1;
            }
            else
            {
                result.double_contact = true;
                result.Point_W_0 = point1;
                result.Point_W_1 = point2;
            }
            return true;
        }

        public static bool IntersectOBBOBB(OBB obb1, OBB obb2, ref Geometry.Geometric_Queries.Distance_Query_Result result)
        {











            var mink2 = Minkowski.Minkowskize_OBB_In_OBB(obb2, obb1);// obb2.Minkowskize_Me(ref obb1.TOrientation, obb1.Position);
            var mink1 = Minkowski.Minkowskize_OBB_In_OBB(obb1, obb2); //obb1.Minkowskize_Me(ref obb2.TOrientation, obb2.Position);

            var dist_of_2_from_1 = obb1.What_Faces(mink2);

            var dist_of_1_from_2 = obb2.What_Faces(mink1);

            if (Math.Max(dist_of_1_from_2.Signed_Dist, dist_of_2_from_1.Signed_Dist) > 0) return false;

            Minkowski_Description mink;
            Facing_Point_Result data;
            
            //pernoume os normal owning afto pou exei to allo pio exo
            bool normal_owning_first = dist_of_2_from_1.Signed_Dist > dist_of_1_from_2.Signed_Dist;
            
            var normal_owning_obb = (normal_owning_first) ? obb1 : obb2;
            var selected_obb = (!normal_owning_first) ? obb1 : obb2;
         
            mink = (normal_owning_first) ? mink2 : mink1;
           
            data = (normal_owning_first) ? dist_of_2_from_1 : dist_of_1_from_2;

            //normal_owning_obb.Normal((selected_data.Normal_Code + 1) % 4)

           Vector3 p0; Vector3 p1; float d_0, d_1;
            var normal = normal_owning_obb.Normal(data.Normal_Code);
            
            OBB.Extraction_of_inner_contact_points2(normal_owning_obb, selected_obb, normal,
                data.Normal_Code,out p0, out p1, out d_0, out d_1 );


            //var Extreme_Point_In_Normal_Of_Body_1 = normal_owning_obb.Vertex(data.Normal_Code);
            //var Extreme_Point_In_Normal_Of_Body_2 = selected_obb.Vertex(selected_obb.Extreme_In(-normal).code);// Vector3.Dot(normal, p0) < Vector3.Dot(normal, p1) ? p0 : p1;

            var test = p0 - p1;
          


            result = new Geometry.Geometric_Queries.Distance_Query_Result();
          
            result.first_has_normal = normal_owning_first;
            result.Normal_W = normal;

             result.signed_distance = normal_owning_first ? dist_of_2_from_1.Signed_Dist : dist_of_1_from_2.Signed_Dist;


             result.double_contact = (Math.Abs(d_1-d_0) < 0.3f);

             //if(test.LengthSquared() > ) result.double_contact = false;
             GELib.Debug.Debug_Drawing.double_edge = result.double_contact;

                result.Point_W_0 = p0;
                result.Point_W_1 = p1;
            
            return true;
        }




        public static bool IntersectOBBTriangle(OBB obb, Triangle trig, ref Geometry.Geometric_Queries.Distance_Query_Result result)
        {

            var minked_tri = Minkowski.Minkowskize_Triangle_In_OBB(trig, obb);
            var minked_obb = Minkowski.Minkowskize_OBB_In_Triangle(obb, trig); 


            var tr_from_obb = obb.What_Faces(minked_tri);

            var obb_from_tr = trig.What_Faces(minked_obb);

            if (Math.Max(tr_from_obb.Signed_Dist, obb_from_tr.Signed_Dist) > 0.0f) return false;

         
            //pernoume os normal owning afto pou exei to allo pio exo
            bool normal_owning_tr = obb_from_tr.Signed_Dist>tr_from_obb.Signed_Dist;

           
           

            if (normal_owning_tr)
            {
                result = new Geometry.Geometric_Queries.Distance_Query_Result();
                result.first_has_normal = !normal_owning_tr;
                result.Normal_W = trig.Normals[obb_from_tr.Normal_Code];
                 var Extreme_Point_In_Normal_Of_Body_1 = trig.Points[obb_from_tr.Normal_Code]-Intersection_Grid.current_pair_element_2.Rigid.Position;
                var Extreme_Point_In_Normal_Of_Body_2 = obb.Vertex(obb.Extreme_In(-result.Normal_W).code)-obb.Position;
                //result.Just_the_extreme_distance = /*result.first_has_normal*/ Vector3.Dot(result.Normal_W, Extreme_Point_In_Normal_Of_Body_2 - Extreme_Point_In_Normal_Of_Body_1);
                    
                result.signed_distance = obb_from_tr.Signed_Dist;
                //if (!diplo)
                //{
                //    result.diplo = false;
                //    result.point_in_world = result.Extreme_Point_In_Normal_Of_Body_2;
                //}
                //else
                //{
                    result.double_contact = false;
                    result.Point_W_0 = Extreme_Point_In_Normal_Of_Body_2+obb.Position;
                    //result.point_in_world_2 = point2;
                //}
            }
            else
            {
                result = new Geometry.Geometric_Queries.Distance_Query_Result();
                result.first_has_normal = !normal_owning_tr;
                result.Normal_W = obb.Normal(tr_from_obb.Normal_Code);
                var Extreme_Point_In_Normal_Of_Body_1 = obb.Vertex(tr_from_obb.Normal_Code)-obb.parent_tmp.Position;
                var Extreme_Point_In_Normal_Of_Body_2 = trig.Points[(trig.Extreme_In(-result.Normal_W).code)] - Intersection_Grid.current_pair_element_2.Rigid.Position;
                //result.Just_the_extreme_distance = /*result.first_has_normal*/Vector3.Dot(result.Normal_W, Extreme_Point_In_Normal_Of_Body_2 - Extreme_Point_In_Normal_Of_Body_1);
                    
                result.signed_distance = tr_from_obb.Signed_Dist;
                //if (!diplo)
                //{
                //    result.diplo = false;
                result.Point_W_0 = Extreme_Point_In_Normal_Of_Body_2+Intersection_Grid.current_pair_element_2.Rigid.Position;
                //}
                //else
                //{
                //    result.diplo = false;
                    //result.point_in_world = point1;
                    //result.point_in_world_2 = point2;
                //}
            }
            return true;
        }

        public static bool IntersectSphereTriangle(Sphere sphere, Triangle trig, ref Geometry.Geometric_Queries.Distance_Query_Result result)
        {

            var mink_sph = new Minkowski_Description();
            mink_sph.HalfAxis = new Vector4(sphere.Radius, sphere.Radius, sphere.Radius,1);
            mink_sph.Position_LM = sphere.Position - trig.Bary_center;
            var sphere_from_trig = trig.What_Faces(mink_sph);
            //var ret = obb.Closest_Point_Sphere_OBB(s1.Position, Radius, ref Point, ref Normal, ref dist);
          

            if (sphere_from_trig.Signed_Dist > 0.0f) return false;

            
                result = new Geometry.Geometric_Queries.Distance_Query_Result();
                result.first_has_normal = false;
                result.Normal_W = trig.Normals[sphere_from_trig.Normal_Code];
                var Extreme_Point_In_Normal_Of_Body_1 = trig.Points[sphere_from_trig.Normal_Code] - Intersection_Grid.current_pair_element_2.Rigid.Position;
                var Extreme_Point_In_Normal_Of_Body_2 = -result.Normal_W * sphere.Radius;
                //result.Just_the_extreme_distance = /*result.first_has_normal*/ Vector3.Dot(result.Normal_W,Extreme_Point_In_Normal_Of_Body_2 - Extreme_Point_In_Normal_Of_Body_1);
                    

                result.signed_distance = sphere_from_trig.Signed_Dist;
                result.double_contact=false;
                result.Point_W_0 = Extreme_Point_In_Normal_Of_Body_2 + sphere.Position;
                
            return true;
        }

        //public static bool IntersectSphereOBB2(Sphere s1, OBB obb, ref Geometry.Geometric_Queries.Distance_Query_Result result)
        //{
        
        //    var res = new Distance_Query_Result_Normal_As_Code();
            
        //    var ret = obb.Closest_Point_Sphere_OBB(s1, ref res);
        //    if (!ret) return false;
        //    result = new Geometry.Geometric_Queries.Distance_Query_Result();
        //    var Extreme_Point_In_Normal_Of_Body_2 = res.Point_W - s1.Position;
        //    Extreme_Point_In_Normal_Of_Body_2.Normalize();
        //    Extreme_Point_In_Normal_Of_Body_2 *= s1.Radius;
        //    var Extreme_Point_In_Normal_Of_Body_1 = res.Point_W - obb.Position;
        //    result.Just_the_extreme_distance = Vector3.Dot(result.Normal_W,  Extreme_Point_In_Normal_Of_Body_2 -  Extreme_Point_In_Normal_Of_Body_1);
                    
            
            

        //    result.first_has_normal = false;
           
        //    result.double_contact = false;
        //    result.Point_W_1 = res.Point_W;

        //    result.Normal_W = obb.Normal(res.Normal_Code);// Normal;
        //    //result.intersection = ret;
            
            

        //    result.signed_distance = res.dist;
            
        //    return true;



            
        //}

        public static bool IntersectSphereOBB(Sphere s1, OBB obb, ref Geometry.Geometric_Queries.Distance_Query_Result result)
        {

            //var res = new Distance_Query_Result_Normal_As_Code();

            var closer = obb.Closest_W_Point_On_OBB_To(s1.Position, 0);
            var res = s1.What_Faces(closer);
            if (res.Signed_Dist>0.1f) return false;

            result = new Geometry.Geometric_Queries.Distance_Query_Result();
            var Extreme_Point_In_Normal_Of_Body_2 =  closer;

            var Extreme_Point_In_Normal_Of_Body_1 = s1.Radius * res.Normal;
            result.signed_distance = res.Signed_Dist;
            //result.Just_the_extreme_distance = res.Signed_Dist-Vector3.Dot(res.Normal, obb.Position-s1.Position);




            result.first_has_normal = true;

            result.double_contact = false;
            result.Point_W_0 = closer;

            result.Normal_W = res.Normal;



            

            return true;




        }



        //WHAT????
        public static bool TestOBBSegment_Intersection_PN(OBB obb, LineSegment ls, ref Distance_Query_Result_PN_only res)
        {
            Stats.IntersectOBBSegment++;

            var pos2 = obb.Point_W_To_L(ls.end);
            var pos1 = obb.Point_W_To_L(ls.start);
            var dir = pos2 - pos1;
            if (Math.Abs(dir.X) < 0.00001f)
            {
                if (Math.Abs(pos1.X) > obb.HalfAxisWidth.X) return false;
            }

            var tleftx = (-obb.HalfAxisWidth.X - pos1.X) / dir.X;
            var trightx = (obb.HalfAxisWidth.X - pos1.X) / dir.X;
            var tenterx = tleftx < trightx ? tleftx : trightx;
            var minindexx = tleftx < trightx ? 0 : 1;
            var texitx = tleftx < trightx ? trightx : tleftx;


            if (Math.Abs(dir.Y) < 0.01f)
            {
                if (Math.Abs(pos1.Y) > obb.HalfAxisWidth.Y) return false;
            }

            var tlefty = (-obb.HalfAxisWidth.Y - pos1.Y) / dir.Y;
            var trighty = (obb.HalfAxisWidth.Y - pos1.Y) / dir.Y;
            var tentery = tlefty < trighty ? tlefty : trighty;
            var minindexy = tlefty < trighty ? 2 : 3;

            var texity = tlefty < trighty ? trighty : tlefty;

            var tmaxmin = tentery > tenterx ? tentery : tenterx;
            var tminmax = texity < texitx ? texity : texitx;
            if (tminmax < tmaxmin) return false;
            if (tmaxmin < 0 || tmaxmin > 1) return false;
            var indexmin = tentery > tenterx ? minindexy : minindexx;


            var pos3 = pos1 + tmaxmin * dir;
            res.Pos_W = Vector3.Transform(pos3, obb.Orientation) + obb.Position;
            res.Dist = tmaxmin;
            //float[] tmp = new float[4];
            //tmp[0] = Math.Abs(pos3.X + obb.HalfAxisWidth.X);
            //tmp[1] = Math.Abs(pos3.X - obb.HalfAxisWidth.X);
            //tmp[2] = Math.Abs(pos3.X - obb.HalfAxisWidth.Y);
            //tmp[3] = Math.Abs(pos3.X + obb.HalfAxisWidth.Y);
            //float min = 1000;
            //int index = -1;
            //for (int j = 0; j < 4; j++)
            //{
            //    if (tmp[j] < min) { min = j; index = j; }
            //}
            Vector3 norm;
            switch (indexmin)
            {
                case 0: norm = Vector3.Transform(Vector3.Left, obb.Orientation); ;
                    break;
                case 1: norm = Vector3.Transform(Vector3.Down, obb.Orientation);
                    break;
                case 2: norm = Vector3.Transform(Vector3.Right, obb.Orientation);
                    break;
                case 3: norm = Vector3.Transform(Vector3.Up, obb.Orientation);
                    break;
                default:
                    throw new Exception();
            }
            res.Normal = norm;
            return true;

        }

        public static Vector3 Closest_Point_Segment(Vector3 pos, LineSegment ls, ref float coeff)
        {


            var v = ls.end - ls.start;
            var p = pos - ls.start;
            var vp = Vector3.Dot(v, p);
            var vv = v.LengthSquared();
            if (vv < 0.0001f) { coeff = 1; return ls.end; }
            if (vp >= vv) { coeff = 1; return ls.end; }
            else if (vp <= 0) { coeff = 0; return ls.start; }
            else
            {

                var c = vp / vv;
                if ((c < 0) || (c > 1)) throw new Exception();
                coeff = c;

                return ls.start + c * v;
            }

        }

        public static Vector3 Closest_W_Point_On_OBB_To(this OBB obb, Vector3 center, float radius)
        {
            float x1width = obb.HalfAxisWidth.X +radius;
            float y1width = obb.HalfAxisWidth.Y + radius;
            var pos_l = obb.Point_W_To_L(center);
            var clampedx = MathHelper.Clamp(pos_l.X, -x1width, x1width);
            var clampedy = MathHelper.Clamp(pos_l.Y, -y1width, y1width);
            var clamped_point_w = obb.Point_L_To_W(new Vector3(clampedx, clampedy, 0));
            return clamped_point_w;
        }



        public static bool Test_Moving_Spheres(MoveSphere s1, MoveSphere s2)
        {
            Stats.intheproxytestpairsphere++;


            //momentum exei provlima an provlepseis me velocity
            //tote paei sosta ok
            //alla epeidi prin kaneis to update to teliko kanei sthn epomenh provlepsi
            //den exei trexei endiamesa convert
            //ki prokyptei provlima giati xanapas me ta palia
            var sep = s2.sphere.Position - s1.sphere.Position;
            var r = s2.sphere.Radius + s1.sphere.Radius;
            var vrel2 = -s1.Velocity;//().Mark ? Vector3.Zero : s2.Momentum * s2.InverseMass;
            var vrel1 = -s2.Velocity;//((NewObject)c1).Mark ? Vector3.Zero : s1.Momentum * s1.InverseMass;
            var rel = vrel2 - vrel1;
            //var normal = sep; normal.Normalize();
            var velax = Vector3.Dot(rel, sep);
            var c = sep.LengthSquared() - r * r;

            if (velax >= 0f) { return false; }


            var a = rel.LengthSquared();

            var b = velax;
            if (c < 0f) { /*throw new Exception();*/ return false; }
            if (a < 0.000001f) { return false; }//akinites relatively

            var d = b * b - a * c;
            if (d < 0) { return false; }
            //if (d < 0.001f) { time = -b / a; return true; }
            else
            {
                var time = (float)(((-b - Math.Sqrt(d)) / a));
                //lasttime = time;
                //lastmove = time * velax;
                if ((time >= 0.016f) || (time <= 0)) { return false; }
                else
                { return true; }
            }


        }













    
        //***************************************************
        //
        //    weird
        //
        //***************************************************
        //public static Ray_Test_Result Trace_Line_Local(LineSegment ls)
        //{

        //    while (true)
        //    {
        //        var aa = grid.getBucket(i, j);
        //        foreach (var g_obj in aa)
        //        {

        //            if(g_obj.g.Last_Query >= Grid_Query_Counter.Counter||g_obj.r!=i||g_obj.c!=j) continue;
        //            g_obj.g.Last_Query = Grid_Query_Counter.Counter;
        //            var obb = g_obj.g.Coll.OBB;
        //            var inner_res = TestOBBLine(obb, ls, mode);
        //            if (inner_res.Did_It_Hit) return inner_res;
        //        }
        //        if (tx <= ty)
        //        { // tx smallest, step in x
        //            if (i == final_i) break;
        //            tx += dtx;
        //            i += di;
        //        }
        //        else
        //        { // ty smallest, step in y
        //            if (j == final_j) break;
        //            ty += dty;
        //            j += dj;
        //        }
        //    }
        //    var res=new Ray_Test_Result();res.Did_It_Hit=false;
        //    return  res;
        //}


        public static bool TestPlaneOBB(Plane_ p, OBB obb)//UNUSED??
        {
            Stats.TestPlaneOBB++;

            return true;
            var nor = p.Normal;
            var minradius = obb.HalfAxisWidth.X * (Math.Abs(Vector3.Dot(nor, obb.Axis(0))) + obb.HalfAxisWidth.Y * Math.Abs(Vector3.Dot(nor, obb.Axis(1))));
            return Vector3.Dot(obb.Position, p.Normal) < p.dist + minradius + 0.01f;

            return true;
        }


        //public static bool IntersectSpherePlane(Sphere s, Plane_ p, ref Distance_Query_Result_Simple res)
        //{
        //    Stats.IntersectSpherePlane++;
        //    var extentinaxis = Vector3.Dot(s.Position, p.Normal);
        //    float deepness = extentinaxis - (p.dist + s.Radius);
        //    if (deepness > 0) return false;
        //    else
        //    {
        //        res.Dist = deepness;
        //        res.Pos_W = s.Position - p.Normal * (s.Radius);

        //        return true;
        //    }
        //}
       
        public static bool IntersectSphereSphere3(Sphere s1, Sphere s2, ref  Geometry.Geometric_Queries.Distance_Query_Result result)
        {


            //Stats.IntersectSphereSphere++;


            var vct = s2.Position - s1.Position;
            var dist = vct.Length();
            dist -= (s1.Radius + s2.Radius);
            if (dist > 0) return false;
            else
            {
                Vector3 Point = Vector3.Zero;
                Vector3 Normal = vct; Normal.Normalize();
                Point = s1.Position + Normal * s1.Radius;
                result = new Geometry.Geometric_Queries.Distance_Query_Result();
                var Extreme_Point_In_Normal_Of_Body_2 = s2.Position - Normal * s2.Radius;

                var Extreme_Point_In_Normal_Of_Body_1 = Point;
                //result.Just_the_extreme_distance =  Vector3.Dot(result.Normal_W,Extreme_Point_In_Normal_Of_Body_2 - Extreme_Point_In_Normal_Of_Body_1);
                    

                result.first_has_normal = true;

                result.double_contact = false;
                result.Point_W_0 = Point;

                result.Normal_W = Normal;
                //result.intersection = true;



                result.signed_distance = dist;

                return true;

            }



            //var res = s1.FindClosest(s2.Position, ref worldposition, ref worldnormal, ref dist);
            //return res;
        }
        
        public static bool TestPointSphere(Vector3 p, Sphere s)
        {
            Stats.TestPointSphere++;

            var seperation = p - s.Position;
            var distsq = seperation.LengthSquared();
            return distsq < (s.Radius) * (s.Radius);
        }
       
    

        public static bool Bounce_Point_Mass_Off_Plane(Plane_ p, Point_Mass_Body par)
        {
           

            var pd = Vector3.Dot(par.Position, p.Normal);
            var d = p.dist;
            d -= pd;
            if (d < 0) return false;
            par.Position += d * p.Normal;
            var mo = par.Momentum;
            var dot = Vector3.Dot(p.Normal, mo);
            if (dot < 0)
            {
                mo -= 2 * dot * p.Normal;
                par.Momentum = mo;
            }

            return true;
        }
        public static bool Bounce_Point_Mass_Off_Sphere(Sphere rc, Point_Mass_Body par)
        {




            var p = par.Position - rc.Position;
            var l = p.Length() - rc.Radius;
            if (l > 0) return false;
            var normal = p;
            normal.Normalize();
            par.Position += (rc.Radius - l) * normal;
            var v = par.Momentum;
            var dot = Vector3.Dot(normal, v);
            if (dot < 0)
            {
                v -= 1.0001f * dot * normal;
                par.Momentum = 0.5f * v;
            }



            return true;
        }






        //public static bool Slim_Plane_OBB(Plane_ p, float dist_other, float half, OBB o, Vector3 previous, ref Distance_Query_Result_Simple res)
        //{
        //    const float fatness = 0.05f;
        //    var radius = Math.Abs(Vector3.Dot(p.Normal, o.HalfAxisWidth.X * o.Axis(0))) + Math.Abs(Vector3.Dot(p.Normal, o.HalfAxisWidth.Y * o.Axis(1)));

        //    var distance_previous = Vector3.Dot(p.Normal, previous) - (p.dist + fatness + radius);
        //    var distance_now = Vector3.Dot(p.Normal, o.Position) - (p.dist + fatness + radius);
        //    var normal = new Vector3(p.Normal.Y, -p.Normal.X, 0);

        //    var distance_previous_other_u = Math.Abs(Vector3.Dot(normal, previous) - dist_other);
        //    if (distance_previous_other_u > half) return false;
        //    var added = (distance_now < 0) ? 0f : -0.0f;
        //    if ((distance_previous * (distance_now + added)) > 0) return false;
        //    res.Normal = (distance_previous >= 0) ? p.Normal : -p.Normal;
        //    res.Pos_W = o.Position - res.Normal * radius;
        //    res.Dist = -(Math.Abs(distance_now) + 0.05f);
        //    return true;

        //}

        //public static bool Slim_Plane_Circle(Plane_ p, float dist_other, float half, Sphere s, Vector3 previous, ref Distance_Query_Result_Simple res)
        //{
        //    const float fatness = 0.05f;
        //    var radius = s.Radius;//Math.Abs(Vector3.Dot(p.Normal, o.HalfAxisWidth.X * o.Axis(0))) + Math.Abs(Vector3.Dot(p.Normal, o.HalfAxisWidth.Y * o.Axis(1)));

        //    var distance_previous = Vector3.Dot(p.Normal, previous) - (p.dist + fatness + radius);
        //    var distance_now = Vector3.Dot(p.Normal, s.Position) - (p.dist + fatness + radius);
        //    var normal = new Vector3(p.Normal.Y, -p.Normal.X, 0);

        //    var distance_previous_other_u = Math.Abs(Vector3.Dot(normal, previous) - dist_other);
        //    if (distance_previous_other_u > half) return false;
        //    var added = (distance_now < 0) ? 0f : -0.0f;
        //    if ((distance_previous * (distance_now + added)) > 0) return false;
        //    res.Normal = (distance_previous >= 0) ? p.Normal : -p.Normal;
        //    res.Pos_W = s.Position - res.Normal * radius;
        //    res.Dist = -(Math.Abs(distance_now) + 0.05f);
        //    return true;
        //}

      
        /// <summary>
        /// to replace the bottom one
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="Velocity1"></param>
        /// <param name="Velocity2"></param>
        /// <returns></returns>
       
        public static bool IntersectSpherePlane2(Sphere s, Plane_ p, ref Vector3 worldposition, ref float dist)
        {
            Stats.IntersectSpherePlane++;
            var extentinaxis = Vector3.Dot(s.Position, p.Normal);
            float deepness = extentinaxis - (p.dist + s.Radius);
            if (deepness > 0) return false;
            else
            {
                dist = deepness;
                worldposition = s.Position - p.Normal * (s.Radius);

                return true;
            }
        }
        public static bool IntersectSphereSphere2(Sphere s1, Sphere s2, ref Vector3 worldposition, ref Vector3 worldnormal, ref float dist)
        {


            Stats.IntersectSphereSphere++;


            var vct = s2.Position - s1.Position;
            var distsq = vct.LengthSquared();
            distsq -= (s1.Radius + s2.Radius) * (s1.Radius + s2.Radius);
            if (distsq > 0) return false;
            else
            {
                worldnormal = vct;
                worldnormal.Normalize();
                dist = -((float)Math.Sqrt(Math.Abs(distsq)));
                worldposition = s1.Position + (s1.Radius + dist) * worldnormal;
                return true;
            }

            //var res = s1.FindClosest(s2.Position, ref worldposition, ref worldnormal, ref dist);
            //return res;
        }

        //public static bool IntersectSphereSphere_Old(Sphere s1, Sphere s2, ref Distance_Query_Result_Simple res)
        //{


        //    Stats.IntersectSphereSphere++;


        //    var vct = s2.Position - s1.Position;
        //    var distsq = vct.LengthSquared();
        //    distsq -= (s1.Radius + s2.Radius) * (s1.Radius + s2.Radius);
        //    if (distsq > 0) return false;
        //    else
        //    {
        //        res.Normal = vct;
        //        res.Normal.Normalize();
        //        res.Dist = -((float)Math.Sqrt(Math.Abs(distsq)));
        //        res.Pos_W = s1.Position + (s1.Radius + res.Dist) * res.Normal;
        //        return true;
        //    }

        //    //var res = s1.FindClosest(s2.Position, ref worldposition, ref worldnormal, ref dist);
        //    //return res;
        //}

        public static bool IntersectSphereSphere(Sphere s1, Sphere s2, ref Distance_Query_Result res)
        {


            var vct = s2.Position - s1.Position;
            var dist = vct.Length();
            dist -= (s1.Radius + s2.Radius) ;
            if (dist > 0) return false;
            if(vct!=Vector3.Zero)vct.Normalize();
            var point=s2.Position-vct*s2.Radius;
            res.Normal_W=vct;
            res.first_has_normal=true;
            res.Point_W_0=point;
            res.signed_distance=dist;
            //res.Just_the_extreme_distance=res.signed_distance-(Vector3.Dot(vct,s2.Position-s1.Position));
            res.first_has_normal=true;
            
                return true;
            

            
        }

    }
}
