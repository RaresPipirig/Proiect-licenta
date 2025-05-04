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
    [SerializeField] private bool isMoving;
    [SerializeField] private int moveSpeed;

    [SerializeField] private int isStunned = 0;
    [SerializeField] private float slashStunDuration;

    Vector2 playerDirection = Vector2.zero;

    [SerializeField] private int detectionRange;
    [SerializeField] private LayerMask playerLayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        GetPlayerDirection();
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
        rb.velocity = playerDirection * moveSpeed;
    }

    private void GetPlayerDirection()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);

        if (hit != null)
        {
            playerDirection = (hit.transform.position - transform.position).normalized;
            isMoving = true;
        }
        else
        {
            playerDirection = Vector2.zero;
            isMoving = false;
        }
    }

    public async void TakeDamage(int damage, Vector2 aimDirection)
    {
        HP -= damage;
        print(gameObject.name + " hit! Remaining HP: " + HP);

        rb.AddForce(aimDirection * knockbackForce, ForceMode2D.Impulse);
        canMove = false;
        isMoving = false;
        await Task.Delay((int)(knockbackDuration * 1000));
        rb.velocity = Vector2.zero;
        isStunned += 1;
        await Task.Delay((int)(slashStunDuration * 1000));
        isStunned -= 1;
        canMove = true;
    }
}
