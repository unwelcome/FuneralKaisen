using System;
using UnityEngine;

public class HealthBarController : MonoBehaviour
{
    [Header("HealthBar")]
    [SerializeField] private bool showHealthBar = true; // Показывать шкалу хп
    [SerializeField] private SpriteRenderer healthBarSpriteRenderer;
    [SerializeField] private Sprite armorSprite;
    [SerializeField] private Sprite healthSprite;

    private HealthController health;
    private float maxWidth;

    private void Start()
    {
        health = GetComponentInParent<HealthController>();
        maxWidth = healthBarSpriteRenderer.size.x;

        health.OnHealthChanged += UpdateBar;

        UpdateBar(health.Armor, health.Armor, health.Health, health.Health); // Первый рендер
    }

    private void OnDestroy()
    {
        if (health != null) health.OnHealthChanged -= UpdateBar;
    }

    private void UpdateBar(int currentArmor, int maxArmor, int currentHealth, int maxHealth)
    {
        float percent = 0;
        if (currentArmor > 0)
        {
            percent = (float)currentArmor / maxArmor;
            healthBarSpriteRenderer.sprite = armorSprite;
        }
        else
        {
            percent = (float)currentHealth / maxHealth;
            healthBarSpriteRenderer.sprite = healthSprite;
        }

        // Изменяем длину спрайта с хп
        healthBarSpriteRenderer.size = new Vector2(percent * maxWidth, healthBarSpriteRenderer.size.y);
    }
}
