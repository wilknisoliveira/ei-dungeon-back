using ei_back.Core.Domain.Entity;

namespace ei_back.Core.Application.Service.Play.Interfaces
{
    public interface IGeneratePlaysResumeService
    {
        Task<Domain.Entity.Play> Handler(List<Domain.Entity.Play> plays, Domain.Entity.Game game, string initialAddicionalInfo, CancellationToken cancellationToken);
    }
}
