/*===================================================================================================

    Author	:	Christian Kenneth Karl Lindberg
    E-Mail	:	ckkli@kth.se
    Brief	:	Collider consisting of vertices.
    Note	:	Written by the author as part of a course assignment at KTH University in Stockholm.

===================================================================================================*/
using System.Collections.Generic;

namespace CLCollision
{
    class MeshCollider : Collider
    {
        private readonly List<Vector3> vertices;

        public override Vector3 FindFurthestPoint(Vector3 direction)
        {
            Vector3 maxPoint = new Vector3();
            float maxDistance = float.MinValue; // double?

            // iterate through all vertices to find furthest
            for (int i = 0; i < vertices.Count; i++)
            {
                float distance = vertices[i].Dot(direction);
                if(distance > maxDistance)
                {
                    maxDistance = distance;
                    maxPoint = vertices[i];
                }
            }

            return maxPoint;
        }
    }
}// namespace
