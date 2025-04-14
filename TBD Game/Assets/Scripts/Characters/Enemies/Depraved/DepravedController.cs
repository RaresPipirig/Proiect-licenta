using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DepravedController : MonoBehaviour
{
    public int HP;
    Rigidbody2D rb;

    public float knockbackForce;
    public bool canMove = true;
    public float knockbackDuration;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        
    }

    public async void TakeDamage(int damage, Vector2 aimDirection)
    {
        HP -= damage;

        rb.AddForce(aimDirection * knockbackForce, ForceMode2D.Impulse);
        canMove = false;
        await Task.Delay((int)(knockbackDuration * 1000));
        rb.velocity = Vector2.zero;
        canMove = true;

        print(gameObject.name + " hit! Remaining HP: " + HP);
    }
}
