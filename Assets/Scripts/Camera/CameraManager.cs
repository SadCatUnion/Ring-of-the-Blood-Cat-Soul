using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : Singleton<CameraManager>
{
    [Header("Component References")]
    public Camera mainCamera;

    [Header("Virtual Cameras")]
    public CinemachineFreeLook VCamPlayer;

    public Camera GetMainCamera()
    {
        return mainCamera;
    }
    public Cinemachine.CinemachineFreeLook GetVCamera()
    {
        return VCamPlayer;
    }
}
