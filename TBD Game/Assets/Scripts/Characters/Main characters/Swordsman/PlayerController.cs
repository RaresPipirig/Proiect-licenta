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
    public AttackController attackController;
    public MovementController movementController;

    private PlayerInput playerInput;

    internal Transform aimIndicator;
    public LayerMask enemyLayer;

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
}
