using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseCamera : MonoBehaviour
{
    public Transform targetToChase;
    public float distance;
    public float height;
    public float rotationDamping = 3f;
    public float heightDamping = 2f;
    public float defaultFOV = 60f;
    public float zoomRatio = 3f;

    private float desiredAngle = 0f;

    void LateUpdate()
    {
        float currentAngle = transform.eulerAngles.y;
        float currentHeight = transform.position.y;

        float desiredHeight = targetToChase.position.y + height;

        currentAngle = Mathf.LerpAngle(currentAngle, desiredAngle, rotationDamping * Time.deltaTime);
        currentHeight = Mathf.Lerp(currentHeight, desiredHeight, heightDamping * Time.deltaTime);
        Quaternion currentRotation = Quaternion.Euler(0, currentAngle, 0);

        Vector3 finalPosition = targetToChase.position - (currentRotation * Vector3.forward * distance);
        finalPosition.y = currentHeight;

        transform.position = finalPosition;
        transform.LookAt(targetToChase);
    }

    private void FixedUpdate()
    {
        desiredAngle = targetToChase.eulerAngles.y;

        Rigidbody rb = targetToChase.GetComponent<Rigidbody>();
        if (rb)
        {
            float speed = rb.velocity.magnitude;
            GetComponent<Camera>().fieldOfView = defaultFOV + (speed * zoomRatio) > 90f ? 90f : defaultFOV + (speed * zoomRatio);

            Vector3 localVelocity = targetToChase.InverseTransformDirection(rb.velocity);
            if (localVelocity.z < -0.5f)
            {
                desiredAngle += 180f;
            }
        }
    }
}
