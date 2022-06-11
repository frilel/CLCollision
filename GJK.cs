/*===================================================================================================

    Author	    :	Christian Kenneth Karl Lindberg
    E-Mail	    :	ckkli@kth.se
    Brief	    :	GJK computation based on Casey Muratori's tutorial, with added logic of computing
                    the closest points between two non-intersecting polyhedra.

    Note	    :	Written by the author as part of a course project at KTH University in Stockholm.
                    Code is not fully optimized and primarily written for the author's own 
                    learning experience and reference.
    
    References  :   Bergen, G. van den. (2003). Collision Detection in Interactive 3D Environments. 
                        CRC Press.
                    Catto, E. (2010). Computing Distance using GJK — GDC 2010. Game Developers 
                        Conference 2010. https://box2d.org/publications/
                    Chou, M.-L. (2013, December 26). Game Physics: Collision Detection – GJK | 
                        Ming-Lun “Allen” Chou | 周明倫. 
                        https://allenchou.net/2013/12/game-physics-collision-detection-gjk/
                    Ericson, C. (2004). Real-Time Collision Detection. CRC Press.
                    Muratori, C. (2006). Implementing GJK (2006). https://caseymuratori.com/blog_0003
                    Winter, I. (2020, August 29). Winter’s Blog. GJK: Collision Detection Algorithm 
                        in 2D/3D. https://blog.winter.dev/2020/gjk-algorithm/

===================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;

namespace clCollision
{
    public static class GJK
    {
        /// <summary>
        /// Enables debug lines.
        /// </summary>
        public static bool DRAWLINES = true;

        /// <summary>
        /// Contains data returned by the GJK function.
        /// </summary>
        public struct Output 
        {
            /// <summary>
            /// Closest points 1 & 2 valid if false.
            /// </summary>
            public bool? Intersection;

            /// <summary>
            /// Closest point on collider "a".
            /// </summary>
            public Vector3 Point1;

            /// <summary>
            /// Closest point on collider "b".
            /// </summary>
            public Vector3 Point2;

            /// <summary>
            /// Number of GJK main loop iterations before termination.
            /// </summary>
            public int Iterations;

            /// <summary>
            /// What expression terminated the algorithm.
            /// </summary>
            public int TerminationID;
        }

        internal struct SimplexVertex
        {
            internal Vector3 point1;  // support point in polygon1
            internal Vector3 point2;  // support point in polygon2
            internal Vector3 point;   // point on Minkowski hull: point1 - point2
            internal float u;         // unnormalized barycentric coordinate for closest point
            internal int index1;      // point1 index in original shape
            internal int index2;      // point2 index in original shape
        }

        public static Output GJK_intersect(CL_Collider a, CL_Collider b)
        {
            Output output = new Output();
            output.Intersection = null;

            SimplexVertex curSupportVertex = new SimplexVertex();

            // Get initial support point in any direction
            Support(a, b, new Vector3(0.0f, 1.0f, 0.0f), ref curSupportVertex);

            // New direction towards the origin
            Vector3 direction = -curSupportVertex.point;

            // Our simplex
            List<SimplexVertex> simplexVertices = new List<SimplexVertex>(4) { curSupportVertex };

            // These store the vertices of the last simplex so that
            // we can check for duplicates and prevent cycling.
            int[] prevIndices1 = new int[3];
            int[] prevIndices2 = new int[3];
            int saveCount = 0;

            // Main loop
            const int maxIters = 100;
            for(int iter = 1; iter < maxIters; iter++)
            {
                output.Iterations = iter;

                // Copy simplex indices so we can identify duplicates.
                saveCount = simplexVertices.Count();
                for (int i = 0; i < saveCount; ++i)
                {
                    prevIndices1[i] = simplexVertices[i].index1;
                    prevIndices2[i] = simplexVertices[i].index2;
                }

                // Get next support
                Support(a, b, direction, ref curSupportVertex);

                //// TERMINATION 1 main termination
                // Check for duplicate support points.
                bool duplicate = false;
                for (int i = 0; i < saveCount; ++i)
                {
                    if (curSupportVertex.index1 == prevIndices1[i] &&
                        curSupportVertex.index2 == prevIndices2[i])
                    {
                        duplicate = true;
                        break;
                    }
                }
                // If we found a duplicate support point we must exit to avoid cycling.
                if (duplicate)
                {
                    output.Intersection = false;
                    output.TerminationID = 1;
                    break;
                }

                //// TERMINATION 2
                // The current support point is overlapping the origin.
                if (direction.Dot(direction) == 0.0f)
                {
                    output.Intersection = false;
                    output.TerminationID = 2;
                    break;
                }

                // New vertex is ok and needed.
                simplexVertices.Add(curSupportVertex);

                // Determine if simplex contains the origin. Remove unused vertices and continue loop if not.
                if (NextSimplex(ref simplexVertices, ref direction))
                {
                    output.Intersection = true;
                    output.TerminationID = 0;
                    break;
                }

                // At the last allowed iteration
                if (output.Iterations == maxIters-1)
                {
                    UnityEngine.Debug.LogWarning(
                        $"clCollision.GJK query between {a.transform.name} and {b.transform.name}, exceeded its maximum number of iterations.");
                    if (DRAWLINES) DrawToSimplexPoints(ref simplexVertices);
                }

            } // Main loop

            if (output.Intersection != null && output.Intersection == false)
            {
                // If we do not intersect we want to calculate the barycentric coordinates
                // and apply those as weights (to the witness points) to get the closest points.
                ComputeClosestPoints(ref simplexVertices, ref output);
            }

            return output;
        }

        private static void Support(CL_Collider a, CL_Collider b, Vector3 direction, ref SimplexVertex simplexVertex)
        {
            simplexVertex.point1 = a.FindFurthestPoint(direction, ref simplexVertex.index1);
            simplexVertex.point2 = b.FindFurthestPoint(-direction, ref simplexVertex.index2);
            simplexVertex.point = simplexVertex.point1 - simplexVertex.point2;

            // Lines with direction of search
            if (DRAWLINES)
            {
                UnityEngine.Debug.DrawRay(a.transform.position, direction.ToUnityVec(), UnityEngine.Color.blue);
                UnityEngine.Debug.DrawRay(b.transform.position, -direction.ToUnityVec(), UnityEngine.Color.blue);
            }
        }

        private static bool NextSimplex(ref List<SimplexVertex> simplex, ref Vector3 direction)
{
            switch (simplex.Count())
{
                case 2: return Line(ref simplex, ref direction);
                case 3: return Triangle(ref simplex, ref direction);
                case 4: return Tetrahedron(ref simplex, ref direction);
            }

            UnityEngine.Debug.LogWarning("clCollision.GJK simplex out of bounds!");
            return false; // should never be here
        }

        private static bool Line(ref List<SimplexVertex> simplex, ref Vector3 direction)
        {
            SimplexVertex a = simplex[1];
            SimplexVertex b = simplex[0];

            Vector3 A = a.point;
            Vector3 B = b.point;

            Vector3 AB = B - A;
            Vector3 AO =    -A;

            if (SameDirection(AB, AO)) // Region AB
            {
                direction = AB.Cross(AO).Cross(AB);
            } else // Region A
            {
                simplex.Clear();
                simplex.Add(a);
                direction = AO;
            }

            return false;
        }

        private static bool Triangle(ref List<SimplexVertex> simplex, ref Vector3 direction)
        {
            SimplexVertex a = simplex[2];
            SimplexVertex b = simplex[1];
            SimplexVertex c = simplex[0];

            Vector3 A = a.point;
            Vector3 B = b.point;
            Vector3 C = c.point;

            Vector3 AB = B - A;
            Vector3 AC = C - A;
            Vector3 AO =   - A;

            Vector3 ABC = AB.Cross(AC);

            if (SameDirection(ABC.Cross(AC), AO))
            {
                if(SameDirection(AC, AO))
                {
                    simplex.Clear();
                    simplex.Add(c);
                    simplex.Add(a);
                    direction = AC.Cross(AO).Cross(AC);
                }
                else
                    goto CheckAB;
            }
            else 
            { 
                if(SameDirection(AB.Cross(ABC), AO))
                    goto CheckAB;
                else
                {
                    if(SameDirection(ABC, AO))
                        direction = ABC;
                    else
                    {
                        simplex.Clear();
                        simplex.Add(b);
                        simplex.Add(c);
                        simplex.Add(a);
                        direction = -ABC;
                    }
                }
            }

            return false;

        CheckAB:
            simplex.Clear();
            simplex.Add(b);
            simplex.Add(a);
            return Line(ref simplex, ref direction);
        }

        private static bool Tetrahedron(ref List<SimplexVertex> simplex, ref Vector3 direction)
        {
            SimplexVertex a = simplex[3];
            SimplexVertex b = simplex[2];
            SimplexVertex c = simplex[1];
            SimplexVertex d = simplex[0];

            Vector3 A = a.point;
            Vector3 B = b.point;
            Vector3 C = c.point;
            Vector3 D = d.point;

            Vector3 AB = B - A;
            Vector3 AC = C - A;
            Vector3 AD = D - A;
            Vector3 AO =    -A;

            // DCB is the face we have already checked
            Vector3 ABC = AB.Cross(AC);
            Vector3 ACD = AC.Cross(AD);
            Vector3 ADB = AD.Cross(AB);

            if(SameDirection(ABC, AO))
            {
                simplex.Clear();
                simplex.Add(c);
                simplex.Add(b);
                simplex.Add(a);
                return Triangle(ref simplex, ref direction);
            }
            if (SameDirection(ACD, AO))
            {
                simplex.Clear();
                simplex.Add(d);
                simplex.Add(c);
                simplex.Add(a);
                return Triangle(ref simplex, ref direction);
            }
            if (SameDirection(ADB, AO))
            {
                simplex.Clear();
                simplex.Add(b);
                simplex.Add(d);
                simplex.Add(a);
                return Triangle(ref simplex, ref direction);
            }

            return true; // origin is enclosed
        }

        private static bool SameDirection(Vector3 direction, Vector3 AO)
        {
            return direction.Dot(AO) > 0.0f;
        }

        /// <summary>
        /// After the algorithm of finding the simplex closest to the origin, 
        /// this function runs algorithms for computing a point on the simplex
        /// closest to origin.
        /// </summary>
        private static void ComputeClosestPoints(ref List<SimplexVertex> simplex, ref Output output)
        {
            Vector3 origin = new Vector3(0.0f, 0.0f, 0.0f);

            switch (simplex.Count())
            {
                case 1:
                    output.Point1 = simplex[0].point1;
                    output.Point2 = simplex[0].point2;
                    if (DRAWLINES) DrawToSimplexPoints(ref simplex);
                    break;
                case 2:
                    Solve2(origin, ref simplex, ref output);
                    break;
                case 3:
                    Solve3(origin, ref simplex, ref output);
                    break;
                default:
                    UnityEngine.Debug.LogWarning("clCollision.GJK simplex out of bounds!");
                    break;
            }
        }

        /// <summary>
        /// Closest point on line segment to query point Q.
        /// Computed as barycentric coordinates for vertexA & vertexB.
        /// Applied as weights to the witness points.
        /// Voronoi regions: A, B, AB
        /// </summary>
        private static void Solve2(Vector3 Q, ref List<SimplexVertex> simplex, ref Output output)
        {
            SimplexVertex vertexA = simplex[1];
            SimplexVertex vertexB = simplex[0];

            Vector3 A = vertexA.point;
            Vector3 B = vertexB.point;

            // Compute barycentric coordinates (pre-division).
            float u = (Q - B).Dot(A - B);
            float v = (Q - A).Dot(B - A);

            // Region A
            if (v <= 0.0f)
            {
                output.Point1 = vertexA.point1;
                output.Point2 = vertexA.point2;
                if (DRAWLINES) DrawToSimplexPoints(ref simplex);
                return;
            }

            // Region B
            if (u <= 0.0f)
            {
                output.Point1 = vertexB.point1;
                output.Point2 = vertexB.point2;
                if (DRAWLINES) DrawToSimplexPoints(ref simplex);
                return;
            }

            // Region AB
            Vector3 AB = B - A;
            float divisor = AB.Dot(AB);

            // if edge has zero length, both points are on the same position?
            if (divisor == 0.0f) divisor = 0.5f;

            float f = 1.0f / divisor; // turn into factor to avoid multiple divisions
            vertexA.u = u * f;
            vertexB.u = v * f;

            output.Point1 = (vertexA.u * vertexA.point1) + (vertexB.u * vertexB.point1);
            output.Point2 = (vertexA.u * vertexA.point2) + (vertexB.u * vertexB.point2); // tvärtom?
            if (DRAWLINES) DrawToSimplexPoints(ref simplex);
        }

        /// <summary>
        /// Determine what Voronoi region point Q is closest to based on Ericson's (2004) example 
        /// of computing "ClosestPtPointTriangle" p.139-140. 
        /// 
        /// If the point is closest to the triangle face it computes barycentric coordinates 
        /// of the simplex vertices, applies those weights to the vertices witness points and 
        /// saves it in output.
        /// 
        /// 
        /// Voronoi regions: A, B, C, AB, BC, CA, ABC
        /// 
        /// References: (Ericson, 2004)
        /// </summary>
        private static void Solve3(Vector3 Q, ref List<SimplexVertex> simplex, ref Output output)
        {
            SimplexVertex vertexA = simplex[2];
            SimplexVertex vertexB = simplex[1];
            SimplexVertex vertexC = simplex[0];

            Vector3 A = vertexA.point;
            Vector3 B = vertexB.point;
            Vector3 C = vertexC.point;

            Vector3 AB = B - A;
            Vector3 AC = C - A;
            Vector3 BC = C - B;

            // Degenerate case: Collinear vertices
            // We could end up with a degenerate triangular simplex with zero area.
            // We avoid dividing the bary-coords with zero area by ensuring that the
            // triangle solver will pick a vertex or edge region in this case.
            // (Catto, 2010)
            double area = (MathUtils.Cross(B - A, C - A)).GetSqrMagnitude(); // pre sqrt and halved
            if (area <= 0.0f)
            {
                // The three points should all be in a line and thus we should be able to
                // just pick two random points and solve the linear interpolation case.
                simplex.Clear();
                simplex.Add(vertexC);
                simplex.Add(vertexB);
                Solve2(Q, ref simplex, ref output);
                return;
            }

            // Region A
            // Compute parametric position s for projection Q’ of Q on AB,
            // Q’ = A + s*AB, s = snom/(snom+sdenom)
            float snom = MathUtils.Dot(Q - A, AB);
            float sdenom = MathUtils.Dot(Q - B, A - B);
            // Compute parametric position t for projection Q’ of Q on AC,
            // Q’ = A + t*AC, s = tnom/(tnom+tdenom)
            float tnom = MathUtils.Dot(Q - A, AC);
            float tdenom = MathUtils.Dot(Q - C, A - C);

            if (snom <= 0.0f && tnom <= 0.0f)
            {
                output.Point1 = vertexA.point1;
                output.Point2 = vertexA.point2;
                if (DRAWLINES) DrawToSimplexPoints(ref simplex);
                return;
            }

            // Region B
            // Compute parametric position u for projection Q’ of Q on BC,
            // Q’ = B + u*BC, u = unom/(unom+udenom)
            float unom = MathUtils.Dot(Q - B, BC);
            float udenom = MathUtils.Dot(Q - C, B - C);
            if (sdenom <= 0.0f && unom <= 0.0f)
            {
                output.Point1 = vertexB.point1;
                output.Point2 = vertexB.point2;
                if (DRAWLINES) DrawToSimplexPoints(ref simplex);
                return;
            }

            // Region C
            if (tdenom <= 0.0f && udenom <= 0.0f)
            {
                output.Point1 = vertexC.point1;
                output.Point2 = vertexC.point2;
                if (DRAWLINES) DrawToSimplexPoints(ref simplex);
                return;
            }

            // Region AB
            // P is outside (or on) AB if the triple scalar product [N QA QB] <= 0
            Vector3 N = MathUtils.Cross(B - A, C - A);
            float vc = MathUtils.Dot(N, MathUtils.Cross(A - Q, B - Q));
            if (vc <= 0.0f && snom >= 0.0f && sdenom >= 0.0f)
            {
                simplex.Clear();
                simplex.Add(vertexB);
                simplex.Add(vertexA);
                Solve2(Q, ref simplex, ref output);
                return;
            }

            // Region BC
            // P is outside (or on) BC if the triple scalar product [N QB QC] <= 0
            float va = MathUtils.Dot(N, MathUtils.Cross(B - Q, C - Q));
            if (va <= 0.0f && unom >= 0.0f && udenom >= 0.0f)
            {
                simplex.Clear();
                simplex.Add(vertexC);
                simplex.Add(vertexB);
                Solve2(Q, ref simplex, ref output);
                return;
            }

            // Region CA
            // P is outside (or on) CA if the triple scalar product [N QC QA] <= 0
            float vb = MathUtils.Dot(N, MathUtils.Cross(C - Q, A - Q));
            if (vb <= 0.0f && tnom >= 0.0f && tdenom >= 0.0f)
            {
                simplex.Clear();
                simplex.Add(vertexA);
                simplex.Add(vertexC);
                Solve2(Q, ref simplex, ref output);
                return;
            }

            // Region ABC
            // P must project inside face region. Compute point using barycentric coordinates
            float u = va / (va + vb + vc);
            float v = vb / (va + vb + vc);
            float w = 1.0f - u - v; // = vc / (va + vb + vc)

            vertexA.u = u;
            vertexB.u = v;
            vertexC.u = w;

            output.Point1 = (vertexA.u * vertexA.point1) + (vertexB.u * vertexB.point1) + (vertexC.u * vertexC.point1);
            output.Point2 = (vertexA.u * vertexA.point2) + (vertexB.u * vertexB.point2) + (vertexC.u * vertexC.point2);
            if (DRAWLINES) DrawToSimplexPoints(ref simplex);
        }

        // DEBUG

        private static void DrawToSimplexPoints(ref List<SimplexVertex> simplex)
        {
            for (int i = 0; i < simplex.Count(); i++)
            {
                UnityEngine.Debug.DrawRay(UnityEngine.Vector3.zero, simplex[i].point2.ToUnityVec(), UnityEngine.Color.red);
                UnityEngine.Debug.DrawRay(UnityEngine.Vector3.zero, simplex[i].point1.ToUnityVec(), UnityEngine.Color.red);
            }
        }

        // UNUSED

        /// <summary>
        /// Determine what Voronoi region point Q is closest to based on Catto's (2010) approach
        /// of using signed areas. 
        /// 
        /// If the point is closest to the triangle face it computes barycentric coordinates 
        /// of the simplex vertices, applies those weights to the vertices witness points and 
        /// saves it in output.
        /// 
        /// Note: Does not work properly.
        /// 
        /// Voronoi regions: A, B, C, AB, BC, CA, ABC
        /// 
        /// References: (Catto, 2010)
        /// </summary>
        private static void Solve3b(Vector3 Q, ref List<SimplexVertex> simplex, ref Output output)
        {
            // Degenerate case: Collinear vertices
            // We could end up with a degenerate triangular simplex with zero area.
            // We avoid dividing the bary-coords with zero area by ensuring that the
            // triangle solver will pick a vertex or edge region in this case.
            // Consider the lowest dimensional features first:
            // vertices, then edges, then the triangle’s interior.
            // (Catto, 2010)

            SimplexVertex vertexA = simplex[2];
            SimplexVertex vertexB = simplex[1];
            SimplexVertex vertexC = simplex[0];

            Vector3 A = vertexA.point;
            Vector3 B = vertexB.point;
            Vector3 C = vertexC.point;

            // Compute edge barycentric coordinates (pre-division).
            float uAB = MathUtils.Dot(Q - B, A - B);
            float vAB = MathUtils.Dot(Q - A, B - A);

            float uBC = MathUtils.Dot(Q - C, B - C);
            float vBC = MathUtils.Dot(Q - B, C - B);

            float uCA = MathUtils.Dot(Q - A, C - A);
            float vCA = MathUtils.Dot(Q - C, A - C);

            // Region A
            if (vAB <= 0.0f && uCA <= 0.0f)
            {
                output.Point1 = vertexA.point1;
                output.Point2 = vertexA.point2;
                if(DRAWLINES) DrawToSimplexPoints(ref simplex);
                return;
            }

            // Region B
            if (uAB <= 0.0f && vBC <= 0.0f)
            {
                output.Point1 = vertexB.point1;
                output.Point2 = vertexB.point2;
                if (DRAWLINES) DrawToSimplexPoints(ref simplex);
                return;
            }

            // Region C
            if (uBC <= 0.0f && vCA <= 0.0f)
            {
                output.Point1 = vertexC.point1;
                output.Point2 = vertexC.point2;
                if (DRAWLINES) DrawToSimplexPoints(ref simplex);
                return;
            }

            // Compute (signed?) triangle area.
            float area = (float)(0.5 * (MathUtils.Cross(B - A, C - A)).GetMagnitude());

            // Compute triangle barycentric coordinates (pre-division).
            Vector3 nABC = MathUtils.Normalize(MathUtils.Cross(B-A, C-A)); // triangle normal
            float uQBC = MathUtils.Dot(MathUtils.Cross(B-Q, C-Q), nABC);
            float vQCA = MathUtils.Dot(MathUtils.Cross(C-Q, A-Q), nABC);
            float wQAB = MathUtils.Dot(MathUtils.Cross(A-Q, B-Q), nABC);

            // Region AB
            if (uAB > 0.0f && vAB > 0.0f && wQAB * area <= 0.0f)
            {
                simplex.Clear();
                simplex.Add(vertexB);
                simplex.Add(vertexA);
                Solve2(Q, ref simplex, ref output);
                return;
            }

            // Region BC
            if (uBC > 0.0f && vBC > 0.0f && uQBC * area <= 0.0f)
            {
                simplex.Clear();
                simplex.Add(vertexC);
                simplex.Add(vertexB);
                Solve2(Q, ref simplex, ref output);
                return;
            }

            // Region CA
            if (uCA > 0.0f && vCA > 0.0f && vQCA * area <= 0.0f)
            {
                simplex.Clear();
                simplex.Add(vertexA);
                simplex.Add(vertexC);
                Solve2(Q, ref simplex, ref output);
                return;
            }

            // Region ABC
            //BarycentricProjection(A, B, C, Q, ref vertexA.u, ref vertexB.u, ref vertexC.u);
            BarycentricCramer(A, B, C, Q, ref vertexA.u, ref vertexB.u, ref vertexC.u);

            output.Point1 = (vertexA.u * vertexA.point1) + (vertexB.u * vertexB.point1) + (vertexC.u * vertexC.point1);
            output.Point2 = (vertexA.u * vertexA.point2) + (vertexB.u * vertexB.point2) + (vertexC.u * vertexC.point2);
            if (DRAWLINES) DrawToSimplexPoints(ref simplex);
        }

        /// <summary>
        /// Determine what Voronoi region point Q is closest to based on Ericson's (2004) example 
        /// of computing "ClosestPtPointTriangle" p.141-142 with optimized performance. 
        /// 
        /// If the point is closest to the triangle face it computes barycentric coordinates 
        /// of the simplex vertices, applies those weights to the vertices witness points and 
        /// saves it in output.
        /// 
        /// Note: Does not work properly.
        /// 
        /// Voronoi regions: A, B, C, AB, BC, CA, ABC
        /// 
        /// References: (Ericson, 2004)
        /// </summary>
        private static void Solve3c(Vector3 Q, ref List<SimplexVertex> simplex, ref Output output)
        {
            SimplexVertex vertexA = simplex[2];
            SimplexVertex vertexB = simplex[1];
            SimplexVertex vertexC = simplex[0];

            Vector3 A = vertexA.point;
            Vector3 B = vertexB.point;
            Vector3 C = vertexC.point;

            Vector3 AB = B - A;
            Vector3 AC = C - A;
            Vector3 AQ = Q - A;

            // Region A
            float d1 = MathUtils.Dot(AB, AQ);
            float d2 = MathUtils.Dot(AC, AQ);
            if (d1 <= 0.0f && d2 <= 0.0f) // barycentric coordinates (1,0,0)
            {
                output.Point1 = vertexA.point1;
                output.Point2 = vertexA.point2;
                if (DRAWLINES) DrawToSimplexPoints(ref simplex);
                return;
            }

            // Region B
            Vector3 BQ = Q - B;
            float d3 = MathUtils.Dot(AB, BQ);
            float d4 = MathUtils.Dot(AC, BQ);
            if (d3 >= 0.0f && d4 <= d3) // barycentric coordinates (0,1,0)
            {
                output.Point1 = vertexA.point1;
                output.Point2 = vertexA.point2;
                if (DRAWLINES) DrawToSimplexPoints(ref simplex);
                return;
            }
            
            // Region AB
            float vc = d1*d4 - d3*d2;

            if (vc <= 0.0f && d1 >= 0.0f && d3 <= 0.0f) // barycentric coordinates (1-v,v,0)
            {
                float V = d1 / (d1 - d3);

                output.Point1 = vertexA.point1 + V * AB;
                output.Point2 = vertexA.point2 + V * AB;
                if (DRAWLINES) DrawToSimplexPoints(ref simplex);
                return; 
            }

            // Region C
            Vector3 cp = Q - C;
            float d5 = MathUtils.Dot(AB, cp);
            float d6 = MathUtils.Dot(AC, cp);
            if (d6 >= 0.0f && d5 <= d6) // barycentric coordinates (0,0,1)
            {
                output.Point1 = vertexC.point1;
                output.Point2 = vertexC.point2;
                if (DRAWLINES) DrawToSimplexPoints(ref simplex);
                return;
            }

            // Region AC
            float vb = d5*d2 - d1*d6;
            if (vb <= 0.0f && d2 >= 0.0f && d6 <= 0.0f) // barycentric coordinates (1-w,0,w)
            {
                float W = d2 / (d2 - d6);

                output.Point1 = vertexA.point1 + W * AC;
                output.Point2 = vertexA.point2 + W * AC;
                if (DRAWLINES) DrawToSimplexPoints(ref simplex);
                return; 
            }

            // Region BC
            float va = d3*d6 - d5*d4;
            if (va <= 0.0f && (d4 - d3) >= 0.0f && (d5 - d6) >= 0.0f) // barycentric coordinates (0,1-w,w)
            {
                float W = (d4 - d3) / ((d4 - d3) + (d5 - d6));

                output.Point1 = vertexB.point1 + W * (C - B);
                output.Point2 = vertexB.point2 + W * (C - B);
                if (DRAWLINES) DrawToSimplexPoints(ref simplex);
                return; 
            }

            // Region ABC barycentric coordinates (u,v,w)
            float denom = 1.0f / (va + vb + vc);
            float v = vb * denom;
            float w = vc * denom;
            output.Point1 = vertexA.point1 + AB * v + AC * w; ; // = u*a + v*b + w*c, u = va * denom = 1.0f - v - w
            output.Point2 = vertexA.point2 + AB * v + AC * w; ;
            if (DRAWLINES) DrawToSimplexPoints(ref simplex);
        }

        /// <summary>
        /// Compute barycentric coordinates (u, v, w) for
        /// point Q with respect to triangle (a, b, c)
        /// using Cramer's rule as exemplified by Ericson (2004).
        /// 
        /// Reference: Ericson (2004) p.47
        /// </summary>
        private static void BarycentricCramer
            (Vector3 A, Vector3 B, Vector3 C, Vector3 Q, ref float u, ref float v, ref float w)
        {
            Vector3 v0 = B - A, v1 = C - A, v2 = Q - A;
            float d00 = v0.Dot(v0);
            float d01 = v0.Dot(v1);
            float d11 = v1.Dot(v1);
            float d20 = v2.Dot(v0);
            float d21 = v2.Dot(v1);
            float denom = d00 * d11 - d01 * d01;
            v = (d11 * d20 - d01 * d21) / denom;
            w = (d00 * d21 - d01 * d20) / denom;
            u = 1.0f - v - w;
        }

        /// <summary>
        /// Compute barycentric coordinates (u, v, w) for
        /// point Q with respect to triangle (a, b, c)
        /// using Ericson's projection method.
        /// 
        /// Reference: Ericson (2004) p.51
        /// </summary>
        private static void BarycentricProjection
            (Vector3 A, Vector3 B, Vector3 C, Vector3 Q, ref float u, ref float v, ref float w)
        {
            /* "An important property of barycentric coordinates is that they remain invariant
             * under projection. This property allows for a potentially more efficient way of computing
             * the coordinates than given earlier. Instead of computing the areas from the
             * 3D coordinates of the vertices, the calculations can be simplified by projecting all
             * vertices to the xy, xz, or yz plane. To avoid degeneracies, the projection is made to
             * the plane where the projected areas are the greatest. The largest absolute component
             * value of the triangle normal indicates which component should be dropped during projection."
             * (Ericson, Real-Time Collision Detection, p.51)
             */

            // Unnormalized triangle normal
            Vector3 m = MathUtils.Cross(B - A, C - A);
            // Nominators and one-over-denominator for u and v ratios
            float nu, nv, ood;
            // Absolute components for determining projection plane
            float x = Math.Abs(m.x), y = Math.Abs(m.y), z = Math.Abs(m.z);
            // Compute areas in plane of largest projection
            if (x >= y && x >= z)
            {
                // x is largest, project to the yz plane
                nu = TriArea2D(Q.y, Q.z, B.y, B.z, C.y, C.z); // Area of PBC in yz plane
                nv = TriArea2D(Q.y, Q.z, C.y, C.z, A.y, A.z); // Area of PCA in yz plane
                ood = 1.0f / m.x; // 1/(2*area of ABC in yz plane)
            } else if (y >= x && y >= z)
            {
                // y is largest, project to the xz plane
                nu = TriArea2D(Q.x, Q.z, B.x, B.z, C.x, C.z);
                nv = TriArea2D(Q.x, Q.z, C.x, C.z, A.x, A.z);
                ood = 1.0f / -m.y;
            } else
            {
                // z is largest, project to the xy plane
                nu = TriArea2D(Q.x, Q.y, B.x, B.y, C.x, C.y);
                nv = TriArea2D(Q.x, Q.y, C.x, C.y, A.x, A.y);
                ood = 1.0f / m.z;
            }
            u = nu * ood;
            v = nv * ood;
            w = 1.0f - u - v;
        }

        private static float TriArea2D(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            return (x1-x2)*(y2-y3) - (x2-x3)*(y1-y2);
        }
    }
}// namespace
