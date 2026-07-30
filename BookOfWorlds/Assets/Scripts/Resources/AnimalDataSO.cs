using UnityEngine;

[CreateAssetMenu(fileName = "AnimalData", menuName = "ScriptableObjects/AnimalData")]
public class AnimalDataSO : ScriptableObject
{
    public string animalName;
    public ResourceDataSO resourceData;
    public int resourceAmount = 1;
    public float cooldownTime = 8f;

    [Header("Movement Settings (הכÿ חאיצא)")]
    public bool canMove = false;
    public float moveSpeed = 3f;
    public float moveRadius = 5f;
    public float idleTimeMin = 2f;
    public float idleTimeMax = 5f;

    
    public enum AnimalType { Cow, Rabbit }
    public AnimalType animalType;
}