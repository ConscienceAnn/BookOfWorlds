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
                Debug.Log($"AnimalBehaviourFactory: создаём RabbitBehaviour для {data.animalName}");
                return new RabbitBehaviour(progressBar, cooldown);

            case AnimalDataSO.AnimalType.Cow:
                Debug.Log($"AnimalBehaviourFactory: создаём CowBehaviour для {data.animalName}");
                return new CowBehaviour(progressBar, cooldown);

            //case AnimalDataSO.AnimalType.Deer:
            //    Debug.Log($"AnimalBehaviourFactory: создаём DeerBehaviour для {data.animalName}");
            //    return new DeerBehaviour(progressBar, cooldown);

            default:
                Debug.Log($"AnimalBehaviourFactory: создаём CowBehaviour (default) для {data.animalName}");
                return new CowBehaviour(progressBar, cooldown);
        }
    }
}