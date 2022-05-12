/*===================================================================================================

    Author	:	Christian Kenneth Karl Lindberg
    E-Mail	:	ckkli@kth.se
    Brief	:	An introductory GJK intersection computation.
    Note	:	Written by the author as part of a course assignment at KTH University in Stockholm.

===================================================================================================*/
using System.Collections.Generic;
using System.Linq;

namespace CLCollision
{
    public class GJK
    {
        
        public bool GJK_intersect(Collider a, Collider b)
        {
            // Get initial support point in any direction
            Vector3 support = Support(a, b, new Vector3(0.0f, 1.0f, 0.0f));

            // Create Simplex and save point
            List<Vector3> simplexPoints = new List<Vector3>(4); // TODO: use simple array
            simplexPoints.Add(support);

            // New direction towards the origin
            Vector3 direction = -support;

            while (true)
            {
                support = Support(a, b, direction);

                if (support.Dot(direction) < 0)
                    return false; // no intersection

                simplexPoints.Add(support);

                if (NextSimplex(ref simplexPoints, ref direction))
                    return true;
            }

        }

        private Vector3 Support(Collider a, Collider b, Vector3 direction)
        {
            return a.FindFurthestPoint(direction) - b.FindFurthestPoint(-direction);
        }

        private bool NextSimplex(ref List<Vector3> points, ref Vector3 direction)
        {
            switch (points.Count())
            {
                case 2: return Line(ref points, ref direction);
                case 3: return Triangle(ref points, ref direction);
                case 4: return Tetrahedron(ref points, ref direction);
            }

            return false; // should never be here
        }

        private bool SameDirection(Vector3 direction, Vector3 AO)
        {
            return direction.Dot(AO) > 0.0f;
        }

        private bool Line(ref List<Vector3> points, ref Vector3 direction)
        {
            Vector3 A = points[1];
            Vector3 B = points[0];

            Vector3 AB = B - A;
            Vector3 AO =    -A;

            if (SameDirection(AB, AO)) // AB is closest
                direction = AB.Cross(AO).Cross(AB);
            else // A is closest
            {
                points.Clear();
                points.Add(A);
                direction = AO;
            }

            return false;
        }
        private bool Triangle(ref List<Vector3> points, ref Vector3 direction)
        {
            Vector3 A = points[2];
            Vector3 B = points[1];
            Vector3 C = points[0];

            Vector3 AB = B - A;
            Vector3 AC = C - A;
            Vector3 AO =    -A;

            Vector3 ABC = AB.Cross(AC);

            if (SameDirection(ABC.Cross(AC), AO))
            {
                if(SameDirection(AC, AO))
                {
                    points.Clear();
                    points.Add(C);
                    points.Add(A); // osäker på vilken som borde in först...
                }
                else
                    goto checkAB;
            }
            else 
            { 
                if(SameDirection(AB.Cross(ABC), AO))
                    goto checkAB;
                else
                {
                    if(SameDirection(ABC, AO))
                        direction = ABC;
                    else
                    {
                        points.Clear();
                        points.Add(B);
                        points.Add(C);
                        points.Add(A);
                        direction = -ABC;
                    }
                }
            }

            return false;

            checkAB:
            points.Clear();
            points.Add(B);
            points.Add(A);
            return Line(ref points, ref direction);

        }
        private bool Tetrahedron(ref List<Vector3> points, ref Vector3 direction)
        {
            Vector3 A = points[3];
            Vector3 B = points[2];
            Vector3 C = points[1];
            Vector3 D = points[0];


            Vector3 AB = B - A;
            Vector3 AC = C - A;
            Vector3 AD = D - A;
            Vector3 AO =    -A;

            // DBC is the face we have already checked (or DCB?)
            Vector3 ABC = AB.Cross(AC);
            Vector3 ACD = AC.Cross(AD);
            Vector3 ADB = AD.Cross(AB);

            if(SameDirection(ABC, AO))
            {
                points.Clear();
                points.Add(C);
                points.Add(B);
                points.Add(A);
                return Triangle(ref points, ref direction);
            }
            if (SameDirection(ACD, AO))
            {
                points.Clear();
                points.Add(D);
                points.Add(C);
                points.Add(A);
                return Triangle(ref points, ref direction);
            }
            if (SameDirection(ADB, AO))
            {
                points.Clear();
                points.Add(B);
                points.Add(D);
                points.Add(A);
                return Triangle(ref points, ref direction);
            }

            return true;
        }
    }
}// namespace
