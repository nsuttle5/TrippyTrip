using UnityEngine;
using System.Collections.Generic;

public class Hand : MonoBehaviour
{
    public Transform handTarget;
    public Transform wheel;
    public Vector3 offset;
    public List<Transform> joints = new List<Transform>();
    public float positionLerpSpeed = 20f;
    public float rotationSpeed = 360f;

    bool initialized;
    public static Transform interacting;

    void Update()
    {
        if (handTarget == null || wheel == null)
        {
            return;
        }

        Vector3 targetPosition = wheel.TransformPoint(offset);
        int jointCount = joints.Count;
        if (jointCount == 0 || joints[0] == null)
        {
            return;
        }

        Transform firstJoint = joints[0];
        float firstToTarget = Vector3.Distance(firstJoint.position, targetPosition);
        float handToTarget = Vector3.Distance(handTarget.position, targetPosition);
        float segmentCount = Mathf.Max(1, jointCount - 1);
        float segmentDistance = (firstToTarget - handToTarget) / segmentCount;
        segmentDistance = Mathf.Max(0f, segmentDistance);

        Vector3 chainDirection = targetPosition - firstJoint.position;
        if (chainDirection.sqrMagnitude <= 0.0001f)
        {
            return;
        }
        chainDirection.Normalize();

        for (int i = 0; i < jointCount; i++)
        {
            Transform currentJoint = joints[i];
            if (currentJoint == null)
            {
                continue;
            }

            Vector3 desiredPosition = firstJoint.position + chainDirection * (segmentDistance * i);
            if (!initialized)
            {
                currentJoint.position = desiredPosition;
            }
            else
            {
                currentJoint.position = Vector3.Lerp(
                    currentJoint.position,
                    desiredPosition,
                    positionLerpSpeed * Time.deltaTime);
            }

            Vector3 direction = targetPosition - currentJoint.position;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                continue;
            }

            Vector3 upDirection = direction.normalized;
            Vector3 forwardDirection = Vector3.ProjectOnPlane(wheel.forward, upDirection);
            if (forwardDirection.sqrMagnitude <= 0.0001f)
            {
                forwardDirection = Vector3.ProjectOnPlane(wheel.right, upDirection);
            }

            Quaternion targetRotation = Quaternion.LookRotation(forwardDirection.normalized, upDirection);
            if (!initialized)
            {
                currentJoint.rotation = targetRotation;
            }
            else
            {
                currentJoint.rotation = Quaternion.RotateTowards(
                    currentJoint.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime);
            }
        }

        initialized = true;
    }
}
