using DataModel.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Features.User
{
    public interface IUserContext
    {
        Task<DataModel.Models.User> GetUser(Guid userId);
        ICollection<UserStatusRef> GetUserStatuses();
    }
}
