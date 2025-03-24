using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using Assets.Scripts.Characters.Main_characters.Swordsman;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public bool isMoving;

    public float dashSpeed;
    public bool isDashing;
    private bool stopDash;
    private float dashStartTime;
    public float maxDashTime;
    public float minDashTime;

    public float sprintSpeed;
    public bool isSprinting;
    private bool sprintFlag;

    public float dashCooldown;
    public bool canDash = true;
    public bool hasReleasedDash;


    public CharacterAnimationController animationController;

    private PlayerInput playerInput;
    private Vector2 movementInput;
    private Rigidbody2D rb;
    private Vector2 lastRecordedDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = new PlayerInput();
        
    }

    private void OnEnable()
    {
        playerInput.Player.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        playerInput.Player.Move.canceled += ctx => movementInput = Vector2.zero;
        playerInput.Player.Dash.started += ctx => StartDash();
        playerInput.Player.Dash.canceled += ctx => StopDash();

        playerInput.Enable();
    }

    private void OnDisable()
    {
        playerInput.Disable();
    }

    void Update()
    {

    }

    private async void StartDash()
    {
        hasReleasedDash = false;
        if (isDashing || !isMoving || !canDash) return;

        isDashing = true;
        canDash = false;
        stopDash = false;
        sprintFlag = true;
        
        dashStartTime = Time.time;

        while (!stopDash && (Time.time - dashStartTime) < maxDashTime
            || (Time.time - dashStartTime) < minDashTime)
        {
            await Task.Yield();
        }

        isDashing = false;
        if (sprintFlag)
        {
            isSprinting = true;
        }

        await Task.Delay((int)(dashCooldown * 1000));
        while (!hasReleasedDash) { await Task.Yield(); }
        canDash = true;
    }

    private void StopDash()
    {
        if (isDashing)
        {
            sprintFlag = false;
        }
        stopDash = true;

        hasReleasedDash = true;
    }

    private void FixedUpdate()
    {
        if (movementInput == Vector2.zero && !isDashing)
        {
            isMoving = false;
            isSprinting = false;
            rb.velocity = Vector2.zero;
        }

        Move(movementInput);
    }

    private void Move(Vector2 targetPos)
    {
        targetPos.Normalize();

        if (targetPos != Vector2.zero)
        {
            isMoving = true;
            lastRecordedDirection = targetPos;
        }

        if (isMoving && !isDashing && !isSprinting)
        {
            rb.velocity = targetPos * moveSpeed;
            return;
        }else if(isSprinting && !isDashing)
        {
            rb.velocity = targetPos * sprintSpeed;
            return;
        }

        if(targetPos == Vector2.zero)
            targetPos = lastRecordedDirection;

        if (isDashing)
            rb.velocity = targetPos * dashSpeed;
    }
}
