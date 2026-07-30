using UnityEngine;

public class AnimalBehaviourFactory
{
    public static IResourceBehaviour Create(AnimalDataSO data, ProgressBarUI progressBar)
    {
        if (data == null || progressBar == null)
            return null;

        float cooldown = data.cooldownTime;

        switch (data.animalType)
        {
            case AnimalDataSO.AnimalType.Rabbit:
                return new RabbitBehaviour(progressBar, cooldown);

            case AnimalDataSO.AnimalType.Cow:
            default:
                return new CowBehaviour(progressBar, cooldown);
        }
    }
}