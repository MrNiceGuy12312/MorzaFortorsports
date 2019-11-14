using UnityEngine;

public class SkidSoundEmitter : MonoBehaviour
{
    public float skidAt = 1.5f;
    public int soundEmissionPerSecond = 10;
    public AudioClip skidSound;

    private float soundDelay = 0f;
    private WheelCollider attachedWheel;

    // Start is called before the first frame update
    void Start()
    {
        attachedWheel = GetComponent<WheelCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        WheelHit hit;

        if (attachedWheel.GetGroundHit(out hit))
        {
            float sidewaysFrictionValue = Mathf.Abs(hit.sidewaysSlip);
            if (skidAt <= sidewaysFrictionValue && soundDelay <= 0)
            {
                AudioSource.PlayClipAtPoint(skidSound, hit.point);
                soundDelay = 1f;
            }
            Debug.Log(sidewaysFrictionValue);
        }
        soundDelay -= Time.deltaTime * soundEmissionPerSecond;
    }
}
