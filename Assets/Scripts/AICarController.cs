using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICarController : CarController
{
    public Transform waypointContainer;
    public float waypointProximity = 25f;
    public float brakingDistance = 10f;
    public float spacingDistance = 4f;
    public float cornerDistance = 12f;
    public float forwardOffset = 0f;
    public float sidewayOffset = 0f;

    private Transform[] waypoints;
    private int currentWaypoint = 0;
    private float inputSteer;
    private float inputTorque;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody>().centerOfMass += centerOfMassAdjustment;
        GetWaypoints();
    }

    void FixedUpdate()
    {
        if (!RaceManager.Started) return;

        Vector3 relativeWaypointPos = transform.InverseTransformPoint(waypoints[currentWaypoint].position.x,
                                                                      transform.position.y,
                                                                      waypoints[currentWaypoint].position.z);
        inputSteer = relativeWaypointPos.x / relativeWaypointPos.magnitude;
        inputSteer += CheckSpacing();
        inputSteer += CheckCollision();

        Vector3 localVelocity = transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity);
        GetComponent<Rigidbody>().AddForce(-transform.up * (localVelocity.z * spoilerRatio), ForceMode.Force);

        if (Mathf.Abs(inputSteer) < 0.5f)
        {
            inputTorque = relativeWaypointPos.z / relativeWaypointPos.magnitude;
            applyHandBrake = false;
        }
        else
        {
            if (localVelocity.z > 5f)
            {
                applyHandBrake = true;
            }
            else if (localVelocity.z < 0)
            {
                applyHandBrake = false;
                inputTorque = -1;
                inputSteer *= -1;
            }
            else
            {
                applyHandBrake = false;
                inputTorque = 0;
            }
        }

        inputSteer = Mathf.Clamp(inputSteer, -1.0f, 1.0f);
        wheelFL.steerAngle = inputSteer * maxTurnAngle;
        wheelFR.steerAngle = inputSteer * maxTurnAngle;

        float currentSpeed = GetComponent<Rigidbody>().velocity.magnitude * 3.6f;
        float adjustment = 1.0f;

        if (currentSpeed < topSpeed && currentSpeed > -maxReverseSpeed)
        {
            // check for forward collision
            adjustment = CheckBraking(); // change to brake scalar;

            if (adjustment == 0)
            {
                wheelRL.motorTorque = inputTorque * maxTorque;
                wheelRR.motorTorque = inputTorque * maxTorque;
                wheelRL.brakeTorque = 0;
                wheelRR.brakeTorque = 0;
            }
            else
            {
                wheelRL.motorTorque = 0;
                wheelRR.motorTorque = 0;
                wheelRL.brakeTorque = adjustment * brakeTorque;
                wheelRR.brakeTorque = adjustment * brakeTorque;
            }
        }
        else
        {
            wheelRL.motorTorque = 0;
            wheelRR.motorTorque = 0;
        }

        if (relativeWaypointPos.magnitude < waypointProximity)
        {
            currentWaypoint += 1;
            if (currentWaypoint == waypoints.Length)
            {
                currentWaypoint = 0;
                RaceManager.Instance.LapFinishedByAI(this);
            }
        }
    }

    private void GetWaypoints()
    {
        Transform[] potentialWaypoints = waypointContainer.GetComponentsInChildren<Transform>();
        waypoints = new Transform[potentialWaypoints.Length - 1];

        for (int i = 1; i < potentialWaypoints.Length; ++i)
        {
            waypoints[i - 1] = potentialWaypoints[i];
        }
    }

    private float CheckSpacing()
    {
        float steeringAdjustment = 0.0f;

        Vector3 rightSide = transform.up + transform.position + (transform.right * sidewayOffset);
        RaycastHit hit;

        Debug.DrawRay(rightSide, transform.right * spacingDistance);

        if (Physics.Raycast(rightSide, transform.right, out hit, spacingDistance))
        {
            steeringAdjustment = -1 + (rightSide - hit.point).magnitude / spacingDistance;
        }

        Vector3 leftSide = transform.up + transform.position + (-transform.right * sidewayOffset);

        Debug.DrawRay(leftSide, -transform.right * spacingDistance);

        if (Physics.Raycast(leftSide, -transform.right, out hit, spacingDistance))
        {
            steeringAdjustment += 1 + (leftSide - hit.point).magnitude / spacingDistance;
        }

        return steeringAdjustment;
    }

    private float CheckCollision()
    {
        float steeringAdjustment = 0.0f;
        RaycastHit hit;

        Vector3 frontRight = transform.position +
            transform.up +
            (transform.right * sidewayOffset) +
            (transform.forward * forwardOffset);

        Debug.DrawRay(frontRight, (transform.right * 0.5f + transform.forward) * cornerDistance);

        if (Physics.Raycast(frontRight, transform.right * 0.5f + transform.forward, out hit, cornerDistance))
        {
            steeringAdjustment = -1 + (frontRight - hit.point).magnitude / cornerDistance;
        }

        Vector3 frontLeft = transform.position +
            transform.up +
            (-transform.right * sidewayOffset) +
            (transform.forward * forwardOffset);

        Debug.DrawRay(frontLeft, (-transform.right * 0.5f + transform.forward) * cornerDistance);

        if (Physics.Raycast(frontLeft, -transform.right * 0.5f + transform.forward, out hit, cornerDistance))
        {
            steeringAdjustment += 1 + (frontLeft - hit.point).magnitude / cornerDistance;
        }

        return steeringAdjustment;
    }

    float CheckBraking()
    {
        float torqueAdjustment = 0f;

        Vector3 front = transform.up + transform.position + (transform.forward * forwardOffset);
        RaycastHit hit;

        Debug.DrawRay(front, transform.forward * brakingDistance);

        if (Physics.Raycast(front, transform.forward, out hit, brakingDistance))
        {
            torqueAdjustment = 1 + (front - hit.point).magnitude / brakingDistance * 2;
        }

        return torqueAdjustment;
    }

    public Transform GetCurrentWaypoint()
    {
        return waypoints[currentWaypoint];
    }

    public Transform GetLastWaypoint()
    {
        if (currentWaypoint - 1 < 0)
        {
            return waypoints[waypoints.Length - 1];
        }

        return waypoints[currentWaypoint - 1];
    }
}
