using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    public PlayerController playerController;

    internal Vector2 movementInput;
    public Rigidbody2D rb;
    internal Vector2 lastRecordedDirection = Vector2.down;

    public float moveSpeed;
    public bool isMoving;
    public int canMove;

    public float dashSpeed;
    public bool isDashing;
    private bool stopDash;
    private float dashStartTime;
    public float maxDashTime;
    public float minDashTime;

    public float sprintSpeed;
    public bool isSprinting;
    private bool canSprint;

    public float dashCooldown;
    public bool canDash = true;
    public bool hasReleasedDash;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (movementInput == Vector2.zero && !isDashing)
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
        if (isDashing || !isMoving || !canDash) return;

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
