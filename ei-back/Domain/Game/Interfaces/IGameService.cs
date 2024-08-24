namespace ei_back.Domain.Game.Interfaces
{
    public interface IGameService
    {
        Task<GameEntity> CreateAsync(GameEntity game);
    }
}
