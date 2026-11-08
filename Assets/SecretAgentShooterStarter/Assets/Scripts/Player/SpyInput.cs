using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public static class SpyInput
{
    private const float InputSystemMouseScale = 0.08f;

    public static Vector2 Move
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                Vector2 move = Vector2.zero;

                if (Keyboard.current.aKey.isPressed) move.x -= 1f;
                if (Keyboard.current.dKey.isPressed) move.x += 1f;
                if (Keyboard.current.sKey.isPressed) move.y -= 1f;
                if (Keyboard.current.wKey.isPressed) move.y += 1f;

                return Vector2.ClampMagnitude(move, 1f);
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
#else
            return Vector2.zero;
#endif
        }
    }

    public static Vector2 Look
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                return Mouse.current.delta.ReadValue() * InputSystemMouseScale;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#else
            return Vector2.zero;
#endif
        }
    }

    public static bool SprintHeld
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                return Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
#else
            return false;
#endif
        }
    }

    public static bool CrouchHeld
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                return Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
#else
            return false;
#endif
        }
    }

    public static bool FireHeld
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                return Mouse.current.leftButton.isPressed;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButton(0);
#else
            return false;
#endif
        }
    }

    public static bool JumpPressed => KeyPressedThisFrame(SpyKey.Space);
    public static bool ToggleCameraPressed => KeyPressedThisFrame(SpyKey.V);
    public static bool ToggleCarryStylePressed => KeyPressedThisFrame(SpyKey.B);
    public static bool InteractPressed => KeyPressedThisFrame(SpyKey.E);
    public static bool DropPressed => KeyPressedThisFrame(SpyKey.G);
    public static bool ThrowPressed => KeyPressedThisFrame(SpyKey.R);
    public static bool ReloadPressed => KeyPressedThisFrame(SpyKey.R);
    public static bool CompanionPressed => KeyPressedThisFrame(SpyKey.C);
    public static bool MapPressed => KeyPressedThisFrame(SpyKey.M);
    public static bool PunchPressed => KeyPressedThisFrame(SpyKey.F);
    public static bool KnifePressed => KeyPressedThisFrame(SpyKey.Q);
    public static bool AmmoSwitchPressed => KeyPressedThisFrame(SpyKey.X);
    public static bool MenuUpPressed => KeyPressedThisFrame(SpyKey.Up);
    public static bool MenuDownPressed => KeyPressedThisFrame(SpyKey.Down);
    public static bool MenuLeftPressed => KeyPressedThisFrame(SpyKey.Left);
    public static bool MenuRightPressed => KeyPressedThisFrame(SpyKey.Right);
    public static bool ConfirmPressed => KeyPressedThisFrame(SpyKey.Enter) || InteractPressed;
    public static bool CancelPressed => KeyPressedThisFrame(SpyKey.Escape);

    public static int WeaponSlotPressed
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame) return 0;
                if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame) return 1;
                if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame) return 2;
                if (Keyboard.current.digit4Key.wasPressedThisFrame || Keyboard.current.numpad4Key.wasPressedThisFrame) return 3;
                if (Keyboard.current.digit5Key.wasPressedThisFrame || Keyboard.current.numpad5Key.wasPressedThisFrame) return 4;
                if (Keyboard.current.digit6Key.wasPressedThisFrame || Keyboard.current.numpad6Key.wasPressedThisFrame) return 5;
                if (Keyboard.current.digit7Key.wasPressedThisFrame || Keyboard.current.numpad7Key.wasPressedThisFrame) return 6;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) return 0;
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) return 1;
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) return 2;
            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) return 3;
            if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) return 4;
            if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6)) return 5;
            if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7)) return 6;
#endif

            return -1;
        }
    }

    public static int WeaponCycleDirection
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                float scroll = Mouse.current.scroll.ReadValue().y;
                if (scroll > 0.01f) return 1;
                if (scroll < -0.01f) return -1;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            float legacyScroll = Input.GetAxis("Mouse ScrollWheel");
            if (legacyScroll > 0.01f) return 1;
            if (legacyScroll < -0.01f) return -1;
#endif

            return 0;
        }
    }

    private static bool KeyPressedThisFrame(SpyKey key)
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            switch (key)
            {
                case SpyKey.Space:
                    return Keyboard.current.spaceKey.wasPressedThisFrame;
                case SpyKey.V:
                    return Keyboard.current.vKey.wasPressedThisFrame;
                case SpyKey.B:
                    return Keyboard.current.bKey.wasPressedThisFrame;
                case SpyKey.E:
                    return Keyboard.current.eKey.wasPressedThisFrame;
                case SpyKey.G:
                    return Keyboard.current.gKey.wasPressedThisFrame;
                case SpyKey.R:
                    return Keyboard.current.rKey.wasPressedThisFrame;
                case SpyKey.C:
                    return Keyboard.current.cKey.wasPressedThisFrame;
                case SpyKey.M:
                    return Keyboard.current.mKey.wasPressedThisFrame;
                case SpyKey.F:
                    return Keyboard.current.fKey.wasPressedThisFrame;
                case SpyKey.Q:
                    return Keyboard.current.qKey.wasPressedThisFrame;
                case SpyKey.X:
                    return Keyboard.current.xKey.wasPressedThisFrame;
                case SpyKey.Up:
                    return Keyboard.current.wKey.wasPressedThisFrame ||
                        Keyboard.current.upArrowKey.wasPressedThisFrame;
                case SpyKey.Down:
                    return Keyboard.current.sKey.wasPressedThisFrame ||
                        Keyboard.current.downArrowKey.wasPressedThisFrame;
                case SpyKey.Left:
                    return Keyboard.current.aKey.wasPressedThisFrame ||
                        Keyboard.current.leftArrowKey.wasPressedThisFrame;
                case SpyKey.Right:
                    return Keyboard.current.dKey.wasPressedThisFrame ||
                        Keyboard.current.rightArrowKey.wasPressedThisFrame;
                case SpyKey.Enter:
                    return Keyboard.current.enterKey.wasPressedThisFrame ||
                        Keyboard.current.numpadEnterKey.wasPressedThisFrame;
                case SpyKey.Escape:
                    return Keyboard.current.escapeKey.wasPressedThisFrame;
            }
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        switch (key)
        {
            case SpyKey.Space:
                return Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space);
            case SpyKey.V:
                return Input.GetKeyDown(KeyCode.V);
            case SpyKey.B:
                return Input.GetKeyDown(KeyCode.B);
            case SpyKey.E:
                return Input.GetKeyDown(KeyCode.E);
            case SpyKey.G:
                return Input.GetKeyDown(KeyCode.G);
            case SpyKey.R:
                return Input.GetKeyDown(KeyCode.R);
            case SpyKey.C:
                return Input.GetKeyDown(KeyCode.C);
            case SpyKey.M:
                return Input.GetKeyDown(KeyCode.M);
            case SpyKey.F:
                return Input.GetKeyDown(KeyCode.F);
            case SpyKey.Q:
                return Input.GetKeyDown(KeyCode.Q);
            case SpyKey.X:
                return Input.GetKeyDown(KeyCode.X);
            case SpyKey.Up:
                return Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
            case SpyKey.Down:
                return Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow);
            case SpyKey.Left:
                return Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
            case SpyKey.Right:
                return Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
            case SpyKey.Enter:
                return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
            case SpyKey.Escape:
                return Input.GetKeyDown(KeyCode.Escape);
            default:
                return false;
        }
#else
        return false;
#endif
    }

    private enum SpyKey
    {
        Space,
        V,
        B,
        E,
        G,
        R,
        C,
        M,
        F,
        Q,
        X,
        Up,
        Down,
        Left,
        Right,
        Enter,
        Escape
    }
}
