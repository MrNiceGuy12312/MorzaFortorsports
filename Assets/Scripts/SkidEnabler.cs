using UnityEngine;

public class SkidEnabler : MonoBehaviour
{
    public WheelCollider wheelCollider;
    public GameObject skidTrailRenderer;
    public float skidLife = 4f;
    private TrailRenderer skidMark;

    // Start is called before the first frame update
    void Start()
    {
        skidMark = skidTrailRenderer.GetComponent<TrailRenderer>();
        skidMark.time = skidLife;
    }

    void LateUpdate()
    {
        if (wheelCollider.forwardFriction.stiffness < 0.1f && wheelCollider.isGrounded)
        {
            if (!skidMark.emitting)
            {
                skidTrailRenderer.transform.parent = wheelCollider.transform;
                skidTrailRenderer.transform.localPosition = wheelCollider.center + ((wheelCollider.radius - 0.1f) * -wheelCollider.transform.up);
                skidMark.Clear();
                skidMark.emitting = true;
                skidMark.time = skidLife;
            }

            if (skidTrailRenderer.transform.parent == null)
            {
                skidMark.emitting = false;
                skidMark.time = 0f;
            }
        }
        else
        {
            skidTrailRenderer.transform.parent = null;
        }
    }
}
