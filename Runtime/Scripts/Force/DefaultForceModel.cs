using UnityEngine;

namespace DefaultNamespace
{
    public class DefaultForceModel : MonoBehaviour, IForceModel
    {
        private Rigidbody _rigidbody;
        public float waterDrag = 0.5f;
        public float waterAngularDrag = 0.5f;
        private void Awake()
        {
            _rigidbody = new Rigidbody();
        }

        private void FixedUpdate()
        {

            _rigidbody.AddForce(-_rigidbody.velocity
                                * waterDrag, ForceMode.Acceleration);
            _rigidbody.AddTorque(-_rigidbody.angularVelocity
                                 * waterAngularDrag, ForceMode.Acceleration);

        }
        public Vector3 GetTorqueDamping()
        {
            throw new System.NotImplementedException();
        }
        public Vector3 GetForceDamping()
        {
            throw new System.NotImplementedException();
        }
    }
}
