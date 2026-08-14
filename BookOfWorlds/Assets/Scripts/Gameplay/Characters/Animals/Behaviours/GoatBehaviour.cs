
using UnityEngine;

/// <summary>
/// Поведение козы.
/// Использует базовую логику с прогресс-баром.
/// </summary>
public class GoatBehaviour : AnimalBehaviourBase
{
    public GoatBehaviour(ProgressBarUI progressBar, float cooldownTime)
        : base(progressBar, cooldownTime)
    {
        // Всё наследуется от AnimalBehaviourBase
    }
}