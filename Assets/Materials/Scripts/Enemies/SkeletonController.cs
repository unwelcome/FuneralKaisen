using UnityEngine;

public class SkeletonController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int health = 100;

    private SkeletonAnimationStates State;
    private enum SkeletonAnimationStates
    {
        idle,
        walk,
        attack1,
        attack2,
        hurt,
        death
    }

    private void Update()
    {
        if (health > 0) State = SkeletonAnimationStates.idle;
        else State = SkeletonAnimationStates.death;
    }
}
