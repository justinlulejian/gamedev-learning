using UnityEngine;

namespace ScriptExtensionMethods
{
    public static class MovementExtensions
    {
        public static void RotateTowards(Transform from, Transform to, float speed, float angleCorrection)
        {
            // From http://answers.unity.com/comments/651932/view.html.
            Vector3 vectorToTarget = to.position - from.position;
            float angle = (Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg) - angleCorrection;
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            from.transform.rotation = Quaternion.Slerp(from.rotation, q, Time.deltaTime * speed);
        }
        
        public static void RotateTowardsQuaternion(Transform from, Quaternion quaternion, float speed)
        { 
            from.transform.rotation = Quaternion.Slerp(from.rotation, quaternion, Time.deltaTime * speed);
        }
    }
}