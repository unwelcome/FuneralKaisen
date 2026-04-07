using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Общий класс эффектов
public abstract class EntityEffect
{
    public float Duration; // Длительность эффекта
    protected float Timer; // Таймер эффекта

    // False - каждый эффект работает независимо
    // True - один эффект со стакающимися характеристиками
    public bool IsStackable;

    public virtual void Apply(GameObject target) { }
    public virtual void AddStack() { }
    public virtual void Tick(GameObject target) { }
    public virtual void Remove(GameObject target) { }

    public bool Update(GameObject target)
    {
        Timer += Time.deltaTime;

        Tick(target);

        // эффект закончился
        if (Timer >= Duration)
        {
            Remove(target);
            return true;
        }

        return false;
    }
}

// Яд
public class PoisonEffect : EntityEffect
{
    private int Damage;
    private float tickCounter = 0;

    public PoisonEffect(int damage, float duration)
    {
        Damage = damage;
        Duration = duration;
        Timer = 0;
        IsStackable = false;
    }

    public override void Tick(GameObject target)
    {
        tickCounter += Time.deltaTime;

        // Если прошла секунда с прошлого получения урона
        if (tickCounter >= 1f)
        {
            target.GetComponent<HealthController>().GetDamage(Damage);
            tickCounter = 0f;
        }
    }
}

// Огонь
public class FireEffect : EntityEffect
{
    private int Damage;
    private float tickCounter = 0;

    public FireEffect(int damage, float duration)
    {
        Damage = damage;
        Duration = duration;
        Timer = 0;
        IsStackable = false;
    }

    public override void Apply(GameObject target)
    {
        Debug.Log("Target get fire!");
    }

    public override void Remove(GameObject target)
    {
        Debug.Log("Fire is gone!");
    }

    public override void Tick(GameObject target)
    {
        tickCounter += Time.deltaTime;

        // Если прошла секунда с прошлого получения урона
        if (tickCounter >= 1f)
        {
            target.GetComponent<HealthController>().GetDamage(Damage);
            tickCounter = 0f;
        }
    }
}

// Заморозка
public class FreezeEffect : EntityEffect
{
    private int stack = 0;
    private float damageMultiplier = 0.15f;
    private float velocityMultiplier = 0.15f;

    private MovementController targetMovementController;
    private AttackController targetAttackController;

    public FreezeEffect(float duration)
    {
        Duration = duration;
        IsStackable = true;
    }

    public override void Apply(GameObject target)
    {
        targetMovementController = target.GetComponent<MovementController>();
        targetAttackController = target.GetComponent<AttackController>();
        AddStack();
    }

    // Добавление эффекта
    public override void AddStack()
    {
        stack++;
        Timer = 0; // сбрасываем таймер

        // Обновляем коэф. заморозки для скорости
        float slow = Math.Min(stack * velocityMultiplier, 1f);
        if (targetMovementController != null) targetMovementController.VelocityMultiplier = 1f - slow;
        if (targetAttackController != null) targetAttackController.DamageMultiplier = 1f - slow;
    }

    // Отключаем эффект заморозки
    public override void Remove(GameObject target)
    {
        if (targetMovementController != null) targetMovementController.VelocityMultiplier = 1f;
        if (targetAttackController != null) targetAttackController.DamageMultiplier = 1f;
    }
}

// Стан
public class StunEffect : EntityEffect
{
    private MovementController targetMovementController;
    private AttackController targetAttackController;

    public StunEffect(float duration) 
    {
        Duration = duration;
        IsStackable = true;
    }

    public override void Apply(GameObject target)
    {
        // Отключаем движение
        targetMovementController = target.GetComponent<MovementController>();
        if (targetMovementController != null) targetMovementController.enabled = false;

        // Отключаем атаку
        targetAttackController = target.GetComponent<AttackController>();
        if (targetAttackController != null) targetAttackController.enabled = false; 
    }

    // Стак эффекта - сбрасываем таймер
    public override void AddStack()
    {
        Timer = 0;
    }

    public override void Remove(GameObject target)
    {
        if (targetMovementController != null) targetMovementController.enabled = true;
        if (targetAttackController != null) targetAttackController.enabled = true;
    }
}

// Неуязвимость
public class ImmortalityEffect : EntityEffect
{
    private HealthController targetHealthController;

    public ImmortalityEffect(float duration)
    {
        Duration = duration;
        IsStackable = true;
    }

    public override void Apply(GameObject target)
    {
        targetHealthController = target.GetComponent<HealthController>();
        if (targetHealthController != null) targetHealthController.IsDamageBlocked = true;
    }

    // Стак эффекта - сбрасываем таймер
    public override void AddStack()
    {
        Timer = 0;
    }

    public override void Remove(GameObject target)
    {
        if (targetHealthController != null) targetHealthController.IsDamageBlocked = false;
    }
}

public class EffectController : MonoBehaviour
{
    private List<EntityEffect> effects = new List<EntityEffect>();

    public void AddEffect(EntityEffect effect)
    {
        // Если эффект не стакающийся - просто добавляем эффект
        if (!effect.IsStackable)
        {
            effect.Apply(gameObject);
            effects.Add(effect);
            return;
        }

        // Если эффект стакающийся, сначала ищем его в списке эффектов
        foreach (EntityEffect curEffect in effects)
        {
            // Обновляем стак эффекта и выходим 
            if (curEffect.GetType() == effect.GetType()) {
                curEffect.AddStack();
                return;
            }
        }

        // Если не нашли эффект - добавляем в список
        effect.Apply(gameObject);
        effects.Add(effect);
    }

    private void Update()
    {
        var toRemove = new List<EntityEffect>();

        foreach (EntityEffect effect in effects)
        {
            if (effect.Update(gameObject))
            {
                toRemove.Add(effect);
            }
        }

        foreach(EntityEffect curEffect in toRemove)
        {
            effects.Remove(curEffect);
        }
    }
    
}
