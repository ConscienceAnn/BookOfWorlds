using UnityEngine;

[CreateAssetMenu(fileName = "AnimalData", menuName = "Game/Animal Data")]
public class AnimalDataSO : ScriptableObject
{
    [Header("ќсновные параметры")]
    public string animalName;
    public ResourceDataSO resourceData; //  —сылка на существующий ResourceDataSO
    public int resourceAmount = 1;
    public float cooldownTime = 8f;
}