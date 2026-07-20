using UnityEngine;

public enum PlayerState
{
    Idle,
    Walk,
    Run,
    Collect
}

public class PlayerStateManager : MonoBehaviour
{
    public PlayerState CurrentState { get; private set; } = PlayerState.Idle;

    // Событие для оповещения об изменении состояния
    public event System.Action<PlayerState> OnStateChanged;

    private PlayerMovement movement;
    private PlayerAnimation playerAnimation; 

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        playerAnimation = GetComponent<PlayerAnimation>(); 
    }

    public void ChangeState(PlayerState newState)
    {
        if (newState == CurrentState) return;

        CurrentState = newState;
        OnStateChanged?.Invoke(newState);

        // Обновляем анимацию
        UpdateAnimation();

        Debug.Log($"State changed to: {CurrentState}");
    }

    private void UpdateAnimation()
    {
        if (playerAnimation == null) return; 

        switch (CurrentState)
        {
            case PlayerState.Idle:
                playerAnimation.SetIdle();
                break;
            case PlayerState.Walk:
                playerAnimation.SetWalk();
                break;
            case PlayerState.Run:
                playerAnimation.SetRun();
                break;
            case PlayerState.Collect:
                playerAnimation.SetCollect();
                break;
        }
    }

    public bool IsMoving()
    {
        return CurrentState == PlayerState.Walk || CurrentState == PlayerState.Run;
    }

    public bool IsCollecting()
    {
        return CurrentState == PlayerState.Collect;
    }
}