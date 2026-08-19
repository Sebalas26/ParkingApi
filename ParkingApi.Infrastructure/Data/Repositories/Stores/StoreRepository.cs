using ParkingApi.Domain.Interfaces.Repositories.Stores;
using ParkingApi.Domain.Models;
using ParkingApi.Infrastructure.Data;
using ParkingApi.Infrastructure.Data.Repositories.Base;

namespace ParkingApi.Infrastructure.Data.Repositories.Stores;

public class StoreRepository : BaseRepository<Store>, IStoreRepository
{
    public StoreRepository(DataContext context) : base(context) { }
}
