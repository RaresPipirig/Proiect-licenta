using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using Assets.Scripts.Characters.Main_characters.Swordsman;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private AttackController attackController;
    [SerializeField] internal MovementController movementController;
    [SerializeField] private HPSystem hpSystem;
    [SerializeField] internal StaminaSystem system;

    private PlayerInput playerInput;

    internal Transform aimIndicator;
    [SerializeField] internal LayerMask enemyLayer;

    [Space]

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce;
    [SerializeField] private float knockbackDuration;

    [Space]

    [Header("Debug")]
    [SerializeField] internal float HP;
    [SerializeField] internal bool isInvincible;

    private void Awake()
    {
        playerInput = new PlayerInput();
        aimIndicator = GetComponent<Transform>().Find("aim_indicator");
    }

    private void OnEnable()
    {
        playerInput.Player.Move.performed += ctx => movementController.movementInput = ctx.ReadValue<Vector2>().normalized;
        playerInput.Player.Move.canceled += ctx => movementController.movementInput = Vector2.zero;
        playerInput.Player.Dash.started += ctx => movementController.StartDash();
        playerInput.Player.Dash.canceled += ctx => movementController.StopDash();
        playerInput.Player.Aim.performed += ctx => attackController.aimInput = ctx.ReadValue<Vector2>().normalized;
        playerInput.Player.Aim.canceled += ctx => attackController.aimInput = Vector2.zero;
        playerInput.Player.SwordSlash.performed += ctx => attackController.SwordSlash();
        playerInput.Player.DashAttack.performed += ctx => attackController.DashAttack();


        playerInput.Enable();
    }

    private void OnDisable()
    {
        playerInput.Disable();
    }

    void Update()
    {
        AimIndicator();
    }

    private void AimIndicator()
    {
        float angle = Mathf.Atan2(attackController.aimDirection.y, attackController.aimDirection.x) * Mathf.Rad2Deg - 90f;
        aimIndicator.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public async void TakeDamage(int damage, Vector2 aimDirection)
    {
        if (!isInvincible)
        {
            if (HP > damage)
            {
                HP -= damage;
                hpSystem.recoveryCooldown = hpSystem.recoveryDelay;
                print(gameObject.name + " hit for " + damage + "! Remaining HP: " + HP);

                movementController.rb.AddForce(aimDirection * knockbackForce, ForceMode2D.Impulse);
                movementController.canMove += 1;
                movementController.isSprinting = false;
                movementController.isDashing = false;
                movementController.isMoving = false;

                await Task.Delay((int)(knockbackDuration * 1000));

                movementController.rb.velocity = Vector2.zero;
                movementController.canMove -= 1;
            }
            else
            {
                GameObject.Destroy(gameObject);
            }
        }
    }

    public void Upgrade(string path)
    {
        switch (path)
        {
            case "HP":
                HP *= 120 / 100;
                break;
            case "Stamina":
                system.maxStamina *= 1.2f;
                system.recoveryRate *= 1.1f;
                break;
            case "Damage":
                attackController.slashDamage *= 125 / 100;
                attackController.dashDamage *= 125 / 100;
                break;
            case "Movement":
                movementController.moveSpeed *= 1.15f;
                movementController.dashSpeed *= 1.15f;
                movementController.sprintSpeed *= 1.15f;
                break;
            default:
                print("Wrong argument");
                break;
        }
    }
}
