using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using static HandlePlayerMovement;

public class HandlePlayerAnimations : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private HandlePlayerMovement pm;

    [Header("Settings")]
    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;
    [SerializeField] private float maxWalkVelo = 0.5f;
    [SerializeField] private float maxRunVelo = 2f;

    private float velocityX;
    private float velocityZ;

    private int velocityXHash;
    private int velocityZHash;

    private int isOnAirHash;
    private int isCrouchingHash;

    private InputSystem_Actions actions;

    private void Start()
    {
        if (!IsOwner)
            return;

        velocityXHash = Animator.StringToHash("Velocity X");
        velocityZHash = Animator.StringToHash("Velocity Z");

        isOnAirHash = Animator.StringToHash("IsOnAir");
        isCrouchingHash = Animator.StringToHash("IsCrouching");

        actions = new InputSystem_Actions();
        actions.Enable();
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        bool isPlayerSprinting = pm.currentMoveState.Value == MovementStates.Sprinting && !pm.isExhausted;
        bool isPlayerCrouching = pm.currentMoveState.Value == MovementStates.Crouching;
        bool isOnAir = pm.currentMoveState.Value == MovementStates.OnAir;

        float currentMaxVelo = isPlayerSprinting ? maxRunVelo : maxWalkVelo;

        Vector2 moveInput = actions.Player.Movement.ReadValue<Vector2>();

        bool isMovingForward = moveInput.y > 0;
        bool isMovingBackward = moveInput.y < 0;
        bool isMovingLeft = moveInput.x < 0;
        bool isMovingRight = moveInput.x > 0;

        if ((isMovingForward || isMovingBackward) && velocityZ < currentMaxVelo)
        {
            velocityZ += Time.deltaTime * acceleration;
        }

        if (isMovingLeft && velocityX > -currentMaxVelo)
        {
            velocityX -= Time.deltaTime * acceleration;
        }

        if (isMovingRight && velocityX < currentMaxVelo)
        {
            velocityX += Time.deltaTime * acceleration;
        }

        // decelerate
        if (!(isMovingForward || isMovingBackward) && velocityZ > 0.0f)
        {
            velocityZ -= Time.deltaTime * deceleration;
        }

        // reset
        if (!(isMovingForward || isMovingBackward) && velocityZ < 0.0f)
        {
            velocityZ = 0.0f;
        }

        // lock (still sprinting, clamp to run max)
        if ((isMovingForward || isMovingBackward) && isPlayerSprinting && velocityZ > currentMaxVelo)
            velocityZ = currentMaxVelo;

        // decelerate to the max walk velocity (sprint was released while still moving)
        else if ((isMovingForward || isMovingBackward) && velocityZ > currentMaxVelo)
        {
            velocityZ -= Time.deltaTime * deceleration;

            // round to currentMaxVelocity if within offset
            if (velocityZ > currentMaxVelo && velocityZ < (currentMaxVelo + 0.05f))
                velocityZ = currentMaxVelo;
        }

        // round to currentMaxVelocity if within offset
        else if ((isMovingForward || isMovingBackward) && velocityZ < currentMaxVelo && velocityZ > (currentMaxVelo - 0.05f))
            velocityZ = currentMaxVelo;

        // decelerate
        if (!isMovingLeft && velocityX < 0.0f)
        {
            velocityX += Time.deltaTime * deceleration;
        }

        // decelerate
        if (!isMovingRight && velocityX > 0.0f)
        {
            velocityX -= Time.deltaTime * deceleration;
        }

        // reset
        if (!isMovingLeft && !isMovingRight && velocityX != 0.0f && (velocityX > -0.05f && velocityX < 0.05f))
        {
            velocityX = 0.0f;
        }

        // lock
        if (isMovingLeft && isPlayerSprinting && velocityX < -currentMaxVelo)
            velocityX = -currentMaxVelo;

        // deceleraate to the max walk velocity
        else if (isMovingLeft && velocityX < -currentMaxVelo)
        {
            velocityX += Time.deltaTime * deceleration;

            // round to currentMaxVelocity if within offset
            if (velocityX < -currentMaxVelo && velocityX > (-currentMaxVelo + 0.05f))
                velocityX = -currentMaxVelo;
        }

        // round to currentMaxVelocity if within offset
        else if (isMovingLeft && velocityX > -currentMaxVelo && velocityX < (-currentMaxVelo - 0.05f))
            velocityX = -currentMaxVelo;

        // lock right
        if (isMovingRight && isPlayerSprinting && velocityX > currentMaxVelo)
            velocityX = currentMaxVelo;

        // deceleraate to the max walk velocity
        else if (isMovingRight && velocityX > currentMaxVelo)
        {
            velocityX -= Time.deltaTime * deceleration;

            // round to currentMaxVelocity if within offset
            if (velocityX > currentMaxVelo && velocityX < (currentMaxVelo + 0.05f))
                velocityX = currentMaxVelo;
        }

        // round to currentMaxVelocity if within offset
        else if (isMovingRight && velocityX < currentMaxVelo && velocityX > (currentMaxVelo - 0.05f))
            velocityX = currentMaxVelo;

        animator.SetFloat(velocityXHash, velocityX);
        animator.SetFloat(velocityZHash, velocityZ);

        animator.SetBool(isCrouchingHash, isPlayerCrouching);
        animator.SetBool(isOnAirHash, isOnAir);
    }
}
