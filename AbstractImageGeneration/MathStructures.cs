/*
MIT License

Copyright (c) 2020 Erik Nordeus

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using static AbstractImageGeneration.MathStructures;

namespace AbstractImageGeneration
{
    internal class MathStructures
    {
        //And edge between two vertices
        public class Edge
        {
            public Vertex v1;
            public Vertex v2;

            //Is this edge intersecting with another edge?
            public bool isIntersecting = false;

            public Edge(Vertex v1, Vertex v2)
            {
                this.v1 = v1;
                this.v2 = v2;
            }

            public Edge(Vector3 v1, Vector3 v2)
            {
                this.v1 = new Vertex(v1);
                this.v2 = new Vertex(v2);
            }

            //Get vertex in 2d space (assuming x, z)
            public Vector2 GetVertex2D(Vertex v)
            {
                return new Vector2(v.position.X, v.position.Z);
            }

            //Flip edge
            public void FlipEdge()
            {
                Vertex temp = v1;

                v1 = v2;

                v2 = temp;
            }
        }
        public class Vertex
        {
            public Vector3 position;

            //The outgoing halfedge (a halfedge that starts at this vertex)
            //Doesnt matter which edge we connect to it
            public HalfEdge halfEdge;

            //Which triangle is this vertex a part of?
            public Triangle triangle;

            //The previous and next vertex this vertex is attached to
            public Vertex prevVertex;
            public Vertex nextVertex;

            //Properties this vertex may have
            //Reflex is concave
            public bool isReflex;
            public bool isConvex;
            public bool isEar;

            public Vertex(Vector3 position)
            {
                this.position = position;
            }

            //Get 2d pos of this vertex
            public Vector2 GetPos2D_XZ()
            {
                Vector2 pos_2d_xz = new Vector2(position.X, position.Z);

                return pos_2d_xz;
            }
        }

        public class HalfEdge
        {
            //The vertex the edge points to
            public Vertex v;

            //The face this edge is a part of
            public Triangle t;

            //The next edge
            public HalfEdge nextEdge;
            //The previous
            public HalfEdge prevEdge;
            //The edge going in the opposite direction
            public HalfEdge oppositeEdge;

            //This structure assumes we have a vertex class with a reference to a half edge going from that vertex
            //and a face (triangle) class with a reference to a half edge which is a part of this face 
            public HalfEdge(Vertex v)
            {
                this.v = v;
            }
        }

        public class Triangle
        {
            //Corners
            public Vertex v1;
            public Vertex v2;
            public Vertex v3;

            //If we are using the half edge mesh structure, we just need one half edge
            public HalfEdge halfEdge;

            public Triangle(Vertex v1, Vertex v2, Vertex v3)
            {
                this.v1 = v1;
                this.v2 = v2;
                this.v3 = v3;
            }

            public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
            {
                this.v1 = new Vertex(v1);
                this.v2 = new Vertex(v2);
                this.v3 = new Vertex(v3);
            }

            public Triangle(HalfEdge halfEdge)
            {
                this.halfEdge = halfEdge;
            }

            //Change orientation of triangle from cw -> ccw or ccw -> cw
            public void ChangeOrientation()
            {
                Vertex temp = this.v1;

                this.v1 = this.v2;

                this.v2 = temp;
            }
        }
    }   
    
    internal class Triangulator
    {
        

        public static List<HalfEdge> TransformFromTriangleToHalfEdge(List<Triangle> triangles)
        {
            //Make sure the triangles have the same orientation
            OrientTrianglesClockwise(triangles);

            //First create a list with all possible half-edges
            List<HalfEdge> halfEdges = new List<HalfEdge>(triangles.Count * 3);

            for (int i = 0; i < triangles.Count; i++)
            {
                Triangle t = triangles[i];

                HalfEdge he1 = new HalfEdge(t.v1);
                HalfEdge he2 = new HalfEdge(t.v2);
                HalfEdge he3 = new HalfEdge(t.v3);

                he1.nextEdge = he2;
                he2.nextEdge = he3;
                he3.nextEdge = he1;

                he1.prevEdge = he3;
                he2.prevEdge = he1;
                he3.prevEdge = he2;

                //The vertex needs to know of an edge going from it
                he1.v.halfEdge = he2;
                he2.v.halfEdge = he3;
                he3.v.halfEdge = he1;

                //The face the half-edge is connected to
                t.halfEdge = he1;

                he1.t = t;
                he2.t = t;
                he3.t = t;

                //Add the half-edges to the list
                halfEdges.Add(he1);
                halfEdges.Add(he2);
                halfEdges.Add(he3);
            }

            //Find the half-edges going in the opposite direction
            for (int i = 0; i < halfEdges.Count; i++)
            {
                HalfEdge he = halfEdges[i];

                Vertex goingToVertex = he.v;
                Vertex goingFromVertex = he.prevEdge.v;

                for (int j = 0; j < halfEdges.Count; j++)
                {
                    //Dont compare with itself
                    if (i == j)
                    {
                        continue;
                    }

                    HalfEdge heOpposite = halfEdges[j];

                    //Is this edge going between the vertices in the opposite direction
                    if (goingFromVertex.position == heOpposite.v.position && goingToVertex.position == heOpposite.prevEdge.v.position)
                    {
                        he.oppositeEdge = heOpposite;

                        break;
                    }
                }
            }


            return halfEdges;
        }

        //Orient triangles so they have the correct orientation
        public static void OrientTrianglesClockwise(List<Triangle> triangles)
        {
            for (int i = 0; i < triangles.Count; i++)
            {
                Triangle tri = triangles[i];

                Vector2 v1 = new Vector2(tri.v1.position.X, tri.v1.position.Z);
                Vector2 v2 = new Vector2(tri.v2.position.X, tri.v2.position.Z);
                Vector2 v3 = new Vector2(tri.v3.position.X, tri.v3.position.Z);

                if (!IsTriangleOrientedClockwise(v1, v2, v3))
                {
                    tri.ChangeOrientation();
                }
            }
        }

        //Is a triangle in 2d space oriented clockwise or counter-clockwise
        //https://math.stackexchange.com/questions/1324179/how-to-tell-if-3-connected-points-are-connected-clockwise-or-counter-clockwise
        //https://en.wikipedia.org/wiki/Curve_orientation
        public static bool IsTriangleOrientedClockwise(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            bool isClockWise = true;

            float determinant = p1.X * p2.Y + p3.X * p1.Y + p2.X * p3.Y - p1.X * p3.Y - p3.X * p2.Y - p2.X * p1.Y;

            if (determinant > 0f)
            {
                isClockWise = false;
            }

            return isClockWise;
        }

        //Is a point d inside, outside or on the same circle as a, b, c
        //https://gamedev.stackexchange.com/questions/71328/how-can-i-add-and-subtract-convex-polygons
        //Returns positive if inside, negative if outside, and 0 if on the circle
        public static float IsPointInsideOutsideOrOnCircle(Vector2 aVec, Vector2 bVec, Vector2 cVec, Vector2 dVec)
        {
            //This first part will simplify how we calculate the determinant
            float a = aVec.X - dVec.X;
            float d = bVec.X - dVec.X;
            float g = cVec.X - dVec.X;

            float b = aVec.Y - dVec.Y;
            float e = bVec.Y - dVec.Y;
            float h = cVec.Y - dVec.Y;

            float c = a * a + b * b;
            float f = d * d + e * e;
            float i = g * g + h * h;

            float determinant = (a * e * i) + (b * f * g) + (c * d * h) - (g * e * c) - (h * f * a) - (i * d * b);

            return determinant;
        }

        //Is a quadrilateral convex? Assume no 3 points are colinear and the shape doesnt look like an hourglass
        public static bool IsQuadrilateralConvex(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            bool isConvex = false;

            bool abc = IsTriangleOrientedClockwise(a, b, c);
            bool abd = IsTriangleOrientedClockwise(a, b, d);
            bool bcd = IsTriangleOrientedClockwise(b, c, d);
            bool cad = IsTriangleOrientedClockwise(c, a, d);

            if (abc && abd && bcd & !cad)
            {
                isConvex = true;
            }
            else if (abc && abd && !bcd & cad)
            {
                isConvex = true;
            }
            else if (abc && !abd && bcd & cad)
            {
                isConvex = true;
            }
            //The opposite sign, which makes everything inverted
            else if (!abc && !abd && !bcd & cad)
            {
                isConvex = true;
            }
            else if (!abc && !abd && bcd & !cad)
            {
                isConvex = true;
            }
            else if (!abc && abd && !bcd & !cad)
            {
                isConvex = true;
            }


            return isConvex;
        }

        //Alternative 1. Triangulate with some algorithm - then flip edges until we have a delaunay triangulation
        public static List<Triangle> TriangulateByFlippingEdges(List<Vector3> sites)
        {
            //Step 1. Triangulate the points with some algorithm
            //Vector3 to vertex
            List<Vertex> vertices = new List<Vertex>();

            for (int i = 0; i < sites.Count; i++)
            {
                vertices.Add(new Vertex(sites[i]));
            }

            //Triangulate the convex hull of the sites
            List<Triangle> triangles = TriangulatePoints(vertices);
            //List triangles = TriangulatePoints.TriangleSplitting(vertices);

            //Step 2. Change the structure from triangle to half-edge to make it faster to flip edges
            List<HalfEdge> halfEdges = TransformFromTriangleToHalfEdge(triangles);

            //Step 3. Flip edges until we have a delaunay triangulation
            int safety = 0;

            int flippedEdges = 0;

            while (true)
            {
                safety += 1;

                if (safety > 100000)
                {
                    //Debug.Log("Stuck in endless loop");

                    break;
                }

                bool hasFlippedEdge = false;

                //Search through all edges to see if we can flip an edge
                for (int i = 0; i < halfEdges.Count; i++)
                {
                    HalfEdge thisEdge = halfEdges[i];

                    //Is this edge sharing an edge, otherwise its a border, and then we cant flip the edge
                    if (thisEdge.oppositeEdge == null)
                    {
                        continue;
                    }

                    //The vertices belonging to the two triangles, c-a are the edge vertices, b belongs to this triangle
                    Vertex a = thisEdge.v;
                    Vertex b = thisEdge.nextEdge.v;
                    Vertex c = thisEdge.prevEdge.v;
                    Vertex d = thisEdge.oppositeEdge.nextEdge.v;

                    Vector2 aPos = a.GetPos2D_XZ();
                    Vector2 bPos = b.GetPos2D_XZ();
                    Vector2 cPos = c.GetPos2D_XZ();
                    Vector2 dPos = d.GetPos2D_XZ();

                    //Use the circle test to test if we need to flip this edge
                    if (IsPointInsideOutsideOrOnCircle(aPos, bPos, cPos, dPos) < 0f)
                    {
                        //Are these the two triangles that share this edge forming a convex quadrilateral?
                        //Otherwise the edge cant be flipped
                        if (IsQuadrilateralConvex(aPos, bPos, cPos, dPos))
                        {
                            //If the new triangle after a flip is not better, then dont flip
                            //This will also stop the algoritm from ending up in an endless loop
                            if (IsPointInsideOutsideOrOnCircle(bPos, cPos, dPos, aPos) < 0f)
                            {
                                continue;
                            }

                            //Flip the edge
                            flippedEdges += 1;

                            hasFlippedEdge = true;

                            FlipEdge(thisEdge);
                        }
                    }
                }

                //We have searched through all edges and havent found an edge to flip, so we have a Delaunay triangulation!
                if (!hasFlippedEdge)
                {
                    //Debug.Log("Found a delaunay triangulation");

                    break;
                }
            }

            //Debug.Log("Flipped edges: " + flippedEdges);

            //Dont have to convert from half edge to triangle because the algorithm will modify the objects, which belongs to the 
            //original triangles, so the triangles have the data we need

            return triangles;
        }

        //Flip an edge
        private static void FlipEdge(HalfEdge one)
        {
            //The data we need
            //This edge's triangle
            HalfEdge two = one.nextEdge;
            HalfEdge three = one.prevEdge;
            //The opposite edge's triangle
            HalfEdge four = one.oppositeEdge;
            HalfEdge five = one.oppositeEdge.nextEdge;
            HalfEdge six = one.oppositeEdge.prevEdge;
            //The vertices
            Vertex a = one.v;
            Vertex b = one.nextEdge.v;
            Vertex c = one.prevEdge.v;
            Vertex d = one.oppositeEdge.nextEdge.v;



            //Flip

            //Change vertex
            a.halfEdge = one.nextEdge;
            c.halfEdge = one.oppositeEdge.nextEdge;

            //Change half-edge
            //Half-edge - half-edge connections
            one.nextEdge = three;
            one.prevEdge = five;

            two.nextEdge = four;
            two.prevEdge = six;

            three.nextEdge = five;
            three.prevEdge = one;

            four.nextEdge = six;
            four.prevEdge = two;

            five.nextEdge = one;
            five.prevEdge = three;

            six.nextEdge = two;
            six.prevEdge = four;

            //Half-edge - vertex connection
            one.v = b;
            two.v = b;
            three.v = c;
            four.v = d;
            five.v = d;
            six.v = a;

            //Half-edge - triangle connection
            Triangle t1 = one.t;
            Triangle t2 = four.t;

            one.t = t1;
            three.t = t1;
            five.t = t1;

            two.t = t2;
            four.t = t2;
            six.t = t2;

            //Opposite-edges are not changing!

            //Triangle connection
            t1.v1 = b;
            t1.v2 = c;
            t1.v3 = d;

            t2.v1 = b;
            t2.v2 = d;
            t2.v3 = a;

            t1.halfEdge = three;
            t2.halfEdge = four;
        }





        public static List<Triangle> TriangulatePoints(List<Vertex> points)
        {
            List<Triangle> triangles = new List<Triangle>();

            //Sort the points along x-axis
            //OrderBy is always soring in ascending order - use OrderByDescending to get in the other order
            points = points.OrderBy(n => n.position.X).ToList();

            //The first 3 vertices are always forming a triangle
            Triangle newTriangle = new Triangle(points[0].position, points[1].position, points[2].position);

            triangles.Add(newTriangle);

            //All edges that form the triangles, so we have something to test against
            List<Edge> edges = new List<Edge>();

            edges.Add(new Edge(newTriangle.v1, newTriangle.v2));
            edges.Add(new Edge(newTriangle.v2, newTriangle.v3));
            edges.Add(new Edge(newTriangle.v3, newTriangle.v1));

            //Add the other triangles one by one
            //Starts at 3 because we have already added 0,1,2
            for (int i = 3; i < points.Count; i++)
            {
                Vector3 currentPoint = points[i].position;

                //The edges we add this loop or we will get stuck in an endless loop
                List<Edge> newEdges = new List<Edge>();

                //Is this edge visible? We only need to check if the midpoint of the edge is visible 
                for (int j = 0; j < edges.Count; j++)
                {
                    Edge currentEdge = edges[j];

                    Vector3 midPoint = (currentEdge.v1.position + currentEdge.v2.position) / 2f;

                    Edge edgeToMidpoint = new Edge(currentPoint, midPoint);

                    //Check if this line is intersecting
                    bool canSeeEdge = true;

                    for (int k = 0; k < edges.Count; k++)
                    {
                        //Dont compare the edge with itself
                        if (k == j)
                        {
                            continue;
                        }

                        if (AreEdgesIntersecting(edgeToMidpoint, edges[k]))
                        {
                            canSeeEdge = false;

                            break;
                        }
                    }

                    //This is a valid triangle
                    if (canSeeEdge)
                    {
                        Edge edgeToPoint1 = new Edge(currentEdge.v1, new Vertex(currentPoint));
                        Edge edgeToPoint2 = new Edge(currentEdge.v2, new Vertex(currentPoint));

                        newEdges.Add(edgeToPoint1);
                        newEdges.Add(edgeToPoint2);

                        Triangle newTri = new Triangle(edgeToPoint1.v1, edgeToPoint1.v2, edgeToPoint2.v1);

                        triangles.Add(newTri);
                    }
                }


                for (int j = 0; j < newEdges.Count; j++)
                {
                    edges.Add(newEdges[j]);
                }
            }
            return triangles;
        }



        private static bool AreEdgesIntersecting(Edge edge1, Edge edge2)
        {
            Vector2 l1_p1 = new Vector2(edge1.v1.position.X, edge1.v1.position.Z);
            Vector2 l1_p2 = new Vector2(edge1.v2.position.X, edge1.v2.position.Z);

            Vector2 l2_p1 = new Vector2(edge2.v1.position.X, edge2.v1.position.Z);
            Vector2 l2_p2 = new Vector2(edge2.v2.position.X, edge2.v2.position.Z);

            bool isIntersecting = AreLinesIntersecting(l1_p1, l1_p2, l2_p1, l2_p2, true);

            return isIntersecting;
        }

        //http://thirdpartyninjas.com/blog/2008/10/07/line-segment-intersection/
        public static bool AreLinesIntersecting(Vector2 l1_p1, Vector2 l1_p2, Vector2 l2_p1, Vector2 l2_p2, bool shouldIncludeEndPoints)
        {
            bool isIntersecting = false;

            float denominator = (l2_p2.Y - l2_p1.Y) * (l1_p2.X - l1_p1.X) - (l2_p2.X - l2_p1.X) * (l1_p2.Y - l1_p1.Y);

            //Make sure the denominator is > 0, if not the lines are parallel
            if (denominator != 0f)
            {
                float u_a = ((l2_p2.X - l2_p1.X) * (l1_p1.Y - l2_p1.Y) - (l2_p2.Y - l2_p1.Y) * (l1_p1.X - l2_p1.X)) / denominator;
                float u_b = ((l1_p2.X - l1_p1.X) * (l1_p1.Y - l2_p1.Y) - (l1_p2.Y - l1_p1.Y) * (l1_p1.X - l2_p1.X)) / denominator;

                //Are the line segments intersecting if the end points are the same
                if (shouldIncludeEndPoints)
                {
                    //Is intersecting if u_a and u_b are between 0 and 1 or exactly 0 or 1
                    if (u_a >= 0f && u_a <= 1f && u_b >= 0f && u_b <= 1f)
                    {
                        isIntersecting = true;
                    }
                }
                else
                {
                    //Is intersecting if u_a and u_b are between 0 and 1
                    if (u_a > 0f && u_a < 1f && u_b > 0f && u_b < 1f)
                    {
                        isIntersecting = true;
                    }
                }

            }

            return isIntersecting;
        }
    }
}
