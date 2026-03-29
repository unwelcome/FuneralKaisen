using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    // ---------------------------------------------- ПОЛЯ ----------------------------------------------
    [SerializeField] private int health = 100;
    [SerializeField] private float immortalTimeOnHit = 100;

    private bool isImmortal = false;
    private SpriteRenderer sprite;

    private void Awake()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
    }


    // Корутина для активации неуязвимости
    private System.Collections.IEnumerator EnableImmortality(float immortalityTime)
    {
        // Устанавливаем неуязвимость
        isImmortal = true;

        // Делаем персонажа слегка прозрачным
        Color color = sprite.color;
        color.a = 0.85f;
        sprite.color = color;

        // Добавить анимацию неуязвимости
        yield return new WaitForSeconds(immortalityTime);

        // Убираем неуязвимость
        isImmortal = false;

        // Убираем полупрозрачность
        color.a = 1f;
        sprite.color = color;
    }

    // ---------------------------------------------- ПУБЛИЧНЫЕ МЕТОДЫ ----------------------------------------------

    // Получение урона
    public bool GetDamage(int damage)
    {
        if (isImmortal) return false;

        health = Math.Max(health - damage, 0);
        Debug.Log($"Health: {health}");

        StartCoroutine(EnableImmortality(immortalTimeOnHit));
        return true;
    }
}