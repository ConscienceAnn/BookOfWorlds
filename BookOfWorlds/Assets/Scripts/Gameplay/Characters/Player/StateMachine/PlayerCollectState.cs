using UnityEngine;

public class PlayerCollectState : PlayerStateBase
{
    public override void Enter(PlayerController player)
    {
        player.SetAnimation("IsCollecting", true);
    }

    public override void Update(PlayerController player)
    {
        // Если сбор завершён — Idle или Run
        if (!player.IsCollecting)
        {
            if (player.IsMoving)
                player.StateMachine.ChangeState(new PlayerRunState());
            else
                player.StateMachine.ChangeState(new PlayerIdleState());
        }
    }

    public override void Exit(PlayerController player)
    {
        player.SetAnimation("IsCollecting", false);
    }
}