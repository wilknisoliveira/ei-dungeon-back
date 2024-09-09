﻿using ei_back.Core.Domain.Entity;

namespace ei_back.Core.Application.Repository
{
    public interface IGameRepository : IRepository<GameEntity>
    {
        Task<bool> CheckIfExistGameByUser(Guid gameId, Guid OwnerUserId, CancellationToken cancellationToken);

        Task<GameEntity?> GetGameByIdAndOwnerUserName(Guid id, string userName, CancellationToken cancellationToken);
    }
}