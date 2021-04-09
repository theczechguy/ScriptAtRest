using System.Collections.Generic;
using System.Linq;
using ScriptAtRestServer.Helpers;
using ScriptAtRestServer.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ScriptAtRestServer.Services
{
    public interface IUserService
    {
        User Authenticate(string username, string password);
        IEnumerable<User> GetAll();
        Task<User> CreateAsync(User user, string password);
        Task DeleteAsync(int id);
        Task<User> GetByIdAsync(int Id);
    }

    public class UserService : IUserService
    {
        private SqLiteDataContext _context;
        public UserService(SqLiteDataContext Context) {
            _context = Context;
        }

        public User Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            var user = _context.Users.SingleOrDefault(x => x.Username == username);

            if (user == null)
            {
                return null;
            }

            if (!PasswordHashHelpers.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt)) {
                return null;
            }

            return user;
        }

        public async Task<User> GetByIdAsync(int Id) 
        {
            User user = await _context.Users.FindAsync(Id);
            if (user == null)
            {
                throw new AppException("User with requested id not found in database");
            }
            return user;
        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users;
        }

        public async Task<User> CreateAsync(User user, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new AppException("Password is required");

            if (await _context.Users.AnyAsync(x => x.Username == user.Username))
                throw new AppException(string.Format("Username : {0} is already taken" , user.Username));

            byte[] passwordHash, passwordSalt;
            PasswordHashHelpers.CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task DeleteAsync(int Id)
        {
            User user = await _context.Users.FindAsync(Id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new AppException("User with requested id not found");
            }
        }
    }
}