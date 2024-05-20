using UnityEngine;

namespace DefaultNamespace
{
    public static class Extensions
    {

        public static void ResetArticulationBody(this ArticulationBody body)
        {
            switch (body.dofCount)
            {
                case 1:
                    body.jointPosition = new ArticulationReducedSpace(0f);
                    body.jointForce = new ArticulationReducedSpace(0f);
                    body.jointVelocity = new ArticulationReducedSpace(0f);
                    break;
                case 2:
                    body.jointPosition = new ArticulationReducedSpace(0f, 0f);
                    body.jointForce = new ArticulationReducedSpace(0f, 0f);
                    body.jointVelocity = new ArticulationReducedSpace(0f, 0f);
                    break;
                case 3:
                    body.jointPosition = new ArticulationReducedSpace(0f, 0f, 0f);
                    body.jointForce = new ArticulationReducedSpace(0f, 0f, 0f);
                    body.jointVelocity = new ArticulationReducedSpace(0f, 0f, 0f);
                    break;
            }
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
        
    }
}