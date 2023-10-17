using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rescaler : MonoBehaviour
{
    public Camera targetCam;
    public Vector3 offset = new Vector3(0, 0, -10);

    [ContextMenu("Rescale To Camera")]
    public void RescaleToCamera()
    {
        if (targetCam == null) targetCam = Camera.main;

        transform.position = targetCam.transform.position - offset;

        Vector3 bottomLeft = targetCam.ViewportToWorldPoint(new Vector3(0, 0));
        Vector3 topRight = targetCam.ViewportToWorldPoint(new Vector3(1, 1));

        Vector3 targetScale = topRight - bottomLeft + new Vector3(0, 0, 1);
        transform.localScale = targetScale;
    }

    void Awake()
    {
        RescaleToCamera();
    }
}
