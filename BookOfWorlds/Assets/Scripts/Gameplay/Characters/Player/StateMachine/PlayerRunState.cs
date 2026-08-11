using UnityEngine;

public class PlayerRunState : PlayerStateBase
{
    public override void Enter(PlayerController player)
    {
        player.SetAnimation("IsRunning", true);
        Debug.Log(" Состояние: Run");
    }

    public override void Update(PlayerController player)
    {
        // Если игрок остановился - Idle
        if (!player.IsMoving)
        {
            player.StateMachine.ChangeState(new PlayerIdleState());
        }

        // Если игрок начинает сбор - Collect
        if (player.IsCollecting)
        {
            player.StateMachine.ChangeState(new PlayerCollectState());
        }
    }

    public override void Exit(PlayerController player)
    {
        // Ничего не делаем
    }
}