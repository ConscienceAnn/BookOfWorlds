using UnityEngine;
using Zenject;

public class BuildingTrigger : MonoBehaviour
{
    private BuildingController buildingController;
    [Inject] private PlayerUIMediator playerUIMediator;

    private void Awake()
    {
        buildingController = GetComponentInParent<BuildingController>();
        
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

            if (playerUIMediator != null)
            {
                playerUIMediator.SetPlayerNearBuilding(true, buildingController);
                playerUIMediator.ShowBuildingPrompt(buildingController);
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

            if (playerUIMediator != null)
            {
                playerUIMediator.SetPlayerNearBuilding(false);
                playerUIMediator.HideBuildingPrompt();
            }
        }
    }
}