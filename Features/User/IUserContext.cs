using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Features.User
{
    public interface IUserContext
    {
        Task<DataModel.Models.User> GetUser(Guid userId);
        ICollection<(int Id, string Name)> GetUserStatuses();
        Task<DataModel.Models.User> CreateUser((string firstName, string lastName) input);
    }
}
