using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;

    internal Vector2 movementInput;
    [SerializeField] internal Rigidbody2D rb;
    internal Vector2 lastRecordedDirection = Vector2.down;

    [Space]

    [Header("Walk Settings")]
    [SerializeField] internal float moveSpeed;

    [Space]

    [Header("Dash Settings")]
    [SerializeField] internal float dashSpeed;
    private bool stopDash;
    private float dashStartTime;
    [SerializeField] private float maxDashTime;
    [SerializeField] private float minDashTime;
    [SerializeField] private float dashCooldown;
    [SerializeField] private float dashStaminaCost;

    [Space]

    [Header("Sprint Settings")]
    [SerializeField] internal float sprintSpeed;
    [SerializeField] internal bool isSprinting;

    [Space]

    [Header("Debug")]
    [SerializeField] internal bool isMoving;
    [SerializeField] internal int canMove;
    private bool canSprint;
    [SerializeField] private bool canDash = true;
    [SerializeField] private bool hasReleasedDash;
    [SerializeField] internal bool isDashing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (movementInput == Vector2.zero && !isDashing && canMove == 0)
        {
            isMoving = false;
            isSprinting = false;
            rb.velocity = Vector2.zero;
        }

        if (canMove == 0)
        {
            Move(movementInput);
        }
    }

    internal async void StartDash()
    {
        hasReleasedDash = false;
        if (isDashing || !isMoving || !canDash || playerController.system.UseStamina(dashStaminaCost)) return;

        isDashing = true;
        canDash = false;
        stopDash = false;
        canSprint = true;
        playerController.isInvincible = true;

        dashStartTime = Time.time;

        while (!stopDash && (Time.time - dashStartTime) < maxDashTime
            || (Time.time - dashStartTime) < minDashTime)
        {
            if ((Time.time - dashStartTime) > minDashTime)
                playerController.isInvincible = false;
            await Task.Yield();
        }

        playerController.isInvincible = false;
        isDashing = false;
        if (canSprint)
        {
            isSprinting = true;
        }

        await Task.Delay((int)(dashCooldown * 1000));
        while (!hasReleasedDash) { await Task.Yield(); }
        canDash = true;
    }

    internal void StopDash()
    {
        if (isDashing)
        {
            canSprint = false;
        }
        stopDash = true;

        hasReleasedDash = true;
    }

    private void Move(Vector2 targetPos)
    {
        if (targetPos != Vector2.zero)
        {
            isMoving = true;
            lastRecordedDirection = targetPos;
        }

        if (isMoving && !isDashing && !isSprinting)
        {
            rb.velocity = targetPos * moveSpeed;
            return;
        }
        else if (isSprinting && !isDashing)
        {
            rb.velocity = targetPos * sprintSpeed;
            return;
        }

        if (targetPos == Vector2.zero)
            targetPos = lastRecordedDirection;

        if (isDashing)
            rb.velocity = targetPos * dashSpeed;
    }
}
