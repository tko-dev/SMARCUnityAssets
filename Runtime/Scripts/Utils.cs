using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using UnityEngine;

namespace DefaultNamespace
{
    public static class Utils
    {
        public static DenseMatrix Skew(this MatrixBuilder<Double> mb, Vector<double> cb)
        {
            return DenseMatrix.OfArray(new[,]
            {
                {0, -cb[2], cb[1]},
                {cb[2], 0, -cb[0]},
                {-cb[1], cb[0], 0}
            });
        }

        public static Vector3 ToVector3(this Vector<double> vec)
        {
            return new Vector3((float) vec[0], (float) vec[1], (float) vec[2]);
        }

        public static GameObject FindChildWithTag(GameObject parent, string tag)
        {
            GameObject child = null;
            foreach(Transform transform in parent.transform) {
                if(transform.CompareTag(tag)) {
                    child = transform.gameObject;
                    break;
                }
            }
            return child;
        }

        public static GameObject FindDeepChildWithTag(GameObject parent, string tag)
        {
            if(parent.transform.CompareTag(tag)) return parent;

            foreach(Transform child in parent.transform)
            {
                var result_go = FindDeepChildWithTag(child.gameObject, tag);
                if(result_go != null) return result_go;
            }
            return null;
        }


        public static GameObject FindDeepChildWithName(GameObject parent, string name)
        {
            if(parent.name == name) return parent;

            Transform result = parent.transform.Find(name);
            if(result != null) return result.gameObject;

            foreach(Transform child in parent.transform)
            {
                var result_go = FindDeepChildWithName(child.gameObject, name);
                if(result_go != null) return result_go;
            }
            return null;
        }

        public static GameObject FindParentWithTag(GameObject self, string tag, bool returnTopLevel)
        {
            Transform parent_tf = self.transform.parent;
            if(parent_tf == null)
            {
                Debug.Log("parent tf is null:"+ self.name);
                if(returnTopLevel) return self;
                else return null;
            }
            if(parent_tf.CompareTag(tag))
            {
                Debug.Log("Found tagged parent");
                return parent_tf.gameObject;
            }
            Debug.Log("Going up a level");
            return FindParentWithTag(parent_tf.gameObject, tag, returnTopLevel);
        }
    }

}