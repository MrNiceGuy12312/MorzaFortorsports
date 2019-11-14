using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public float topSpeed = 150f;
    public float maxReverseSpeed = 75f;
    public Vector3 centerOfMassAdjustment = new Vector3(0, -0.5f, 0);
    public float maxTurnAngle = 10f;
    public float maxTorque = 10f;
    public float decelerationTorque = 30f;
    public float brakeTorque = 10f;
    public float spoilerRatio = 0.1f;
    public float handBrakeForwardSlip = 0.04f;
    public float handBrakeSidewaySlip = 0.08f;

    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;
    public Transform wheelTransformFL;
    public Transform wheelTransformFR;
    public Transform wheelTransformRL;
    public Transform wheelTransformRR;

    protected bool applyHandBrake = false;

    private void Start()
    {
        GetComponent<Rigidbody>().centerOfMass += centerOfMassAdjustment;
    }

    private void Update()
    {
        UpdateWheelTransforms();
    }

    private void UpdateWheelTransforms()
    {
        float rotationThisFrame = 360.0f * Time.deltaTime;
        wheelTransformFL.Rotate(0, 0, wheelFL.rpm / rotationThisFrame);
        wheelTransformFR.Rotate(0, 0, -wheelFR.rpm / rotationThisFrame);
        wheelTransformRL.Rotate(0, 0, wheelRL.rpm / rotationThisFrame);
        wheelTransformRR.Rotate(0, 0, -wheelRR.rpm / rotationThisFrame);

        Vector3 localRotation = wheelTransformFL.localEulerAngles;
        wheelTransformFL.localEulerAngles = new Vector3(localRotation.x, wheelFL.steerAngle + 90.0f, localRotation.z);
        localRotation = wheelTransformFR.localEulerAngles;
        wheelTransformFR.localEulerAngles = new Vector3(localRotation.x, wheelFR.steerAngle - 90.0f, localRotation.z);

        WheelHit contact = new WheelHit();

        if (wheelFL.GetGroundHit(out contact))
        {
            Vector3 wheelPosition = wheelFL.transform.position;
            wheelPosition.y = (contact.point + wheelFL.transform.up * wheelFL.radius).y;
            wheelTransformFL.position = wheelPosition;
        }

        if (wheelFR.GetGroundHit(out contact))
        {
            Vector3 wheelPosition = wheelFR.transform.position;
            wheelPosition.y = (contact.point + wheelFR.transform.up * wheelFR.radius).y;
            wheelTransformFR.position = wheelPosition;
        }

        if (wheelRL.GetGroundHit(out contact))
        {
            Vector3 wheelPosition = wheelRL.transform.position;
            wheelPosition.y = (contact.point + wheelRL.transform.up * wheelRL.radius).y;
            wheelTransformRL.position = wheelPosition;
        }

        if (wheelRR.GetGroundHit(out contact))
        {
            Vector3 wheelPosition = wheelRR.transform.position;
            wheelPosition.y = (contact.point + wheelRR.transform.up * wheelRR.radius).y;
            wheelTransformRR.position = wheelPosition;
        }
    }

    private void FixedUpdate()
    {
        float currentSpeed = GetComponent<Rigidbody>().velocity.magnitude * 3.6f;

        if (currentSpeed < topSpeed && currentSpeed > -maxReverseSpeed)
        {
            wheelRL.motorTorque = Input.GetAxis("Vertical") * maxTorque;
            wheelRR.motorTorque = Input.GetAxis("Vertical") * maxTorque;
        }
        else
        {
            wheelRL.motorTorque = 0;
            wheelRR.motorTorque = 0;
        }

        Vector3 localVelocity = transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity);
        GetComponent<Rigidbody>().AddForce(-transform.up * (localVelocity.z * spoilerRatio), ForceMode.Force);

        // apply the brakes if accelerating in the opposite direction of velocity.
        if ((Input.GetAxis("Vertical") <= -0.1f && localVelocity.z > 0) ||
            (Input.GetAxis("Vertical") >= 0.1f && localVelocity.z < 0))
        {
            wheelRL.brakeTorque = decelerationTorque + maxTorque;
            wheelRR.brakeTorque = decelerationTorque + maxTorque;
        }
        else
        {
            wheelRL.brakeTorque = 0;
            wheelRR.brakeTorque = 0;
        }

        if (Input.GetButton("Jump"))
        {
            applyHandBrake = true;
            wheelFL.brakeTorque = brakeTorque;
            wheelFR.brakeTorque = brakeTorque;
            wheelRL.brakeTorque = brakeTorque;
            wheelRR.brakeTorque = brakeTorque;

            if (GetComponent<Rigidbody>().velocity.magnitude > 0)
            {
                SetSlipValues(handBrakeForwardSlip, handBrakeSidewaySlip);
            }
            else
            {
                SetSlipValues(1f, 1f);
            }
        }
        else
        {
            applyHandBrake = false;
            wheelFL.brakeTorque = 0;
            wheelFR.brakeTorque = 0;
            wheelRL.brakeTorque = 0;
            wheelRR.brakeTorque = 0;
            SetSlipValues(1f, 1f);
        }

        wheelFL.steerAngle = Input.GetAxis("Horizontal") * maxTurnAngle;
        wheelFR.steerAngle = Input.GetAxis("Horizontal") * maxTurnAngle;
    }

    private void SetSlipValues(float forwardSlip, float sidewaySlip)
    {
        WheelFrictionCurve tempCurve = wheelFL.forwardFriction;
        tempCurve.stiffness = forwardSlip;
        wheelFL.forwardFriction = tempCurve;

        tempCurve = wheelFL.sidewaysFriction;
        tempCurve.stiffness = forwardSlip;
        wheelFL.sidewaysFriction = tempCurve;

        tempCurve = wheelFR.forwardFriction;
        tempCurve.stiffness = forwardSlip;
        wheelFR.forwardFriction = tempCurve;

        tempCurve = wheelFR.sidewaysFriction;
        tempCurve.stiffness = forwardSlip;
        wheelFR.sidewaysFriction = tempCurve;

        tempCurve = wheelRL.forwardFriction;
        tempCurve.stiffness = forwardSlip;
        wheelRL.forwardFriction = tempCurve;

        tempCurve = wheelRL.sidewaysFriction;
        tempCurve.stiffness = forwardSlip;
        wheelRL.sidewaysFriction = tempCurve;

        tempCurve = wheelRR.forwardFriction;
        tempCurve.stiffness = forwardSlip;
        wheelRR.forwardFriction = tempCurve;

        tempCurve = wheelRR.sidewaysFriction;
        tempCurve.stiffness = forwardSlip;
        wheelRR.sidewaysFriction = tempCurve;
    }
}
