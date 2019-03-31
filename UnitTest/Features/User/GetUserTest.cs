using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AutoFixture;
using Features.User;
using FluentAssertions;
using UnitTest.Common;
using Xunit;

namespace UnitTest.Features.User
{
    public class GetUserTest : TestBase
    {
        private readonly IUserContext _userContext;

        public GetUserTest()
        {
            _userContext = new UserContext(_db);
        }

        [Fact]
        public async Task ThrowValidationExceptionWhenIdIsEmpty()
        {
            var userId = Guid.Empty;

            await Assert.ThrowsAsync<ValidationException>(() => _userContext.GetUser(userId));
        }

        [Fact]
        public async Task GetUserWhenProvidingAValidId()
        {
            var userId = _fixture.Build<Guid>()
                .Create();

            _db.Users.Add(new DataModel.Models.User
            {
                Id = userId
            });
            _db.SaveChanges();

            var result = await _userContext.GetUser(userId);

            result.Should().NotBeNull();
        }
    }
}
