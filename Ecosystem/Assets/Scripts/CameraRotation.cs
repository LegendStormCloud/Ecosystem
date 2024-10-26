using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    const float rotationSpeed = 24f;
    public bool canRotate = false;

    void Update()
    {
        if(!canRotate)
        {
            return;
        }

        transform.RotateAround(Vector3.zero, Vector3.up, Time.deltaTime * EcosystemManager.instance.timeScale * rotationSpeed);
    }
}