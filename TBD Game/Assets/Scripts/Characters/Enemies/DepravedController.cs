using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DepravedController : MonoBehaviour
{
    [SerializeField] private int HP;
    Rigidbody2D rb;

    [SerializeField] private float knockbackForce;
    [SerializeField] private float knockbackDuration;

    [SerializeField] private bool canMove = true;
    [SerializeField] private float moveSpeed;

    [SerializeField] private int isStunned = 0;
    [SerializeField] private float slashStunDuration;

    Vector2 playerDirection = Vector2.zero;

    [SerializeField] private int detectionRange;
    [SerializeField] private float tooCloseRange;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [SerializeField] private bool canAttack = true;
    [SerializeField] private bool isAttacking = false;
    [SerializeField] private float attackRange;
    [SerializeField] private float attackInitiationRange;
    [SerializeField] private float attackDelay;
    [SerializeField] private float attackDuration;
    [SerializeField] private float attackCooldown;
    [SerializeField] private int attackDamage;
    private Transform attackIndicator;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        attackIndicator = GetComponent<Transform>().Find("attack_indicator");
    }

    void Update()
    {
        GetPlayerDirection();
    }

    private void FixedUpdate()
    {
        if (canAttack && isCloseEnoughToPlayer() && isStunned == 0)
            HitPlayer();

        if (canMove && isStunned == 0)
        {
            Move();
        }
    }

    private void Move()
    {
        Collider2D close = Physics2D.OverlapCircle(transform.position, tooCloseRange, playerLayer);

        if (!isAttacking && close == null)
            rb.velocity = playerDirection * moveSpeed;
        else
            rb.velocity = Vector2.zero;
    }

    private void GetPlayerDirection()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
        if (hit != null)
        {
            Vector2 directionToPlayer = hit.transform.position - transform.position;
            
            if (HasLineOfSight(hit.transform.position))
            {
                playerDirection = directionToPlayer.normalized;
            }
            else
            {
                playerDirection = Vector2.zero;
            }
        }
        else
        {
            playerDirection = Vector2.zero;
        }
    }

    private bool HasLineOfSight(Vector3 targetPosition)
    {
        Vector2 direction = targetPosition - transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, direction.magnitude, obstacleLayer);

        Debug.DrawRay(transform.position, direction, hit.collider == null ? Color.green : Color.red, 1f);

        return hit.collider == null;
    }

    private bool isCloseEnoughToPlayer()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackInitiationRange, playerLayer);

        return hit != null;
    }

    public async void TakeDamage(int damage, Vector2 aimDirection)
    {
        HP -= damage;
        print(gameObject.name + " hit! Remaining HP: " + HP);

        if (HP <= 0)
            Destroy(gameObject);

        rb.AddForce(aimDirection * knockbackForce, ForceMode2D.Impulse);
        canMove = false;
        await Task.Delay((int)(knockbackDuration * 1000));
        rb.velocity = Vector2.zero;
        isStunned += 1;
        await Task.Delay((int)(slashStunDuration * 1000));
        isStunned -= 1;
        canMove = true;
    }

    public async void HitPlayer()
    {
        if (!canAttack)
        {
            return;
        }

        canAttack = false;
        isAttacking = true;
        
        rb.velocity = Vector2.zero;
        attackIndicator.gameObject.SetActive(true);
        await Task.Delay((int)(attackDelay * 1000));
        attackIndicator.gameObject.SetActive(false);

        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);
        if (hit != null && isStunned == 0)
        {
            hit.transform.parent.GetComponent<PlayerController>()?.TakeDamage(attackDamage, playerDirection);
        }

        await Task.Delay((int)(attackDuration * 1000));
        isAttacking = false;

        await Task.Delay ((int)(attackCooldown * 1000));
        canAttack = true;
    }
}
