using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaSystem : MonoBehaviour
{
    [Header("Stamina Settings")]
    [SerializeField] internal float maxStamina;
    [SerializeField] internal float recoveryRate;
    [SerializeField] private float recoveryDelay;
    [SerializeField] private float recoveryCooldown;

    [Space]

    [Header("Debug")]
    [SerializeField] private float currentStamina;

    void Awake()
    {
        currentStamina = maxStamina;
    }

    private void Update()
    {
        if (recoveryCooldown > 0)
        {
            recoveryCooldown -= Time.deltaTime;
        }
        else if (currentStamina < maxStamina)
        {
            currentStamina += recoveryRate * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina);
        }
    }

    public bool UseStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            recoveryCooldown = recoveryDelay;
            return true;
        }
        return false;
    }

    public float GetStaminaPercent()
    {
        return currentStamina / maxStamina;
    }
}
