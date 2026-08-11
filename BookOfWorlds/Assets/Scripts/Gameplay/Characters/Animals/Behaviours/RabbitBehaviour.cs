using UnityEngine;

/// <summary>
/// Поведение кролика.
/// Использует базовую логику с прогресс-баром.
/// Движение управляется отдельным компонентом AnimalMover.
/// </summary>
public class RabbitBehaviour : AnimalBehaviourBase
{
    public RabbitBehaviour(ProgressBarUI progressBar, float cooldownTime)
        : base(progressBar, cooldownTime)
    {
        // Всё наследуется от AnimalBehaviourBase
    }
}