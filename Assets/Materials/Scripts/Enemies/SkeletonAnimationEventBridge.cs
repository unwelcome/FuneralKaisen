using UnityEngine;

// Кладётся на тот же объект, что и Animator (Square).
// Пробрасывает Animation Events к SkeletonController на родителе.
public class SkeletonAnimationEventBridge : MonoBehaviour
{
    private SkeletonController controller;

    private void Awake()
    {
        controller = GetComponentInParent<SkeletonController>();
    }

    public void SpawnHitbox()    => controller?.SpawnHitbox();
    public void OnAttackEnd()    => controller?.OnAttackEnd();
    public void OnHurtEnd()      => controller?.OnHurtEnd();
    public void OnDeathAnimEnd() => controller?.OnDeathAnimEnd();
}
