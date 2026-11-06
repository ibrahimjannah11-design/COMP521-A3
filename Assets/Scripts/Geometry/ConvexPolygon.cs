using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sim   // Used bc I did not have access to Unity when writing code (PC unabvailable and my mac cant use Unity :( )
                // So i wrote my own Vec2, Rect2D, ConvexPolygon, etc. to avoid Unity dependencies.
                 // Keeping this comment in case i do not revert back to Unity's Vector2, Rect, etc. layer
{
    //Convex polygon with counter-clockwise vertices.
    public class ConvexPolygon
    {
        public readonly Vec2[] Vertices;
        public readonly Rect2D Bounds;

        public ConvexPolygon(Vec2[] vertices)
        {
            Vertices = vertices;
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            foreach (var v in vertices)
            {
                minX = Math.Min(minX, v.x); maxX = Math.Max(maxX, v.x);
                minY = Math.Min(minY, v.y); maxY = Math.Max(maxY, v.y);
            }
            Bounds = new Rect2D(minX, minY, maxX, maxY);
        }

        public bool Contains(Vec2 p)
        {
            int n = Vertices.Length;
            for (int i = 0; i < n; i++)
            {
                Vec2 a = Vertices[i], b = Vertices[(i + 1) % n];
                float cross = (b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x);
                if (cross < 0f) return false;
            }
            return true;
        }

        // Helper funcytion to project the polygon onto an axis and return the min/max values
        private (float min, float max) ProjectPolygon(Vec2 axis)
        {
            float min = Vec2.Dot(axis, Vertices[0]);
            float max = min;
            for (int i = 1; i < Vertices.Length; i++)
            {
                float projection = Vec2.Dot(axis, Vertices[i]);
                if (projection < min) min = projection;
                if (projection > max) max = projection;
            }
            return (min, max);
        }

        // For overlaps, use SAT (Separating Axis Test) to check if two convex polygons overlap. 
        // Return true if they overlap, false otherwise.
        public bool Overlaps(ConvexPolygon other)
        {
            // First, we can check the bounding boxes for a quick answer
            if (!Bounds.Overlaps(other.Bounds))
                return false;

            // Now, we can actually apply the Separating Axis Theorem (SAT) to check for overlap
            // First, take edge normals for each edge of both polygons
            List<Vec2> axes = new List<Vec2>();
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vec2 a = Vertices[i];
                Vec2 b = Vertices[(i + 1) % Vertices.Length];
                Vec2 edge = b - a;
                Vec2 normal = new Vec2(-edge.y, edge.x).Normalized; // Perpendicular to the edge
                axes.Add(normal);
            }

            // Now do the same for the other polygon
            for (int i = 0; i < other.Vertices.Length; i++)
            {
                Vec2 a = other.Vertices[i];
                Vec2 b = other.Vertices[(i + 1) % other.Vertices.Length];
                Vec2 edge = b - a;
                Vec2 normal = new Vec2(-edge.y, edge.x).Normalized; // Perpendicular to the edge
                axes.Add(normal);
            }

            // For each axis, project both polygons and check for overlap
            foreach (var axis in axes)
            {
                (float min1, float max1) = ProjectPolygon(axis);
                (float min2, float max2) = other.ProjectPolygon(axis);

                // If any axis says no overlap, then the polygons do not overlap
                if (max1 < min2 || max2 < min1)
                    return false;
            }

            return true;
        }
    }

}