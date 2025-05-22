using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    public PlayerController playerController;
    internal Vector2 aimInput;
    internal Vector2 aimDirection = Vector2.down;

    public float swordSlashCooldown;
    public bool isSlashing;
    public bool canSlash;
    public float slashAngle;
    public float slashRange;
    public float slashDuration;
    public float sprintDelay;
    public int slashDamage;
    public float slashStaminaCost;

    [SerializeField] private float dashDistance = 5f;
    [SerializeField] internal int dashDamage = 20;
    [SerializeField] private Vector2 hitboxSize = new Vector2(1f, 1f);
    [SerializeField] private bool canDashAttack = true;
    [SerializeField] private float dashAttackCooldown = 5f;
    [SerializeField] private float dashStaminaCost;

    private void Awake()
    {
        
    }

    void Update()
    {
        if (aimInput == Vector2.zero)
        {
            aimDirection = playerController.movementController.lastRecordedDirection;
            playerController.aimIndicator.gameObject.SetActive(false);
        }
        else
        {
            aimDirection = aimInput;
            if(!isSlashing)
                playerController.aimIndicator.gameObject.SetActive(true);
        }
    }

    private void Attack(Collider2D hit, int damage)
    {
        hit.transform.parent.GetComponent<DepravedController>()?.TakeDamage(damage, aimDirection);
        hit.transform.parent.GetComponent<MageController>()?.TakeDamage(damage, aimDirection);
        hit.transform.parent.GetComponent<FireballController>()?.TakeDamage();
    }

    internal async void SwordSlash()
    {
        if (playerController.movementController.isDashing 
            || !canSlash 
            || !playerController.system.UseStamina(slashStaminaCost))
        {
            return;
        }

        isSlashing = true;
        playerController.aimIndicator.gameObject.SetActive(false);
        canSlash = false;

        if (playerController.movementController.isDashing)
        {
            playerController.movementController.isDashing = false;
            await Task.Delay((int)(sprintDelay * 1000));
        }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(new Vector2(transform.position.x, transform.position.y + 0.1f), slashRange, playerController.enemyLayer);

        foreach (Collider2D hit in hitEnemies)
        {
            Vector2 directionToEnemy = (hit.transform.position - transform.position).normalized;
            float angleToEnemy = Vector2.Angle(aimDirection, directionToEnemy);

            if (angleToEnemy <= slashAngle / 2)
            {
                Attack(hit, slashDamage);
            }
        }

        await Task.Delay((int)(slashDuration * 1000));
        isSlashing = false;
        playerController.aimIndicator.gameObject.SetActive(true);
        await Task.Delay((int)(swordSlashCooldown * 1000));
        canSlash = true;
    }

    internal async void DashAttack()
    {
        if (!canDashAttack 
            || playerController.movementController.canMove != 0
            || !playerController.system.UseStamina(dashStaminaCost))
            return;

        canDashAttack = false;

        Vector2 start = transform.position;
        Vector2 end = start + aimDirection.normalized * dashDistance;

        Vector2 dashDirection = (end - start).normalized;
        float distance = dashDistance;

        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            start,
            hitboxSize,
            Mathf.Atan2(dashDirection.y, dashDirection.x) * Mathf.Rad2Deg,
            dashDirection,
            distance,
            playerController.enemyLayer
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                Attack(hit.collider, dashDamage);
            }
        }

        transform.position = end;

        await Task.Delay((int)(dashAttackCooldown * 1000));
        canDashAttack = true;
    }
}
