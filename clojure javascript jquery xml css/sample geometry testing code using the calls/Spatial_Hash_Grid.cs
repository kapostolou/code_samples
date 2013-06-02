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

namespace GELib
{
    public struct Stored_Geometrical_Piece
    {
        public Geometry_Container g;
        public int col;
        public int row;
        public Stored_Geometrical_Piece(Geometry_Container g, int col, int row)
        {
            this.col = col;
            this.row = row;
            this.g = g;
        }
    }
   
    public class Spatial_Hash_Grid
    {
        const int NUM_BUCKETS = 1024;
        public List<Stored_Geometrical_Piece>[] buckets = new List<Stored_Geometrical_Piece>[NUM_BUCKETS];
        const uint h1 = 0x8da6b343; 
        const uint h2 = 0xd8163841; 
        
        public Spatial_Hash_Grid()
        {
            for (int i = 0; i < NUM_BUCKETS; i++)
            {
                buckets[i] = new List<Stored_Geometrical_Piece>(40);
            }
        }
        private int get_Bucket_Index(int col, int row)
        {
            var n = h1 * row + h2 * col;
            n = n % NUM_BUCKETS;
            if (n < 0) n += NUM_BUCKETS;
            return (int)n;
        }
        public void Insert(int col, int row, Geometry_Container gc)
        {
            buckets[get_Bucket_Index(col, row)].Add(new Stored_Geometrical_Piece(gc, col, row));
        }

        public List<Stored_Geometrical_Piece> getBucket(int col, int row)
        {
            return buckets[get_Bucket_Index(col, row)];
        }

        public void Clear()
        {
            for (int i = 0; i < NUM_BUCKETS; i++)
            {
        
                buckets[i].Clear();
            }
        }


    }

}
