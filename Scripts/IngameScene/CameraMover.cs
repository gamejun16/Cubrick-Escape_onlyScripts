using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    [Header("offset")]
    public Vector3 posOffset, rotOffset;

    [Header("Components")]
    public Camera cam;

    [Space]
    public float turnSpeed;

    CamFixmode mode;
    int fov;

    private void Start()
    {
        DataContainer.instance.GetSettingInfo(out mode);
        DataContainer.instance.GetSettingInfo(out fov);

        cam.fieldOfView = fov;

        transform.eulerAngles = rotOffset;
    }

    private void FixedUpdate()
    {
        switch (mode)
        {
            case CamFixmode.stageCenterFix:
                
                if (PlayerMover.instance == null || !PlayerMover.instance.isMovePossible) return;

                transform.position = Vector3.Lerp(
                    transform.position,
                    new Vector3(posOffset.x,
                        posOffset.y + PlayerMover.instance.transform.position.y,
                        posOffset.z
                        ),
                    0.2f
                    );

                break;

            case CamFixmode.playerCenterFix:

                if (PlayerMover.instance == null || !PlayerMover.instance.isMovePossible) return;

                transform.position = Vector3.Lerp(
                    transform.position,
                    new Vector3(posOffset.x + PlayerMover.instance.transform.position.x * 0.5f,
                        posOffset.y + PlayerMover.instance.transform.position.y,
                        posOffset.z + PlayerMover.instance.transform.position.z * 0.5f
                        ),
                    0.2f
                    );

                break;
        }


        
    }
}
