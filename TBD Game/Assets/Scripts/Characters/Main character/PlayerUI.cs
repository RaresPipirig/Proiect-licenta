using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider staminaBar;

    [Header("Player Systems")]
    [SerializeField] private HPSystem hpSystem;
    [SerializeField] private StaminaSystem staminaSystem;

    void Start()
    {
        healthBar = GameObject.FindWithTag("HPBar").GetComponent<Slider>();
        staminaBar = GameObject.FindWithTag("StaminaBar").GetComponent<Slider>();

        if (healthBar != null)
        {
            healthBar.maxValue = 1f;
            healthBar.value = 1f;
        }

        if (staminaBar != null)
        {
            staminaBar.maxValue = 1f;
            staminaBar.value = 1f;
        }
    }

    void Update()
    {
        UpdateHealthBar();
        UpdateStaminaBar();
    }

    void UpdateHealthBar()
    {
        if (healthBar != null && hpSystem != null)
        {
            healthBar.value = hpSystem.GetHPPercent();
        }
    }

    void UpdateStaminaBar()
    {
        if (staminaBar != null && staminaSystem != null)
        {
            staminaBar.value = staminaSystem.GetStaminaPercent();
        }
    }
}
