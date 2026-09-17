using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Domain.Entities;
using SmartMetroService.Storage.Sql;

namespace SmartMetroService.Storage.Repositories;

public class JourneyRepository : Repository<Journey>, IJourneyRepository
{
    public JourneyRepository(MyApplicationDbContext db) : base(db)
    {
    }
}
