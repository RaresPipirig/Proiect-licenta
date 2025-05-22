using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class FireballController : MonoBehaviour
{
    private Vector2 direction;
    [SerializeField] private float speed;
    Rigidbody2D rb;

    [SerializeField] private float detectionRange;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private int damage;

    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 target)
    {
        direction = target;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + 90f);
    }

    void Update()
    {
        CheckPlayerHit();
    }

    private void FixedUpdate()
    {
        rb.velocity = direction * speed;
    }

    public void TakeDamage()
    {
        Destroy(gameObject);
    }

    private void CheckPlayerHit()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);

        if (hit != null)
        {
            hit.transform.parent.GetComponent<PlayerController>()?.TakeDamage(damage, direction);
            Destroy(gameObject);
        }
    }
}
