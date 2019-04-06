using DataModel;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using DataModel.Models;

namespace Features.User
{
    public class UserContext : IUserContext
    {
        private readonly DatabaseContext _db;

        public UserContext(DatabaseContext db)
        {
            _db = db;
        }

        public async Task<DataModel.Models.User> GetUser(Guid userId)
        {
            var query = from user in _db.Users
                        where user.Id == userId
                        select user;

            var result = await query.FirstOrDefaultAsync();

            return result;
        }

        public ICollection<UserStatusRef> GetUserStatuses()
        {
            var result = Enum.GetValues(typeof(UserStatus)).Cast<UserStatus>().Select(userStatus => new UserStatusRef(userStatus)).ToArray();

            return result;
        }
    }
}
