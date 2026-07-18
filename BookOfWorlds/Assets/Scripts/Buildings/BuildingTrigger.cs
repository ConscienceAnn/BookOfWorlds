using UnityEngine;

public class BuildingTrigger : MonoBehaviour
{
    private BuildingController buildingController;

    private void Awake()
    {
        buildingController = GetComponentInParent<BuildingController>();
        Debug.Log($" BuildingTrigger.Awake(): buildingController = {(buildingController != null ? "НАЙДЕН" : "НЕ НАЙДЕН")}");
    }

    //  Публичный метод для PlayerCollector
    public BuildingController GetBuildingController()
    {
        return buildingController;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (buildingController != null && buildingController.IsRestored())
            {
                Debug.Log($" Здание уже восстановлено, триггер игнорируется");
                return;
            }

            buildingController?.OnPlayerEnter();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (buildingController != null && buildingController.IsRestored())
            {
                return;
            }

            buildingController?.OnPlayerExit();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}