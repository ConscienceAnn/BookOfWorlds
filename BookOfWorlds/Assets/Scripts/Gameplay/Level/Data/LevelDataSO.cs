using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelData", menuName = "Levels/Level Data")]
public class LevelDataSO : ScriptableObject
{
    [Header("Основная информация")]
    public string levelName = "Новый уровень";
    public int levelIndex = 0;

    [Header("Стартовые условия")]
    public int startCoins = 0;
    public Vector3 playerStartPosition = Vector3.zero;

    [Header("Ресурсы")]
    public GameObject collectableObjectsPrefab; 

    [Header("Здания")]
    public GameObject buildingsPrefab;

    [Header("Животные")]
    public GameObject animalsPrefab;

    [Header("Зона продажи")]
    public GameObject sellZoneData;

    [Header("Условия завершения")]
    //public int coinsToUnlockNextLevel = 100;
    public bool showCompleteUI = true;

    [Header("Визуал завершения уровня")]
    public GameObject levelCompleteEffectPrefab;
}



