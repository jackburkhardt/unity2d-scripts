using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class FollowCamera : MonoBehaviour
{
    // Target of followcam (player, in most cases)
    public Transform target;
    // Follow offset for the camera
    public Vector3 offset;
    // Box for when to start movement (NOT USED -- SMOOTHING WORKS FINE)
    public Canvas deadbox;
    // Factor for camera movement smoothing
    [Range(1, 10)] public float smoothFactor;

    private void FixedUpdate()
    {
        Follow();
    }

    private void Follow()
    {
        Vector3 targetPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothFactor * Time.fixedDeltaTime);
        transform.position = smoothedPosition;
    }
}
