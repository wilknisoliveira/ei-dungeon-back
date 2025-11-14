namespace ei_back.Core.Application.Service.Play.Interfaces;

public interface IUpsertWorldInfoService
{
    Task<string> Handler(string playerInfo, CancellationToken cancellationToken);
    Task Handler(Guid gameId, List<Domain.Entity.Play> plays, CancellationToken cancellationToken);
}