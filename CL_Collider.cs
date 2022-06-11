/*===================================================================================================

    Author	:	Christian Kenneth Karl Lindberg
    E-Mail	:	ckkli@kth.se
    Brief	:	An introductory base class for colliders.
    Note	:	Written by the author as part of a course project at KTH University in Stockholm.

===================================================================================================*/

namespace clCollision
{
    public abstract class CL_Collider : UnityEngine.MonoBehaviour
    {
        internal abstract Vector3 FindFurthestPoint(Vector3 direction, ref int index);

        internal abstract Vector3 FindFurthestPoint(Vector3 direction);
    }
}// namespace
