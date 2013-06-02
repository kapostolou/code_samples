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
using GELib.Spatial.Grids;
using GELib.Geometry.Geometric_Objects;
using GELib.Physics.Rigid_Body_Structures;
using GELib.Geometry.Geometric_Queries;


namespace GELib
{

    public struct Grid_Placement_Info
    {
        public Spatial.Grid_Coord coords;
        public int row_width;
        public int col_width;

        public int own_group;
        public int[] groups_it_hits;
    }
    public enum Col_Slot
    {
        Neutral=0,
        Player,
        Enemy,
        NA

    }
   

    public class Intersection_Grid
    {

       
        
        //xrisimopoiountai mono cellsize kai no of grids
        Grid_Info grid_info;

        //one per group
        public Spatial_Hash_Grid[] grids;

        //quick access to the grids storing obstacle geometry and the player allied geometry
        public Spatial_Hash_Grid neutral;
        public Spatial_Hash_Grid player;
        
        public void Clear()
        {
            
                    for (int u = 0; u < grid_info.groups; u++)

                        grids[u].Clear();
        }
        
        
        public Intersection_Grid(Spatial.Grids.Grid_Info Grid_Info)
        {

            this.grid_info = Grid_Info;

            grids = new Spatial_Hash_Grid[grid_info.groups];

            for (int u = 0; u < grid_info.groups; u++)//

                grids[u] = new Spatial_Hash_Grid();
            neutral = grids[(int)Col_Slot.Neutral];
            player = grids[(int)Col_Slot.Player];

        }


      
        public void InsertCollidable(Collidable c)
        {

            if (c.Spawning_Or_Hit) return;

         
            for (int piece_i = 0; piece_i < c.Rigid.Geometries.Count; piece_i++)
            {

                Grid_Query_Counter.Counter++;
                var piece = c.Rigid.Geometries[piece_i];
             
                var placement = piece.placement;
                if (placement.own_group >= 2)
                {
                    placement.row_width = 1; placement.col_width = 1;
                }
                int border = placement.own_group < 2 ? 1 : 0;


                for (int group_i = 0; group_i < placement.groups_it_hits.Length; group_i++)
                    for (int i = -border; i < placement.row_width + border; i++)//ta width xekinane me 1
                        for (int j = -border; j < placement.col_width + border; j++)
                        {
                            

                            var list = grids[placement.groups_it_hits[group_i]].getBucket(placement.coords.col + j, placement.coords.row + i);

                            for (int u = 0; u < list.Count; u++)
                            {
                                if (!((list[u].row == placement.coords.row + i) && (list[u].col == placement.coords.col + j))) continue;
                                var other = list[u].g;
                                if (((other.Collision_Type == 1)  && (piece.Collision_Type == 6)) || ((other.Collision_Type == 6) && (piece.Collision_Type == 1)))
                                {
                                    var a=5;
                                    Console.WriteLine(a);
                                }
                                current_pair_element_1 = piece;
                                current_pair_element_2 = other;
                                //
                                if ((other.Coll.Ignore_In_Collisions_Cause_It_Got_Killed) || (other.Coll.Spawning_Or_Hit) || other.Coll == piece.Coll)
                                    continue;

                                if (other.Last_Query < Grid_Query_Counter.Counter)
                                {
                                    other.Last_Query = Grid_Query_Counter.Counter;
                                   
                                    var dispatcher = Dispatcher.Dispatch_array[piece.Collision_Type, other.Collision_Type];
                               
                                    var coarse_test = dispatcher.Coarse_Test;
                                    if(coarse_test==null)
                                    {
                                        
                                        continue;
                                    }
                                    var check_coarse = dispatcher.rever_coarse?coarse_test(other, piece):coarse_test(piece, other);
                                    if(!check_coarse)
                                    {
                                        
                                        continue;
                                    }
                                    
                                   
                                    var test_call = dispatcher.Test;
                                   
                                    if (test_call != null)
                                    {
                                        var test_result = dispatcher.rever_test? test_call( other,piece): test_call(piece, other);
                                        if (test_result)
                                        {
                                            piece.Coll.Intersecting.Add(other.Coll);
                                            other.Coll.Intersecting.Add(piece.Coll);

                                            if (dispatcher.kill_first) piece.Ignore_In_Collisions_Cause_It_Got_Killed = true;
                                            if (dispatcher.kill_second) other.Ignore_In_Collisions_Cause_It_Got_Killed = true;
                                            if (dispatcher.kill_first) return;
                                            if (dispatcher.kill_second) continue;
                                            
                                        }
                                    }
                                   
                                    var intersect_call = dispatcher.I_Test;
                                    
                                    if (intersect_call != null)
                                    {
                                        var intersect_call_result = dispatcher.rever_intersect? intersect_call( other,piece): intersect_call(piece, other);
                                        if (intersect_call_result)
                                        {
                                            piece.Coll.Contacted.Add(other.Coll);
                                            other.Coll.Contacted.Add(piece.Coll);

                                            
                                            
                                        }
                                    }
                                
                                    

                                    

                                }




                            }


                        }


                //do not store enemies in the grid, only test them against existing geometry
                if (placement.own_group < 2)
                    for (int i = -1; i < placement.row_width + 1; i++)
                        for (int j = -1; j < placement.col_width + 1; j++)
                        {
                            
                            grids[placement.own_group].Insert(placement.coords.col + i, placement.coords.row + j, piece);

                        }

            }

            return;




        }




        public Grid_Placement_Info get_Info(Geometry_Container c)
        {
            var ret = new Grid_Placement_Info();
            var aabb = c._AABB;

            ret.coords = Spatial.Grid_Func.Grid_Coords(aabb.Position, grid_info.Cellsize);

            var widths = Spatial.Grid_Func.Find_Width_In_Cells(grid_info.Cellsize, aabb.Position, aabb.widthx, aabb.widthy);
            ret.row_width = widths.row_width;
            ret.col_width = widths.col_width;
            //if ((ret.width_x > 100) || (ret.width_y > 100)) throw new Exception();
            ret.own_group = Base_Collision_Batches.Collision_Group_Slot[c.Collision_Type];

            ret.groups_it_hits = Base_Collision_Batches.what_groups_each_group_collides_with[ret.own_group];// Multiple_Dispatch.Check_With_Whom[c.Collision_Type];
            return ret;
        }

        //used for debuging the next function
        public static OBB last_trace;

        /// <summary>
        /// xekinas me deiktes i j
        /// kai kataligeis se final_i, final_j //elegxe to an piaseis esto ena einai ok
        /// exeis kati times tx ty pou deixnoun "se ti pososto os pros monada apo to sinoliko manhattan x kai y antistoixa eisai tora"
        /// diladi xekinane apo miden ftanoun 1 alla kai pali me ta final stamatas
        /// </summary>

        //this method performs a bersenhaam algorithm like traversal of the "neutral" grid using a linesegment.
        //until it hits with an obstacle reporting the ray's/segment's parameter at the point of entering and leaving it
        public static Geometry.Ray_Test_Result Trace_Line(LineSegment ls, Geometry.Ray_Test_Mode mode)
        {
            var direction = ls.end - ls.start;
            if (mode == Geometry.Ray_Test_Mode.Line) throw new Exception();

            Grid_Query_Counter.Counter++;
            var grid = GELib.Managers.Spatial_Partitioning_Management.main_spatial_grid.neutral;// grids[(int)Col_Slot.Neutral];
         
            var cell_size = World_Info.Collision_Cellsize;
            var cell_coords_start = Spatial.Grid_Func.Grid_Coords(ls.start, cell_size);
            Spatial.Grid_Coord cell_coords_end;
            cell_coords_end = Spatial.Grid_Func.Grid_Coords(ls.end, cell_size);
         

            var i = cell_coords_start.col;
            var j = cell_coords_start.row;
            var final_i = cell_coords_end.col;
            var final_j = cell_coords_end.row;

            var y_manh_dist_inv = direction.Y == 0 ? Single.MaxValue : (float)1 / Math.Abs(direction.Y);
            var x_manh_dist_inv = direction.X == 0 ? Single.MaxValue : (float)1 / Math.Abs(direction.X);

            //an to cell afxanei -1,0,1 kathos proxoras analoga pou koitas
            int di = ((ls.start.X < ls.end.X) ? 1 : ((ls.start.X > ls.end.X) ? -1 : 0));
            int dj = ((ls.start.Y < ls.end.Y) ? 1 : ((ls.start.Y > ls.end.Y) ? -1 : 0));

            //se ti posostioso ton manh_dist xekinises,
            //xekinises oxi h arxiki timi pou einai miden
            //alla ta t deixnoun "to epomeno toixaki"
            //kai mallon me ta midenika sta di pou vazo pio pano ktl h idea einai oti
            //den tha peraseis dyo tixakia mias sintetagmenis seri
            //ex ou kai h anisotita sto for(;;)
            var tx = (ls.start.X > ls.end.X ? ls.start.X - (i * cell_size) : (i + 1) * cell_size - ls.start.X) * x_manh_dist_inv;
            var ty = (ls.start.Y > ls.end.Y ? ls.start.Y - (j * cell_size) : (j + 1) * cell_size - ls.start.Y) * y_manh_dist_inv;

            //ta vimata pou kaneis sthn timi tou tx kathos proxoras
            var dtx = cell_size * x_manh_dist_inv;
            var dty = cell_size * y_manh_dist_inv;

            while (true)
            {
                var aa = grid.getBucket(i, j);
                Geometry.Ray_Test_Result Min = new Geometry.Ray_Test_Result(); Min.enter = Single.MaxValue;
                for (int ib = 0; ib < aa.Count; ib++)
                {

                    var g_obj = aa[ib];
                    if (g_obj.g.Last_Query >= Grid_Query_Counter.Counter || g_obj.col != i || g_obj.row != j) continue;
                    g_obj.g.Last_Query = Grid_Query_Counter.Counter;
                    var collidable = g_obj.g.Coll;
                    if (collidable.RTII == RTI.Firing || collidable.RTII == RTI.Just_A_Triangle || collidable.RTII == RTI.Chain_Rectangle) continue;

                    var obb = collidable.OBB;
                    var inner_res = Geometry_Func.TestOBBLine(obb, ls, mode);
                    if (inner_res.Did_It_Hit)
                        if (inner_res.enter < Min.enter)
                        {
                            Min = inner_res;
                            last_trace = obb;
                        }

                }
                if (Min.enter != Single.MaxValue) return Min;
                if (tx <= ty)
                { // tx smallest, step in x
                    if (i == final_i) break;
                    tx += dtx;
                    i += di;
                }
                else
                { // ty smallest, step in y
                    if (j == final_j) break;
                    ty += dty;
                    j += dj;
                }
            }
            var res = new Geometry.Ray_Test_Result(); res.Did_It_Hit = false;
            return res;
        }


        //used as a static variable to avoid generating too many objects and confusing the Garbage collector
        //logically belings inside the scope pf the Test_Circle method
        private static Sphere _test_Sphere=new Sphere();
        public static bool Test_Circle(Vector3 pos, float r)
        {

            var coords = Spatial.Grid_Func.Grid_Coords(pos, World_Info.Collision_Cellsize);

            var widths = Spatial.Grid_Func.
                Find_Width_In_Cells(World_Info.Collision_Cellsize, new Vector3(pos.X - r, pos.Y - r, -10), 2 * r, 2 * r);


            Grid_Query_Counter.Counter++;
            var grid = GELib.Managers.Spatial_Partitioning_Management.main_spatial_grid.neutral;// grids[(int)Col_Slot.Neutral];


            for (int i = 0; i < widths.row_width; i++)//ta width xekinane me 1
                for (int j = 0; j < widths.col_width; j++)
                {
                    var list = grid.getBucket(coords.col + i, coords.row + j);

                    for (int u = 0; u < list.Count; u++)
                    {
                        if (!((list[u].row == coords.row + j) && (list[u].col == coords.col + i))) continue;
                        var other = list[u].g;
                        if(other.Last_Query>=  Grid_Query_Counter.Counter) continue;
                        other.Last_Query = Grid_Query_Counter.Counter;
                        var obb_tested_ = other as OBB_GC;
                        if (obb_tested_ == null) continue;
                        var obb_tested = obb_tested_.OBB;
                        _test_Sphere.Position=pos;
                        _test_Sphere.Radius=r;
                        bool test_result = Geometry_Func.TestSphereOBB(_test_Sphere, obb_tested);
                        if (test_result) return true;
                    }
                   
                }
            return false;
        }



        //will be refactored, only here cause some experimental method uses it
        public static Geometry_Container current_pair_element_1;
        public static Geometry_Container current_pair_element_2;

    }

   
    
}
