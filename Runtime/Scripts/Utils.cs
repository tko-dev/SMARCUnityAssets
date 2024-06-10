using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
               
        // From https://forum.unity.com/threads/random-number-with-normal-distribution-passing-average-value.1229193/
        /// <summary>
        /// A gaussian or normal distribution.
        /// </summary>
        public class NormalDistribution
        {
    
            private double m_factor;
    
            public NormalDistribution(double mean, double sigma)
            {
                Mean = mean;
                Sigma = sigma;
                Variance = sigma * sigma;
            }
    
            public double Mean { get; private set; }
    
            public double Variance { get; private set; }
    
            public double Sigma { get; private set; }
    
            private bool m_useLast;
    
            private double m_y2;
    
            /// <summary>
            /// Sample a value from distribution for a given random varible.
            /// </summary>
            /// <returns>A value from the distribution</returns>
            public double Sample()
            {
                double x1, x2, w, y1;
    
                if (m_useLast)
                {
                    y1 = m_y2;
                    m_useLast = false;
                }
                else
                {
                    do
                    {
                        x1 = 2.0 * Random.value - 1.0;
                        x2 = 2.0 * Random.value - 1.0;
                        w = x1 * x1 + x2 * x2;
                    }
                    while (w >= 1.0);
    
                    w = Math.Sqrt(-2.0 * Math.Log(w) / w);
                    y1 = x1 * w;
                    m_y2 = x2 * w;
                    m_useLast = true;
                }
    
                return Mean + y1 * Sigma;
            }
    
        }


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

        public static string GetGameObjectPath(GameObject obj)
        {
            string path = "/" + obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            return path;
        }

        
    }

}