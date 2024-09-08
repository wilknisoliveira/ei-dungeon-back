using ei_back.Core.Domain.Entity;

namespace ei_back.Core.Application.Service.Play.Interfaces
{
    public interface IGeneratePlaysResumeService
    {
        Task<PlayEntity> Handler(List<PlayEntity> plays, GameEntity game, string initialAddicionalInfo, CancellationToken cancellationToken);
    }
}
