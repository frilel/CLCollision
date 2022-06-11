/*===================================================================================================

    Author	:	Christian Kenneth Karl Lindberg
    E-Mail	:	ckkli@kth.se
    Brief	:	An introductory MeshCollider consisting of vertices from a mesh.
    Note	:	Written by the author as part of a course project at KTH University in Stockholm.

===================================================================================================*/
using System.Collections.Generic;
using UnityEngine;

namespace clCollision
{
    
    [RequireComponent(typeof(MeshFilter))]
    public class CL_MeshCollider : CL_Collider
    {
        public Vector3[] Vertices;

        private void Awake()
        {
            UnityEngine.Vector3[] meshVertices = GetComponent<MeshFilter>().sharedMesh.vertices; 
            Vertices = new Vector3[meshVertices.Length];

            for (int i = 0; i < meshVertices.Length; i++)
                Vertices[i] = meshVertices[i].ToCLVec();
        }

        internal override Vector3 FindFurthestPoint(Vector3 direction, ref int index)
        {
            Vector3 maxPoint = new Vector3();
            float maxDistance = float.MinValue;

            // The direction has to be rotated with the transform's rotation to get the correct direction
            Matrix4x4 rotation = Matrix4x4.Rotate(this.transform.rotation);
            rotation = rotation.transpose; // don't understand underlying reason why it needs to be transposed however
            UnityEngine.Vector3 worldDir = rotation.MultiplyVector(direction.ToUnityVec());

            direction = worldDir.ToCLVec();

            // find vertex with highest Dot (=furthest) in direction
            int verticesCount = Vertices.Length;
            for (int i = 0; i < verticesCount; i++)
            {
                float distance = Vertices[i].Dot(direction);

                if(distance > maxDistance)
                {
                    maxDistance = distance;
                    maxPoint = Vertices[i];
                    index = i;
                }
            }

            // transform local point to world
            //UnityEngine.Vector3 pointInWorld = this.transform.localToWorldMatrix.MultiplyPoint3x4(maxPoint.ToUnityVec()); // also works
            UnityEngine.Vector3 pointInWorld = this.transform.TransformPoint(maxPoint.ToUnityVec());
            maxPoint = pointInWorld.ToCLVec();

            return maxPoint;
        }

        internal override Vector3 FindFurthestPoint(Vector3 direction)
        {
            Vector3 maxPoint = new Vector3();
            float maxDistance = float.MinValue;

            // The direction has to be rotated with the transform's rotation to get the correct direction
            Matrix4x4 rotation = Matrix4x4.Rotate(this.transform.rotation);
            rotation = rotation.transpose;

            UnityEngine.Vector3 worldDir = rotation.MultiplyVector(direction.ToUnityVec());
            direction = worldDir.ToCLVec();

            // find vertex with highest Dot (=furthest) in direction
            int verticesCount = Vertices.Length;
            for (int i = 0; i < verticesCount; i++)
            {
                float distance = Vertices[i].Dot(direction);

                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    maxPoint = Vertices[i];
                }
            }

            // transform local point to world
            //UnityEngine.Vector3 pointInWorld = this.transform.localToWorldMatrix.MultiplyPoint3x4(maxPoint.ToUnityVec()); // also works
            UnityEngine.Vector3 pointInWorld = this.transform.TransformPoint(maxPoint.ToUnityVec());
            maxPoint = pointInWorld.ToCLVec();

            return maxPoint;
        }

    }
}// namespace
