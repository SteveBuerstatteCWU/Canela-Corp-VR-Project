using UnityEngine;

public class CardboardBodyFollow : MonoBehaviour
{
    [SerializeField] private Transform cameraRig; // empty child of the player, parent of the camera
    [SerializeField] private Transform head;      // Main Camera
    [SerializeField] private float turnSpeed = 8f; // higher = body keeps up faster

    void LateUpdate()
    {
        // Remember how the rig is facing in the world before the body turns
        Quaternion rigWorldRotation = cameraRig.rotation;

        // Turn the body toward where the head is facing (left/right only)
        Vector3 lookDir = Vector3.ProjectOnPlane(head.forward, Vector3.up);
        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion target = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, turnSpeed * Time.deltaTime);
        }

        // Undo the body's turn on the rig so the view isn't affected
        cameraRig.rotation = rigWorldRotation;
    }
}