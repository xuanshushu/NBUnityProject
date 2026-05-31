using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public sealed class NBShaderSampleFirstPersonController : MonoBehaviour
{
    [SerializeField] private float m_MoveSpeed = 5f;
    [SerializeField] private float m_FastMoveMultiplier = 2.5f;
    [SerializeField] private float m_MouseLookSensitivity = 1.5f;
    [SerializeField] private float m_TouchLookSensitivity = 120f;
    [SerializeField] private float m_MinPitch = -80f;
    [SerializeField] private float m_MaxPitch = 80f;
    [SerializeField] private bool m_LockCursorOnDesktop = true;
    [SerializeField] private bool m_ShowMobileControlsInEditor;
    [SerializeField] private GameObject m_MobileControlsRoot;

    private float m_Yaw;
    private float m_Pitch;
    private InputAction m_MoveAction;
    private InputAction m_MouseLookAction;
    private InputAction m_StickLookAction;
    private InputAction m_AscendAction;
    private InputAction m_DescendAction;
    private InputAction m_FastMoveAction;

    private bool useMobileControls
    {
        get { return Application.isMobilePlatform || (Application.isEditor && m_ShowMobileControlsInEditor); }
    }

    public void ConfigureMobileControls(GameObject mobileControlsRoot)
    {
        m_MobileControlsRoot = mobileControlsRoot;
        ApplyMobileControlVisibility();
    }

    private void Awake()
    {
        ReadCurrentRotation();
        ApplyMobileControlVisibility();
    }

    private void OnEnable()
    {
        EnsureActions();
        SetActionsEnabled(true);
        ReadCurrentRotation();
        ApplyMobileControlVisibility();
        if (!useMobileControls && m_LockCursorOnDesktop)
            LockCursor();
    }

    private void OnDisable()
    {
        SetActionsEnabled(false);
        if (!useMobileControls)
            UnlockCursor();
    }

    private void OnDestroy()
    {
        DisposeActions();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (useMobileControls)
            return;

        if (!hasFocus)
        {
            UnlockCursor();
            return;
        }

        if (m_LockCursorOnDesktop)
            LockCursor();
    }

    private void Update()
    {
        ApplyMobileControlVisibility();
        UpdateCursorLock();
        UpdateLook();
        UpdateMove();
    }

    private void ReadCurrentRotation()
    {
        var euler = transform.eulerAngles;
        m_Yaw = euler.y;
        m_Pitch = NormalizeAngle(euler.x);
    }

    private void UpdateLook()
    {
        if (useMobileControls)
        {
            var look = m_StickLookAction.ReadValue<Vector2>();
            m_Yaw += look.x * m_TouchLookSensitivity * Time.unscaledDeltaTime;
            m_Pitch -= look.y * m_TouchLookSensitivity * Time.unscaledDeltaTime;
        }
        else
        {
            var canLook =
                Cursor.lockState == CursorLockMode.Locked ||
                (Mouse.current != null && Mouse.current.rightButton.isPressed);
            if (!canLook)
                return;

            var look = m_MouseLookAction.ReadValue<Vector2>();
            m_Yaw += look.x * m_MouseLookSensitivity;
            m_Pitch -= look.y * m_MouseLookSensitivity;
        }

        m_Pitch = Mathf.Clamp(m_Pitch, m_MinPitch, m_MaxPitch);
        transform.rotation = Quaternion.Euler(m_Pitch, m_Yaw, 0f);
    }

    private void UpdateMove()
    {
        var moveInput = m_MoveAction.ReadValue<Vector2>();
        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();

        var forward = transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude > 0.0001f)
            forward.Normalize();

        var right = transform.right;
        right.y = 0f;
        if (right.sqrMagnitude > 0.0001f)
            right.Normalize();

        var velocity = right * moveInput.x + forward * moveInput.y;
        if (!useMobileControls)
            velocity += Vector3.up * ReadKeyboardVerticalMove();

        if (velocity.sqrMagnitude > 1f)
            velocity.Normalize();

        var speed = m_MoveSpeed;
        if (!useMobileControls && m_FastMoveAction.IsPressed())
            speed *= m_FastMoveMultiplier;

        transform.position += velocity * speed * Time.deltaTime;
    }

    private float ReadKeyboardVerticalMove()
    {
        var value = 0f;
        if (m_DescendAction.IsPressed())
            value -= 1f;
        if (m_AscendAction.IsPressed())
            value += 1f;
        return value;
    }

    private void UpdateCursorLock()
    {
        if (useMobileControls || !m_LockCursorOnDesktop)
            return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            UnlockCursor();
            return;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && !IsPointerOverUi())
            LockCursor();
    }

    private void EnsureActions()
    {
        if (m_MoveAction != null)
            return;

        m_MoveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
        m_MoveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        m_MoveAction.AddBinding("<Gamepad>/leftStick");

        m_MouseLookAction = new InputAction("Mouse Look", InputActionType.Value, "<Mouse>/delta", expectedControlType: "Vector2");
        m_StickLookAction = new InputAction("Stick Look", InputActionType.Value, "<Gamepad>/rightStick", expectedControlType: "Vector2");

        m_AscendAction = new InputAction("Ascend", InputActionType.Button);
        m_AscendAction.AddBinding("<Keyboard>/space");
        m_AscendAction.AddBinding("<Keyboard>/e");

        m_DescendAction = new InputAction("Descend", InputActionType.Button);
        m_DescendAction.AddBinding("<Keyboard>/leftCtrl");
        m_DescendAction.AddBinding("<Keyboard>/q");

        m_FastMoveAction = new InputAction("Fast Move", InputActionType.Button, "<Keyboard>/leftShift");
    }

    private void SetActionsEnabled(bool enabled)
    {
        if (m_MoveAction == null)
            return;

        if (enabled)
        {
            m_MoveAction.Enable();
            m_MouseLookAction.Enable();
            m_StickLookAction.Enable();
            m_AscendAction.Enable();
            m_DescendAction.Enable();
            m_FastMoveAction.Enable();
            return;
        }

        m_MoveAction.Disable();
        m_MouseLookAction.Disable();
        m_StickLookAction.Disable();
        m_AscendAction.Disable();
        m_DescendAction.Disable();
        m_FastMoveAction.Disable();
    }

    private void DisposeActions()
    {
        if (m_MoveAction == null)
            return;

        m_MoveAction.Dispose();
        m_MouseLookAction.Dispose();
        m_StickLookAction.Dispose();
        m_AscendAction.Dispose();
        m_DescendAction.Dispose();
        m_FastMoveAction.Dispose();
    }

    private void ApplyMobileControlVisibility()
    {
        if (m_MobileControlsRoot != null && m_MobileControlsRoot.activeSelf != useMobileControls)
            m_MobileControlsRoot.SetActive(useMobileControls);
    }

    private static bool IsPointerOverUi()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private static void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private static float NormalizeAngle(float angle)
    {
        while (angle > 180f)
            angle -= 360f;
        while (angle < -180f)
            angle += 360f;
        return angle;
    }
}
