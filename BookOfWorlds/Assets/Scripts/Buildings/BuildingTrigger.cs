using UnityEngine;

public class BuildingTrigger : MonoBehaviour
{
    private BuildingController buildingController;
    private PlayerUI playerUI;

    private void Awake()
    {
        buildingController = GetComponentInParent<BuildingController>();
        playerUI = FindObjectOfType<PlayerUI>();
        Debug.Log($"BuildingTrigger.Awake(): buildingController = {(buildingController != null ? "НАЙДЕН" : "НЕ НАЙДЕН")}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (buildingController != null && buildingController.IsRestored())
            {
                Debug.Log($"Здание уже восстановлено, триггер игнорируется");
                return;
            }

            buildingController?.OnPlayerEnter();

            if (playerUI != null)
            {
                playerUI.ShowBuildingPrompt(buildingController);
            }
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

            if (playerUI != null)
            {
                playerUI.HideBuildingPrompt();
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}