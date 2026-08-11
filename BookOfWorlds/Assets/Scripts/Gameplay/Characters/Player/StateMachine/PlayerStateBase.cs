public abstract class PlayerStateBase
{
    public abstract void Enter(PlayerController player);
    public abstract void Update(PlayerController player);
    public abstract void Exit(PlayerController player);
}