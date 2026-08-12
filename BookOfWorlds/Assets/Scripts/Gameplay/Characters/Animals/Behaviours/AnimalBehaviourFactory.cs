using UnityEngine;

public static class AnimalBehaviourFactory
{
    public static IResourceBehaviour Create(AnimalDataSO data, ProgressBarUI progressBar)
    {
        if (data == null)
        {
            Debug.LogError("AnimalBehaviourFactory: data is NULL!");
            return null;
        }

        if (progressBar == null)
        {
            Debug.LogError("AnimalBehaviourFactory: progressBar is NULL!");
            return null;
        }

        float cooldown = data.cooldownTime;

        switch (data.animalType)
        {
            case AnimalDataSO.AnimalType.Rabbit:
                return new RabbitBehaviour(progressBar, cooldown);

            case AnimalDataSO.AnimalType.Cow:
                return new CowBehaviour(progressBar, cooldown);

            default:
                return new CowBehaviour(progressBar, cooldown);
        }
    }
}