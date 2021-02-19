using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ADSCamera : MonoBehaviour
{
    [SerializeField]
    private CameraSetting adsSetting = null;

    [SerializeField]
    private float fovTransitionSpeed = 5.0f;

    [SerializeField]
    private CameraSetting idleSetting = null;

    public bool isADS = false;

    [SerializeField]
    private float posTransitionSpeed = 5.0f;

    private Camera viewCamera;

    private void Awake()
    {
        viewCamera = GetComponent<Camera>();
    }

    private void FixedUpdate()
    {
        if (!viewCamera.enabled)
        {
            return;
        }

        transform.localPosition = Vector3.MoveTowards(transform.localPosition,
            isADS ? adsSetting.localPosition : idleSetting.localPosition, posTransitionSpeed * Time.deltaTime);
        viewCamera.fieldOfView = Mathf.Lerp(viewCamera.fieldOfView, isADS ? adsSetting.fov : idleSetting.fov,
            fovTransitionSpeed * Time.deltaTime);
    }

    [Serializable]
    private class CameraSetting
    {
        public float fov = 60.0f;
        public Vector3 localPosition = Vector3.zero;
    }
}