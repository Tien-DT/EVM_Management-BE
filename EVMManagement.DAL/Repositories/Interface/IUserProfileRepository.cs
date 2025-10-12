using System;
using System.Threading.Tasks;
using EVMManagement.DAL.Models.Entities;
using EVMManagement.DAL.Repositories.Interface;

namespace EVMManagement.DAL.Repositories.Interface
{
    public interface IUserProfileRepository : IGenericRepository<UserProfile>
    {
        // Add entity-specific methods if needed in future
    }
}
