using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPSystem : MonoBehaviour
{
    [Header("HP Settings")]
    [SerializeField] internal float maxHP;
    [SerializeField] internal float recoveryRate;
    [SerializeField] internal float recoveryDelay;
    [SerializeField] internal float recoveryCooldown;

    [Space]

    [Header("Debug")]
    public PlayerController playerController;

    void Awake()
    {
        playerController.HP = maxHP;
    }
    private void Update()
    {
        if (recoveryCooldown > 0)
        {
            recoveryCooldown -= Time.deltaTime;
        }
        else if (playerController.HP < maxHP)
        {
            playerController.HP += recoveryRate * Time.deltaTime;
            playerController.HP = Mathf.Min(playerController.HP, maxHP);
        }
    }

    public float GetHPPercent()
    {
        return playerController.HP / maxHP;
    }
}
