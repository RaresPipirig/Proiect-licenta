using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MageController : MonoBehaviour
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
    [SerializeField] private float tooFarRange;
    [SerializeField] private LayerMask playerLayer;
    Collider2D close;
    Collider2D far;

    [SerializeField] private bool isAttacking;
    [SerializeField] private bool canAttack;
    private Transform attackIndicator;
    [SerializeField] private float attackDelay;
    [SerializeField] private float attackDuration;

    [SerializeField] private float fireballCooldown;
    [SerializeField] private GameObject fireballPrefab;
    Vector2 spawnOffset = new Vector2(0f, 1f);

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        attackIndicator = GetComponent<Transform>().Find("attack_indicator");
    }

    void Update()
    {
        GetPlayerDirection();

        if (canAttack && far != null)
            HitPlayer();
    }

    private void FixedUpdate()
    {
        if (canMove && isStunned == 0)
        {
            Move();
        }
    }

    private void Move()
    {
        close = Physics2D.OverlapCircle(transform.position, tooCloseRange, playerLayer);
        far = Physics2D.OverlapCircle(transform.position, tooFarRange, playerLayer);

        if (!isAttacking && far == null)
            rb.velocity = playerDirection * moveSpeed;
        else if(!isAttacking && close != null)
            rb.velocity = -playerDirection * moveSpeed;
        else
            rb.velocity = Vector2.zero;
    }

    private void GetPlayerDirection()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);

        if (hit != null)
        {
            playerDirection = (hit.transform.position - transform.position).normalized;
        }
        else
        {
            playerDirection = Vector2.zero;
        }
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
        if (!canAttack || isStunned != 0)
        {
            return;
        }

        canAttack = false;
        isAttacking = true;

        rb.velocity = Vector2.zero;
        attackIndicator.gameObject.SetActive(true);
        await Task.Delay((int)(attackDelay * 1000));
        attackIndicator.gameObject.SetActive(false);

        if (isStunned != 0)
        {
            isAttacking = false;
            canAttack = true;
            return;
        }

        GameObject fireball = Instantiate(fireballPrefab, 
            (Vector2)transform.position + spawnOffset + playerDirection * 1f, 
            Quaternion.identity);
        FireballController controller = fireball.GetComponent<FireballController>();
        controller.Initialize(playerDirection);

        await Task.Delay((int)(attackDuration * 1000));
        isAttacking = false;

        await Task.Delay((int)(fireballCooldown * 1000));
        canAttack = true;
    }
}
