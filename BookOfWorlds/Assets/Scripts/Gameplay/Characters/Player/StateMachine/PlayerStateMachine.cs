using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    private PlayerController player;
    private PlayerStateBase currentState;

    public PlayerStateBase CurrentState => currentState;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    private void Start()
    {
        ChangeState(new PlayerIdleState());
    }

    private void Update()
    {
        currentState?.Update(player);
    }

    public void ChangeState(PlayerStateBase newState)
    {
        currentState?.Exit(player);
        currentState = newState;
        currentState?.Enter(player);
    }
}