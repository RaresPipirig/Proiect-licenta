using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepravedController : MonoBehaviour
{
    public int HP;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void TakeDamage(int damage)
    {
        HP -= damage;
        print(gameObject.name + " hit! Remaining HP: " + HP);
    }
}
