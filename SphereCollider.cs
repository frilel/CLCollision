/*===================================================================================================

    Author	:	Christian Kenneth Karl Lindberg
    E-Mail	:	ckkli@kth.se
    Brief	:	A WIP SphereCollider. Currently not usable with the GJK.
    Note	:	Written by the author as part of a course project at KTH University in Stockholm.

===================================================================================================*/

namespace clCollision
{
    public class CL_SphereCollider : CL_Collider
    {
        public Vector3 Center;
        public float Radius;

        internal override Vector3 FindFurthestPoint(Vector3 direction, ref int index)
        {
            Vector3 furthestPoint = new Vector3(direction, Center);
            furthestPoint.SetMagnitude(Radius);
            return this.transform.position + furthestPoint;
        }

        internal override Vector3 FindFurthestPoint(Vector3 direction)
        {
            throw new System.NotImplementedException();
        }
    }
}// namespace
