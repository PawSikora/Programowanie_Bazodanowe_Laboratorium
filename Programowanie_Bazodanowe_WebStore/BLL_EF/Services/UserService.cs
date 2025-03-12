using BLL.DTOModels.UserDTOs;
using BLL.ServiceInterfaces;
using DAL;
using Microsoft.EntityFrameworkCore;

namespace BLL_EF.Services
{
    public class UserService : IUserService
    {
        private readonly WebstoreContext _context;
        private static string? _userSession = null;

        public UserService(WebstoreContext context)
        {
            _context = context;
        }

        public async Task<bool> Login(UserLoginRequestDTO userLoginRequestDTO)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Login == userLoginRequestDTO.Login && u.Password == userLoginRequestDTO.Password && u.IsActive);

            if (user != null)
            {
                _userSession = user.Login;
                return true;
            }

            return false;
        }

        public Task Logout()
        {
            _userSession = null;
            return Task.CompletedTask;
        }
    }
}