using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuAnimRemoveAlongX : MonoBehaviour
{
    private Transform referencePoint;
    public float pointOfDestruction = 571f;



    void Start()
    {
        referencePoint = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (Mathf.Abs(referencePoint.position.x - transform.position.x) > pointOfDestruction)
        {
            Destroy(gameObject);
        }
    }
}