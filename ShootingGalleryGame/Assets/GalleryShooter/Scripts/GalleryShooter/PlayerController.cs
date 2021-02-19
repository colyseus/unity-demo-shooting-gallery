using System.Collections;
using System.Collections.Generic;
using LucidSightTools;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : ExampleNetworkedEntityView
{
    private CharacterController _characterController;

    private Quaternion cachedHeadRotation;
    private Vector2 currentLookRotation = Vector2.zero;
    private float gravityValue = -9.81f;
    private bool groundedPlayer;

    [SerializeField]
    private Gun gun = null;

    [SerializeField]
    private Transform headRoot = null;

    private bool isPaused;

    [SerializeField]
    private float jumpHeight = 1.0f;

    [SerializeField]
    private ADSCamera lookCamera = null;

    [SerializeField]
    private float lookSpeed = 3.0f;

    [SerializeField]
    private float playerSpeed = 2.0f;

    [SerializeField]
    private Crosshair crosshair;

    private Vector3 playerVelocity;

    public string userName;

    public bool isReady = false;

    [SerializeField]
    private TextMeshPro userNameDisplay = null;

    [SerializeField]
    private PlayerStatusIndicator statusIndicator = null;

    protected override void Start()
    {
        autoInitEntity = false;
        base.Start();
        userName = string.Empty;
        _characterController = GetComponent<CharacterController>();
        isPaused = false;
        SetCursorUnlocked(false);
        lookCamera.gameObject.SetActive(false);
        StartCoroutine("WaitForConnect");
    }

    private IEnumerator WaitForConnect()
    {
        if (ExampleManager.Instance.CurrentUser != null && !IsMine)
        {
            yield break;
        }

        lookCamera.gameObject.SetActive(true);
        while (!ExampleManager.Instance.IsInRoom)
        {
            yield return 0;
        }

        LSLog.LogImportant("HAS JOINED ROOM - CREATING ENTITY");
        ExampleManager.CreateNetworkedEntityWithTransform(transform.position, Quaternion.identity,
            new Dictionary<string, object> {["userName"] = ExampleManager.Instance.UserName}, this, entity =>
            {
                userName = ExampleManager.Instance.UserName;
                userNameDisplay.text = userName;
                ExampleManager.Instance.CurrentNetworkedEntity = entity;
            });
    }

    public override void OnEntityRemoved()
    {
        base.OnEntityRemoved();
        LSLog.LogImportant("REMOVING ENTITY", LSLog.LogColor.lime);
        Destroy(gameObject);
    }

    protected override void ProcessViewSync()
    {
        // This is the target playback time of this body
        double interpolationTime = ExampleManager.Instance.GetServerTime - interpolationBackTimeMs;

        // Use interpolation if the target playback time is present in the buffer
        if (proxyStates[0].timestamp > interpolationTime)
        {
            // The longer the time since last update add a delta factor to the lerp speed to get there quicker
            float deltaFactor = ExampleManager.Instance.GetServerTimeSeconds > proxyStates[0].timestamp
                ? (float) (ExampleManager.Instance.GetServerTimeSeconds - proxyStates[0].timestamp) * 0.2f
                : 0f;

            if (syncLocalPosition)
            {
                myTransform.localPosition = Vector3.Slerp(myTransform.localPosition, proxyStates[0].pos,
                    Time.deltaTime * (positionLerpSpeed + deltaFactor));
            }

            if (syncLocalRotation && Mathf.Abs(Quaternion.Angle(transform.localRotation, proxyStates[0].rot)) >
                snapIfAngleIsGreater)
            {
                myTransform.localRotation = proxyStates[0].rot;
            }

            if (syncLocalRotation)
            {
                myTransform.localRotation = Quaternion.Slerp(myTransform.localRotation, proxyStates[0].rot,
                    Time.deltaTime * (rotationLerpSpeed + deltaFactor));
                headRoot.localRotation = Quaternion.Slerp(headRoot.localRotation, cachedHeadRotation,
                    Time.deltaTime * (rotationLerpSpeed + deltaFactor));
            }
        }
        // Use extrapolation (If we didnt get a packet in the last X ms and object had velocity)
        else
        {
            EntityState latest = proxyStates[0];

            float extrapolationLength = (float) (interpolationTime - latest.timestamp);
            // Don't extrapolation for more than 500 ms, you would need to do that carefully
            if (extrapolationLength < extrapolationLimitMs / 1000f)
            {
                if (syncLocalPosition)
                {
                    myTransform.localPosition = latest.pos + latest.vel * extrapolationLength;
                }

                if (syncLocalRotation)
                {
                    myTransform.localRotation = latest.rot;
                }
            }
        }
    }

    protected override void UpdateViewFromState()
    {
        base.UpdateViewFromState();
        //catch the incoming head rotation. If it has xView, it will have the rest
        if (state.attributes.ContainsKey("xViewRot"))
        {
            cachedHeadRotation.x = float.Parse(state.attributes["xViewRot"]);
            cachedHeadRotation.y = float.Parse(state.attributes["yViewRot"]);
            cachedHeadRotation.z = float.Parse(state.attributes["zViewRot"]);
            cachedHeadRotation.w = float.Parse(state.attributes["wViewRot"]);
        }

        if (string.IsNullOrEmpty(userName) && state.attributes.ContainsKey("userName"))
        {
            userName = state.attributes["userName"];
            userNameDisplay.text = userName;
        }

        if (state.attributes.ContainsKey("isReady"))
        {
            isReady = bool.Parse(state.attributes["isReady"]);
            statusIndicator.UpdateReady(isReady);
        }
    }

    protected override void UpdateStateFromView()
    {
        base.UpdateStateFromView();

        //Update the head rotation attributes
        Dictionary<string, string> updatedAttributes = new Dictionary<string, string>
        {
            {"xViewRot", headRoot.localRotation.x.ToString()},
            {"yViewRot", headRoot.localRotation.y.ToString()},
            {"zViewRot", headRoot.localRotation.z.ToString()},
            {"wViewRot", headRoot.localRotation.w.ToString()}
        };
        SetAttributes(updatedAttributes);
    }

    protected override void Update()
    {
        base.Update();

        if (!HasInit || !IsMine)
        {
            return;
        }

        HandleInput();
        HandleLook();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetPause(!isPaused);
        }

        if (isPaused)
        {
            return;
        }

        groundedPlayer = _characterController.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        move = transform.TransformDirection(move);
        _characterController.Move(move * Time.deltaTime * playerSpeed);

        if (Input.GetButtonDown("Jump") && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        _characterController.Move(playerVelocity * Time.deltaTime);

        if (Input.GetMouseButton(0))
        {
            ExampleManager.RFC(state.id, "FireGunRFC", null);
        }

        lookCamera.isADS = Input.GetMouseButton(1);

        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
    }

    public void FireGunRFC()
    {
        gun.Fire(state.id);
    }

    public void Reload()
    {
        gun.Reload();
    }

    private void HandleLook()
    {
        if (isPaused)
        {
            return;
        }

        currentLookRotation.y += Input.GetAxis("Mouse X");
        currentLookRotation.x += -Input.GetAxis("Mouse Y");

        Vector3 bodyRot = transform.eulerAngles;
        Vector3 look = currentLookRotation * lookSpeed;
        //Player rotation
        bodyRot.y = look.y;
        transform.eulerAngles = bodyRot;
        //Tilt
        Vector3 head = headRoot.eulerAngles;
        head.x = Mathf.Clamp(look.x, -80, 80); //Clamp to prevent gimbal lock
        headRoot.eulerAngles = head;
    }

    private void SetCursorUnlocked(bool val)
    {
        Cursor.visible = val;
        Cursor.lockState = val ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void SetPause(bool pause)
    {
        isPaused = pause;
        SetCursorUnlocked(isPaused);
    }

    public void ShowTargetHit()
    {
        if (IsMine)
        {
            crosshair.ShowHit();
        }
        
        //If we want to show anything when a remote client hits a target, put that here
    }

    public void UpdateReadyState(bool ready)
    {
        if (IsMine)
        {
            isReady = ready;
            SetAttributes(new Dictionary<string, string>() { {"isReady", isReady.ToString()} });
            statusIndicator.UpdateReady(ready);
        }
    }
}