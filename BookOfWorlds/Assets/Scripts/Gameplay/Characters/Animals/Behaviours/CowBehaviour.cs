using UnityEngine;

/// <summary>
/// Поведение коровы.
/// Ничего не переопределяет — использует базовую логику с прогресс-баром.
/// </summary>
public class CowBehaviour : AnimalBehaviourBase
{
    public CowBehaviour(ProgressBarUI progressBar, float cooldownTime)
        : base(progressBar, cooldownTime)
    {
        // Всё наследуется от AnimalBehaviourBase
    }
}