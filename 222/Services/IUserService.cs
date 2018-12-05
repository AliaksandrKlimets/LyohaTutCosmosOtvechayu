using _222.EF;
using _222.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _222.Services
{
    public interface IUserService
{
         Task<User> FindOnLoginAsync(string email, string passwordString);
         Task CreateAsync(RegisterViewModel model);
        Task<bool> IsEmailExistAsync(string email);
}
}
