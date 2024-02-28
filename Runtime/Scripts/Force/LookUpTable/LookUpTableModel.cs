using UnityEngine;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

namespace DefaultNamespace.LookUpTable
{
    public class LookUpTableModel : MonoBehaviour
    {
        private Rigidbody rb;

        public void Start()
        {
            rb = GetComponent<Rigidbody>();
            
        }

        public void FixedUpdate()
        {
            var Qr = transform.localRotation.To<NED>();
            
        }

      
    }
}