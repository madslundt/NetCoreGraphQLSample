using System;
using System.Threading.Tasks;
using AutoFixture;
using UnitTest.Common;
using Xunit;

namespace UnitTest.Features.User
{
    public class GetUserTest : TestBase
    {
        [Fact]
        public async Task GetUser()
        {
            var user = _fixture.Build<DataModel.Models.User>()
                .Create();

            _db.Add(user);
            await _db.SaveChangesAsync();

            var query = @"
                query {
                  user {
                    find(userId: """ + user.Id + @""") {
                      id
                      firstName
                      lastName
                    }
                  }
                }
            ";

            var expected = $"{{user: {{ find: {SerializeObject(new { user.Id, user.FirstName, user.LastName })} }} }}";

            await AssertQuerySuccess(query, expected);
        }

        [Fact]
        public async Task GetErrorWhenGettingPasswordFromUser()
        {
            var user = _fixture.Build<DataModel.Models.User>()
                .Create();

            _db.Add(user);
            await _db.SaveChangesAsync();

            var query = @"
                query {
                  user {
                    find(userId: """ + user.Id + @""") {
                      id
                      password
                    }
                  }
                }
            ";

            string expected = null;

            await AssertQueryWithErrors(query, expected, 1);
        }
    }
}
