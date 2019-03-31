using System;
using System.Threading.Tasks;

namespace Features.User
{
    public interface IUserContext
    {
        Task<DataModel.Models.User> GetUser(Guid userId);
    }
}
