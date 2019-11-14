using UnityEngine;

public class AntiRollBar : MonoBehaviour
{
    public float antiRoll = 5000.0f;
    public WheelCollider wheelL;
    public WheelCollider wheelR;

    // Update is called once per frame
    void FixedUpdate()
    {
        WheelHit hit = new WheelHit();
        float travelL = 1.0f;
        float travelR = 1.0f;

        bool groundedL = wheelL.GetGroundHit(out hit);
        
        if (groundedL)
        {
            travelL = (-wheelL.transform.InverseTransformPoint(hit.point).y - wheelL.radius) / wheelL.suspensionDistance;
        }

        bool groundedR = wheelR.GetGroundHit(out hit);

        if (groundedR)
        {
            travelR = (-wheelR.transform.InverseTransformPoint(hit.point).y - wheelR.radius) / wheelR.suspensionDistance;
        }

        float antiRollForce = (travelL - travelR) * antiRoll;
        if (groundedL)
        {
            GetComponent<Rigidbody>().AddForceAtPosition(wheelL.transform.up * -antiRollForce, wheelL.transform.position);
        }
        if (groundedR)
        {
            GetComponent<Rigidbody>().AddForceAtPosition(wheelR.transform.up * -antiRollForce, wheelR.transform.position);
        }
    }
}
