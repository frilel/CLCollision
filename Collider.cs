/*===================================================================================================

    Author	:	Christian Kenneth Karl Lindberg
    E-Mail	:	ckkli@kth.se
    Brief	:	Base class for colliders.
    Note	:	Written by the author as part of a course assignment at KTH University in Stockholm.

===================================================================================================*/

namespace CLCollision
{
    public abstract class Collider
    {
        public abstract Vector3 FindFurthestPoint(Vector3 direction);

    }
}// namespace
