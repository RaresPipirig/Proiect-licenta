using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    public PlayerController playerController;
    internal Vector2 aimInput;

    public float swordSlashCooldown;
    public bool isSlashing;
    public bool canSlash;
    public Transform attackPoint;
    public float slashAngle;
    private float slashRange;
    public float slashDuration;
    public float sprintDelay;
    public int slashDamage;
    private Vector2 slashDirection = Vector2.down;

    private void Awake()
    {
        Transform child = GetComponent<Transform>().Find("SwordHitbox");
        slashRange = child.GetComponent<CircleCollider2D>().radius * GetComponent<Transform>().localScale.x;
    }

    void Update()
    {
        
    }

    internal async void SwordSlash()
    {
        if (playerController.isDashing || !canSlash)
        {
            return;
        }

        isSlashing = true;
        canSlash = false;

        if (playerController.isSprinting)
        {
            playerController.isSprinting = false;
            await Task.Delay((int)(sprintDelay * 1000));
        }

        if (aimInput == Vector2.zero)
            slashDirection = playerController.lastRecordedDirection;
        else
        {
            slashDirection = aimInput;
        }
        float angle = Mathf.Atan2(slashDirection.y, slashDirection.x) * Mathf.Rad2Deg;
        attackPoint.rotation = Quaternion.Euler(0, 0, angle);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(new Vector2(attackPoint.position.x, attackPoint.position.y + 0.1f), slashRange, playerController.enemyLayer);

        foreach (Collider2D hit in hitEnemies)
        {
            Vector2 directionToEnemy = (hit.transform.position - transform.position).normalized;
            float angleToEnemy = Vector2.Angle(slashDirection, directionToEnemy);

            if (angleToEnemy <= slashAngle / 2)
            {
                hit.transform.parent.GetComponent<DepravedController>()?.TakeDamage(slashDamage);
            }
        }

        await Task.Delay((int)(slashDuration * 1000));
        isSlashing = false;
        await Task.Delay((int)(swordSlashCooldown * 1000));
        canSlash = true;
    }
}
