using UnityEngine;

public class FirstPersonViewModel : MonoBehaviour
{
    public SpyPlayerController player;
    public SpyWeaponController weapon;
    public Transform armsVisual;
    public Transform leftArm;
    public Transform rightArm;

    [Header("Motion")]
    public Vector3 restLocalPosition = Vector3.zero;
    public Vector3 restLocalEuler = Vector3.zero;
    public float walkBobRate = 7.5f;
    public float sprintBobRate = 11f;
    public Vector2 bobAmount = new Vector2(0.018f, 0.026f);
    public float lookSway = 0.24f;
    public float motionSmoothing = 13f;
    public Vector3 sprintLocalOffset = new Vector3(0.08f, -0.12f, -0.08f);
    public Vector3 sprintLocalEuler = new Vector3(17f, -12f, 8f);
    public Vector3 reloadLocalOffset = new Vector3(0.04f, -0.09f, -0.06f);
    public Vector3 reloadLocalEuler = new Vector3(13f, 0f, 24f);
    public Vector3 airborneLocalOffset = new Vector3(0f, -0.035f, -0.025f);
    public Vector3 airborneLocalEuler = new Vector3(-3.5f, 0f, 0f);
    public Vector3 recoilLocalOffset = new Vector3(0f, 0.012f, -0.08f);
    public Vector3 recoilLocalEuler = new Vector3(-5.5f, 0f, 0f);
    public float weaponSwapDuration = 0.4f;

    [Header("Carry Poses")]
    public float carryArmSmoothing = 11f;
    public Vector3 bridalLeftArmOffset = new Vector3(-0.16f, 0.08f, 0.2f);
    public Vector3 bridalLeftArmEuler = new Vector3(-24f, -12f, -20f);
    public Vector3 bridalRightArmOffset = new Vector3(0.16f, 0.08f, 0.24f);
    public Vector3 bridalRightArmEuler = new Vector3(-24f, 12f, 20f);
    public Vector3 shoulderRightArmOffset = new Vector3(0.2f, 0.04f, 0.12f);
    public Vector3 shoulderRightArmEuler = new Vector3(-32f, 16f, 34f);

    private Vector3 smoothedPosition;
    private Vector3 smoothedEuler;
    private bool firstPersonActive = true;
    private bool knifeMelee;
    private float meleeUntil;
    private Vector3 leftArmRestPosition;
    private Quaternion leftArmRestRotation;
    private Vector3 rightArmRestPosition;
    private Quaternion rightArmRestRotation;
    private bool armRestTransformsCached;
    private string observedWeaponId;
    private float weaponSwapStartedAt;
    private float weaponSwapUntil;

    private void Awake()
    {
        smoothedPosition = restLocalPosition;
        smoothedEuler = restLocalEuler;
    }

    private void LateUpdate()
    {
        if (!firstPersonActive || player == null) return;

        bool bridalCarry = player.IsCarrying && player.selectedCarryStyle == CarryStyle.BridalCarry;
        bool shoulderCarry = player.IsCarrying && player.selectedCarryStyle == CarryStyle.OverShoulder;
        UpdateArmPresentation(bridalCarry, shoulderCarry);

        Vector2 move = player.ControlsLocked ? Vector2.zero : SpyInput.Move;
        float moveAmount = Mathf.Clamp01(move.magnitude);
        float bobRate = SpyInput.SprintHeld && !player.IsCarrying ? sprintBobRate : walkBobRate;
        float phase = Time.time * bobRate;
        Vector3 bob = new Vector3(
            Mathf.Sin(phase) * bobAmount.x,
            Mathf.Abs(Mathf.Cos(phase)) * -bobAmount.y,
            0f) * moveAmount;

        Vector2 look = player.ControlsLocked ? Vector2.zero : SpyInput.Look;
        Vector3 swayEuler = new Vector3(
            -Mathf.Clamp(look.y, -8f, 8f),
            Mathf.Clamp(look.x, -8f, 8f),
            -Mathf.Clamp(look.x, -8f, 8f) * 0.35f) * lookSway;

        Vector3 targetPosition = restLocalPosition + bob;
        Vector3 targetEuler = restLocalEuler + swayEuler;
        bool reloading = weapon != null && weapon.IsReloading;
        bool sprinting = SpyInput.SprintHeld &&
            moveAmount > 0.1f &&
            !player.IsCarrying &&
            !player.IsCrouching &&
            !reloading;
        if (sprinting)
        {
            targetPosition += sprintLocalOffset;
            targetEuler += sprintLocalEuler;
        }
        if (reloading)
        {
            float reloadMotion = Mathf.Sin(Time.time * 8f) * 0.018f;
            targetPosition += reloadLocalOffset + Vector3.up * reloadMotion;
            targetEuler += reloadLocalEuler + Vector3.right * reloadMotion * 80f;
        }
        if (!player.IsGrounded)
        {
            targetPosition += airborneLocalOffset;
            targetEuler += airborneLocalEuler;
        }

        UpdateWeaponSwapMotion(ref targetPosition, ref targetEuler);
        float recoil = weapon != null ? Mathf.Clamp01(weapon.RecoilAmount) : 0f;
        targetPosition += recoilLocalOffset * recoil;
        targetEuler += recoilLocalEuler * recoil;
        if (Time.time < meleeUntil)
        {
            float normalized = 1f - Mathf.Clamp01((meleeUntil - Time.time) / 0.34f);
            float strike = Mathf.Sin(normalized * Mathf.PI);
            targetPosition += new Vector3(
                knifeMelee ? -0.08f : 0.1f,
                knifeMelee ? -0.04f : 0.02f,
                strike * (knifeMelee ? 0.34f : 0.22f));
            targetEuler += new Vector3(
                knifeMelee ? -12f : 5f,
                knifeMelee ? -8f : 10f,
                strike * (knifeMelee ? -18f : 24f));
        }
        float blend = 1f - Mathf.Exp(-motionSmoothing * Time.deltaTime);
        smoothedPosition = Vector3.Lerp(smoothedPosition, targetPosition, blend);
        smoothedEuler = Vector3.Lerp(smoothedEuler, targetEuler, blend);
        transform.localPosition = smoothedPosition;
        transform.localRotation = Quaternion.Euler(smoothedEuler);
    }

    private void UpdateWeaponSwapMotion(ref Vector3 targetPosition, ref Vector3 targetEuler)
    {
        string currentWeaponId = weapon != null && weapon.currentWeapon != null
            ? weapon.currentWeapon.weaponId
            : string.Empty;
        if (observedWeaponId == null)
        {
            observedWeaponId = currentWeaponId;
        }
        else if (observedWeaponId != currentWeaponId)
        {
            observedWeaponId = currentWeaponId;
            weaponSwapStartedAt = Time.time;
            weaponSwapUntil = Time.time + Mathf.Max(0.1f, weaponSwapDuration);
        }

        if (Time.time >= weaponSwapUntil) return;

        float duration = Mathf.Max(0.1f, weaponSwapUntil - weaponSwapStartedAt);
        float normalized = Mathf.Clamp01((Time.time - weaponSwapStartedAt) / duration);
        float lowerAmount = Mathf.Sin(normalized * Mathf.PI);
        targetPosition += new Vector3(0.025f, -0.2f, -0.12f) * lowerAmount;
        targetEuler += new Vector3(20f, -8f, 14f) * lowerAmount;
    }

    public void SetFirstPersonActive(bool active)
    {
        firstPersonActive = active;
        gameObject.SetActive(active);
    }

    public void PlayMelee(bool useKnife)
    {
        knifeMelee = useKnife;
        meleeUntil = Time.time + 0.34f;
    }

    private void SetArmVisibility(bool showLeft, bool showRight)
    {
        if (armsVisual != null && !armsVisual.gameObject.activeSelf)
        {
            armsVisual.gameObject.SetActive(true);
        }
        if (leftArm != null && leftArm.gameObject.activeSelf != showLeft)
        {
            leftArm.gameObject.SetActive(showLeft);
        }
        if (rightArm != null && rightArm.gameObject.activeSelf != showRight)
        {
            rightArm.gameObject.SetActive(showRight);
        }
    }

    private void UpdateArmPresentation(bool bridalCarry, bool shoulderCarry)
    {
        CacheArmRestTransforms();
        SetArmVisibility(true, bridalCarry || shoulderCarry || !player.IsCarrying);
        if (!armRestTransformsCached) return;

        Vector3 leftPosition = leftArmRestPosition;
        Quaternion leftRotation = leftArmRestRotation;
        Vector3 rightPosition = rightArmRestPosition;
        Quaternion rightRotation = rightArmRestRotation;

        if (bridalCarry)
        {
            leftPosition += bridalLeftArmOffset;
            leftRotation *= Quaternion.Euler(bridalLeftArmEuler);
            rightPosition += bridalRightArmOffset;
            rightRotation *= Quaternion.Euler(bridalRightArmEuler);
        }
        else if (shoulderCarry)
        {
            rightPosition += shoulderRightArmOffset;
            rightRotation *= Quaternion.Euler(shoulderRightArmEuler);
        }

        float blend = 1f - Mathf.Exp(-carryArmSmoothing * Time.deltaTime);
        leftArm.localPosition = Vector3.Lerp(leftArm.localPosition, leftPosition, blend);
        leftArm.localRotation = Quaternion.Slerp(leftArm.localRotation, leftRotation, blend);
        rightArm.localPosition = Vector3.Lerp(rightArm.localPosition, rightPosition, blend);
        rightArm.localRotation = Quaternion.Slerp(rightArm.localRotation, rightRotation, blend);
    }

    private void CacheArmRestTransforms()
    {
        if (armRestTransformsCached || leftArm == null || rightArm == null) return;

        leftArmRestPosition = leftArm.localPosition;
        leftArmRestRotation = leftArm.localRotation;
        rightArmRestPosition = rightArm.localPosition;
        rightArmRestRotation = rightArm.localRotation;
        armRestTransformsCached = true;
    }
}
