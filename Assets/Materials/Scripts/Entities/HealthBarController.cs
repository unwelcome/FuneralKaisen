using System;
using UnityEngine;

public class HealthBarController : MonoBehaviour
{
    [Header("HealthBarPoint")]
    [SerializeField] private bool showHealthBar = true; // Показывать шкалу хп
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] armorSprites; // (100%, 80%, ...)
    [SerializeField] private Sprite[] healthSprites; // (100%, 80%, ...)

    private HealthController health;

    private void Start()
    {
        health = GetComponentInParent<HealthController>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        health.OnHealthChanged += UpdateBar;

        UpdateBar(health.Armor, health.Armor, health.Health, health.Health); // Первый рендер
    }

    private void OnDestroy()
    {
        if (health != null) health.OnHealthChanged -= UpdateBar;
    }

    private void UpdateBar(int currentArmor, int maxArmor, int currentHealth, int maxHealth)
    {
        if (currentArmor > 0)
        {
            float percent = (float)currentArmor / maxArmor;
            spriteRenderer.sprite = armorSprites[getSpriteIndex(percent, armorSprites.Length - 1)];
        }
        else
        {
            float percent = (float)currentHealth / maxHealth;
            spriteRenderer.sprite = healthSprites[getSpriteIndex(percent, healthSprites.Length - 1)];
        }
    }

    private int getSpriteIndex(float percent, int spritesCount)
    {
        int len = spritesCount - 1; // Не учитываем последний спрайт с пустым health bar-ом

        int index = Mathf.Clamp(
            Mathf.FloorToInt(len - percent * len),
            0,
            len
        );

        // Если хп нет, то выводим пустой health bar
        if (percent <= 0) return len + 1;

        return index;
    }
}
