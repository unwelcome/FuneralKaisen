using System;
using UnityEngine;

public class HealthController : MonoBehaviour
{
    // ---- HP ----
    [Header("HP")]
    [SerializeField] private int MaxHealth; // хп
    public int Health { get; private set; } // макс хп

    // ---- Armor ----
    [Header("Armor")]
    [SerializeField] private int MaxArmor; // армор
    public int Armor { get; private set; } // макс армор

    // ---- Action -----
    public event Action<int, int, int, int> OnHealthChanged;

    // ---- BlockDamage ----
    public bool IsDamageBlocked = false; // может ли перс получать урон

    // -----------------------------------------------------------------------

    private void Awake()
    {
        Health = MaxHealth;
        Armor = MaxArmor;
        IsDamageBlocked = false;
    }

    // ---- HP ----

    // Увеличение максимального хп; если increaseCurrentHealth = true, то с пропорциональным увеличением текущего хп
    public void IncreaseMaxHealth(int extraHealth, bool increaseCurrentHealth = true)
    {
        float healthPercentage = Health / MaxHealth;
        MaxHealth += extraHealth;
        if (increaseCurrentHealth) Health = (int)(MaxHealth * healthPercentage);

        // Событие изменения хп
        OnHealthChanged?.Invoke(Armor, MaxArmor, Health, MaxHealth);
    }

    // Исцеление хп
    public void HealHealth(int healAmount)
    {
        Health = Math.Min(Health + healAmount, MaxHealth);

        // Событие изменения хп
        OnHealthChanged?.Invoke(Armor, MaxArmor, Health, MaxHealth);
    }

    // ---- Armor ----

    // Увеличение максимального кол-ва армора; если increaseCurrentArmor = true, то с пропорциональным увеличением текущего армора
    public void IncreaseMaxArmor(int extraArmor, bool increaseCurrentArmor = true)
    {
        float armorPercentage = Armor / MaxArmor;
        MaxArmor += extraArmor;
        if (increaseCurrentArmor) Armor = (int)(MaxArmor * armorPercentage);

        // Событие изменения хп
        OnHealthChanged?.Invoke(Armor, MaxArmor, Health, MaxHealth);
    }

    // Исцеление армора
    public void HealArmor(int healAmount)
    {
        Armor = Math.Min(Armor + healAmount, MaxArmor);

        // Событие изменения хп
        OnHealthChanged?.Invoke(Armor, MaxArmor, Health, MaxHealth);
    }

    // ---- Damage ----

    // Получение урона
    public bool GetDamage(int damage)
    {
        // Перс не может получать урон
        if (IsDamageBlocked) return false;

        Debug.Log($"Current armor / health: {Armor} / {Health}");

        // Если армор есть - урон идет по армору
        if (Armor > 0)
        {
            Armor = Math.Max(Armor - damage, 0);

            // Событие изменение армора
            OnHealthChanged?.Invoke(Armor, MaxArmor, Health, MaxHealth);

            return true;
        }

        // Иначе урон идет по хп
        Health = Math.Max(Health - damage, 0);

        // Событие изменение хп
        OnHealthChanged?.Invoke(Armor, MaxArmor, Health, MaxHealth);

        return true;
    }
}
