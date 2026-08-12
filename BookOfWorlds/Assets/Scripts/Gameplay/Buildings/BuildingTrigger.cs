using UnityEngine;

public class BuildingTrigger : MonoBehaviour
{
    private BuildingController buildingController;
    private PlayerUI playerUI;

    private void Awake()
    {
        buildingController = GetComponentInParent<BuildingController>();
        playerUI = FindObjectOfType<PlayerUI>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (buildingController != null && buildingController.IsRestored())
            {
                return;
            }

            buildingController?.OnPlayerEnter();

            if (playerUI != null)
            {
                playerUI.SetPlayerNearBuilding(true, buildingController);
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
                playerUI.SetPlayerNearBuilding(false);
                playerUI.HideBuildingPrompt();
            }
        }
    }
}