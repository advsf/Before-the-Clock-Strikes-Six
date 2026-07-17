using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class HandlePlayerMovement : NetworkBehaviour
{
    public enum MovementStates
    {
        Walking,
        Sprinting,
        OnAir,
        Idle
    }

    public static NetworkVariable<MovementStates> currentMoveState = new(MovementStates.Idle, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform orientation;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float gravity;

    [Header("Stamina Settings")]
    [SerializeField] private float staminaDecreaseAmount = 0.1f;
    [SerializeField] private float staminaIncreaseAmount = 0.075f;
    [SerializeField] private float staminaCooldown = 5f; // if stamina was depleted, how long before user can sprint again
    public float maxStamina = 100f;
    public float currentStamina;

    [Header("Jump Setings")]
    [SerializeField] private float jumpPower = 2f;

    private float verticalVelocity;
    private Vector2 moveDirection;

    private bool isSprinting;
    private bool isExhausted = false;

    private InputSystem_Actions action;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            action = new InputSystem_Actions();

            action.Player.Movement.Enable();
            action.Player.Sprint.Enable();
            action.Player.Jump.Enable();
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsOwner)
        {
            action.Player.Movement.Disable();
            action.Player.Sprint.Disable();
            action.Player.Jump.Disable();
        }
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        moveDirection = action.Player.Movement.ReadValue<Vector2>().normalized;
        isSprinting = action.Player.Sprint.IsPressed();

        // handle states
        HandleMoveStates();

        HandleStamina();

        HandleMoving();
    }

    private void HandleMoving()
    {
        Vector3 move = orientation.right * moveDirection.x + orientation.forward * moveDirection.y;

        // set speed
        float desiredSpeed = isSprinting && !isExhausted ? runSpeed : walkSpeed;

        // reset y velocity if grounded
        if (characterController.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        if (action.Player.Jump.triggered && characterController.isGrounded && !isExhausted)
            HandleJumping();

        // apply desired speed
        move = desiredSpeed * move;

        // set the y velocity directly for gravity
        // and prevents weird bugs
        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        characterController.Move(Time.deltaTime * move);
    }

    private void HandleJumping()
    {
        verticalVelocity = Mathf.Sqrt(jumpPower * -2f * gravity);
    }

    private void HandleStamina()
    {
        if (isSprinting && !isExhausted)
        {
            currentStamina -= staminaDecreaseAmount * Time.deltaTime;
        }

        // recharge only if grounded
        else if (characterController.isGrounded)
        {
            currentStamina += staminaIncreaseAmount * Time.deltaTime;
        }

        // cooldown
        if (currentStamina <= 0 && !isExhausted)
        {
            StartCoroutine(HandleStaminaCooldown());
        }

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }

    private IEnumerator HandleStaminaCooldown()
    {
        isExhausted = true;

        yield return new WaitForSeconds(staminaCooldown);

        isExhausted = false;
    }

    private void HandleMoveStates()
    {
        if (moveDirection != Vector2.zero)
        {
            currentMoveState.Value = isSprinting && !isExhausted ? MovementStates.Sprinting : MovementStates.Walking;
        }

        else if (characterController.isGrounded)
        {
            currentMoveState.Value = MovementStates.OnAir;
        }

        else
        {
            currentMoveState.Value = MovementStates.Idle;
        }
    }
}
