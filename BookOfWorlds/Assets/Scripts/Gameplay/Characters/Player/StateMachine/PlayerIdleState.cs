using UnityEngine;

public class PlayerIdleState : PlayerStateBase
{
    public override void Enter(PlayerController player)
    {
        player.SetAnimation("IsRunning", false);
    }

    public override void Update(PlayerController player)
    {
        // Если игрок двигается - Run
        if (player.IsMoving)
        {
            player.StateMachine.ChangeState(new PlayerRunState());
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