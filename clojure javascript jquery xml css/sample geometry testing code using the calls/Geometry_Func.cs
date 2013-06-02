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
      
         

       
        public static bool IntersectOBBOBB(OBB obb1, OBB obb2, ref Geometry.Geometric_Queries.Distance_Query_Result result)
        {











            var mink2 = Minkowski.Minkowskize_OBB_In_OBB(obb2, obb1);
            var mink1 = Minkowski.Minkowskize_OBB_In_OBB(obb1, obb2);

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

            

           Vector3 p0; Vector3 p1; float d_0, d_1;
            var normal = normal_owning_obb.Normal(data.Normal_Code);
            
            OBB.Extraction_of_inner_contact_points2(normal_owning_obb, selected_obb, normal,
                data.Normal_Code,out p0, out p1, out d_0, out d_1 );


            
            var test = p0 - p1;
          


            result = new Geometry.Geometric_Queries.Distance_Query_Result();
          
            result.first_has_normal = normal_owning_first;
            result.Normal_W = normal;

             result.signed_distance = normal_owning_first ? dist_of_2_from_1.Signed_Dist : dist_of_1_from_2.Signed_Dist;


             result.double_contact = (Math.Abs(d_1-d_0) < 0.3f);

            
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
            
                    
                result.signed_distance = obb_from_tr.Signed_Dist;
                result.double_contact = false;
                result.Point_W_0 = Extreme_Point_In_Normal_Of_Body_2+obb.Position;
            
            }
            else
            {
                result = new Geometry.Geometric_Queries.Distance_Query_Result();
                result.first_has_normal = !normal_owning_tr;
                result.Normal_W = obb.Normal(tr_from_obb.Normal_Code);
                var Extreme_Point_In_Normal_Of_Body_1 = obb.Vertex(tr_from_obb.Normal_Code)-obb.parent_tmp.Position;
                var Extreme_Point_In_Normal_Of_Body_2 = trig.Points[(trig.Extreme_In(-result.Normal_W).code)] - Intersection_Grid.current_pair_element_2.Rigid.Position;
                    
                result.signed_distance = tr_from_obb.Signed_Dist;
                result.Point_W_0 = Extreme_Point_In_Normal_Of_Body_2+Intersection_Grid.current_pair_element_2.Rigid.Position;
            
            }
            return true;
        }

        public static bool IntersectSphereTriangle(Sphere sphere, Triangle trig, ref Geometry.Geometric_Queries.Distance_Query_Result result)
        {

            var mink_sph = new Minkowski_Description();
            mink_sph.HalfAxis = new Vector4(sphere.Radius, sphere.Radius, sphere.Radius,1);
            mink_sph.Position_LM = sphere.Position - trig.Bary_center;
            var sphere_from_trig = trig.What_Faces(mink_sph);
            
          

            if (sphere_from_trig.Signed_Dist > 0.0f) return false;

            
                result = new Geometry.Geometric_Queries.Distance_Query_Result();
                result.first_has_normal = false;
                result.Normal_W = trig.Normals[sphere_from_trig.Normal_Code];
                var Extreme_Point_In_Normal_Of_Body_1 = trig.Points[sphere_from_trig.Normal_Code] - Intersection_Grid.current_pair_element_2.Rigid.Position;
                var Extreme_Point_In_Normal_Of_Body_2 = -result.Normal_W * sphere.Radius;
            
                    

                result.signed_distance = sphere_from_trig.Signed_Dist;
                result.double_contact=false;
                result.Point_W_0 = Extreme_Point_In_Normal_Of_Body_2 + sphere.Position;
                
            return true;
        }

       
        public static bool IntersectSphereOBB(Sphere s1, OBB obb, ref Geometry.Geometric_Queries.Distance_Query_Result result)
        {

           

            var closer = obb.Closest_W_Point_On_OBB_To(s1.Position, 0);
            var res = s1.What_Faces(closer);
            if (res.Signed_Dist>0.1f) return false;

            result = new Geometry.Geometric_Queries.Distance_Query_Result();
            var Extreme_Point_In_Normal_Of_Body_2 =  closer;

            var Extreme_Point_In_Normal_Of_Body_1 = s1.Radius * res.Normal;
            result.signed_distance = res.Signed_Dist;
           




            result.first_has_normal = true;

            result.double_contact = false;
            result.Point_W_0 = closer;

            result.Normal_W = res.Normal;



            

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
           

            //momentum exei provlima an provlepseis me velocity
            //tote paei sosta ok
            //alla epeidi prin kaneis to update to teliko kanei sthn epomenh provlepsi
            //den exei trexei endiamesa convert
            //ki prokyptei provlima giati xanapas me ta palia
            var sep = s2.sphere.Position - s1.sphere.Position;
            var r = s2.sphere.Radius + s1.sphere.Radius;
            var vrel2 = -s1.Velocity;
            var vrel1 = -s2.Velocity;
            var rel = vrel2 - vrel1;
            
            var velax = Vector3.Dot(rel, sep);
            var c = sep.LengthSquared() - r * r;

            if (velax >= 0f) { return false; }


            var a = rel.LengthSquared();

            var b = velax;
            if (c < 0f) { /*throw new Exception();*/ return false; }
            if (a < 0.000001f) { return false; }//akinites relatively

            var d = b * b - a * c;
            if (d < 0) { return false; }
            
            else
            {
                var time = (float)(((-b - Math.Sqrt(d)) / a));
            
                if ((time >= 0.016f) || (time <= 0)) { return false; }
                else
                { return true; }
            }


        }













    
      
        public static bool TestPlaneOBB(Plane_ p, OBB obb)//UNUSED??
        {
            Stats.TestPlaneOBB++;

            return true;
            var nor = p.Normal;
            var minradius = obb.HalfAxisWidth.X * (Math.Abs(Vector3.Dot(nor, obb.Axis(0))) + obb.HalfAxisWidth.Y * Math.Abs(Vector3.Dot(nor, obb.Axis(1))));
            return Vector3.Dot(obb.Position, p.Normal) < p.dist + minradius + 0.01f;

            return true;
        }


   
        public static bool TestPointSphere(Vector3 p, Sphere s)
        {
           

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

            res.first_has_normal=true;
            
                return true;
            

            
        }

    }
}
