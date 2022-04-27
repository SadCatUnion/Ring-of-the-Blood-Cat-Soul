using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : Singleton<CameraManager>
{
    public enum ECameraMode
    {
        LOCK_FREE,
        LOCK_ON,
    }
    [Header("Component References")]
    public Camera mainCamera;
    public ECameraMode cameraMode = ECameraMode.LOCK_FREE;

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
    public ECameraMode GetCameraMode()
    {
        return cameraMode;
    }
}
