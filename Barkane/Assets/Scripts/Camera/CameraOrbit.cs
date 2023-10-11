using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraOrbit : Singleton<CameraOrbit>
{
    [SerializeField] private float mouseSensitivity = 4.0f;
    [SerializeField] private float ScrollSenstivity = 0.2f;
    [SerializeField] private float orbitDampen = 10.0f;
    [SerializeField] private float scrollDampen = 6.0f;
    [SerializeField] private float minCameraDistance = 1.5f;
    [SerializeField] private float maxCameraDistance = 10.0f;

    [SerializeField] private bool cameraDisabled = true;
    public bool CameraDisabled {get => cameraDisabled;}
    private bool clicking = false;

    private Transform cameraTransform;
    private Transform cameraParent;
    private Vector3 localRoatation;
    private float cameraDistance = 5.0f;
    private Vector2 prevMousePosition;


    private PlayerActionHints hints;

    bool moved = false;
    bool scrolled = false;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        cameraTransform = this.transform;
        cameraParent = this.transform.parent;
        localRoatation.x = cameraParent.localRotation.eulerAngles.y * -1;
        localRoatation.y = cameraParent.localRotation.eulerAngles.x;
        UpdateSensitivity();
        hints = FindObjectOfType<PlayerActionHints>();
    }

    public void UpdateSensitivity()
    {
        mouseSensitivity = 1f * 0.01f * (PlayerPrefs.HasKey("pan") ? PlayerPrefs.GetInt("pan") : 50);
        ScrollSenstivity = 0.4f * 0.01f * (PlayerPrefs.HasKey("zoom") ? PlayerPrefs.GetInt("zoom") : 50);
    }

    //CO: We want the camera to move after everything else
    private void LateUpdate() 
    {
        if(!cameraDisabled)
        {
            if(clicking && !PauseManager.IsPaused)
            {
                Vector2 diff = prevMousePosition - Mouse.current.position.ReadValue();
                localRoatation.x += diff.x * mouseSensitivity; 
                localRoatation.y += diff.y * mouseSensitivity;
                localRoatation.y = Mathf.Clamp(localRoatation.y, -80f, 80f);
                moved = true;
            }
            float scrollAmount = Mouse.current.scroll.ReadValue().y * ScrollSenstivity * 0.01f * cameraDistance;
            if(scrollAmount > 0)
                scrolled = true;
            cameraDistance -= scrollAmount;
            cameraDistance = Mathf.Clamp(cameraDistance, minCameraDistance, maxCameraDistance);
        }

        Quaternion quaternion = Quaternion.Euler(localRoatation.y, localRoatation.x * -1, 0);
        cameraParent.rotation = Quaternion.Lerp(cameraParent.rotation, quaternion, Time.deltaTime * orbitDampen);
        if(cameraTransform.localPosition.z != cameraDistance * -1)
            cameraTransform.localPosition = new Vector3(0, 0, Mathf.Lerp(cameraTransform.localPosition.z, cameraDistance * -1, Time.deltaTime * scrollDampen));
        
        prevMousePosition = Mouse.current.position.ReadValue();

         hints = FindObjectOfType<PlayerActionHints>(); 
        if(moved && scrolled && hints != null)
            hints.DisableHint("camera");
    }

    private void OnClick(InputValue value)
    {
        if(PauseManager.IsPaused) return;
        clicking = value.isPressed;
    }

    private void OnMiddleClick(InputValue value)
    {
       // clicking = value.isPressed;
        //ToggleCamera(value.isPressed);
    }

    private void OnToggleCamera(InputValue value)
    {
        ToggleCamera(cameraDisabled);
    }

    private void ToggleCamera(bool value)
    {
        cameraDisabled = !value;
        if(!cameraDisabled)
            prevMousePosition = Mouse.current.position.ReadValue();
    }

}
