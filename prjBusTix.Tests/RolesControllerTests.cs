using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using prjBusTix.Controllers;
using prjBusTix.Dto;
using prjBusTix.Model;

namespace prjBusTix.Tests
{
    public class RolesControllerTests
    {
        private RolesController CreateController(Mock<RoleManager<IdentityRole>> mockRoleManager = null, Mock<UserManager<ClApplicationUser>> mockUserManager = null)
        {
            var roleManager = mockRoleManager?.Object ?? new Mock<RoleManager<IdentityRole>>(Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null).Object;
            var userManager = mockUserManager?.Object ?? new Mock<UserManager<ClApplicationUser>>(Mock.Of<IUserStore<ClApplicationUser>>(), null, null, null, null, null, null, null, null).Object;
            var logger = new Mock<ILogger<RolesController>>().Object;
            return new RolesController(roleManager, userManager, logger);
        }

        [Fact]
        public async Task CreateRole_NullDto_ReturnsBadRequest()
        {
            var controller = CreateController();

            var result = await controller.CreateRole(null);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task AssignRole_NullDto_ReturnsBadRequest()
        {
            var controller = CreateController();

            var result = await controller.AssignRole(null);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateRole_WhenRoleExists_ReturnsBadRequest()
        {
            var mockRoleStore = new Mock<IRoleStore<IdentityRole>>();
            var mockRoleManager = new Mock<RoleManager<IdentityRole>>(mockRoleStore.Object, null, null, null, null, null, null, null, null);
            mockRoleManager.Setup(r => r.RoleExistsAsync("admin")).ReturnsAsync(true);

            var controller = CreateController(mockRoleManager, null);

            var dto = new CreateRoleDto { RoleName = "admin" };
            var result = await controller.CreateRole(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}

