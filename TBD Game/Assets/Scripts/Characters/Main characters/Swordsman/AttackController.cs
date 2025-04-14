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

    private void Awake()
    {
        Transform child = GetComponent<Transform>().Find("SwordHitbox");
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

    internal async void SwordSlash()
    {
        if (playerController.movementController.isDashing || !canSlash)
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
                hit.transform.parent.GetComponent<DepravedController>()?.TakeDamage(slashDamage, aimDirection);
            }
        }

        await Task.Delay((int)(slashDuration * 1000));
        isSlashing = false;
        playerController.aimIndicator.gameObject.SetActive(true);
        await Task.Delay((int)(swordSlashCooldown * 1000));
        canSlash = true;
    }
}
