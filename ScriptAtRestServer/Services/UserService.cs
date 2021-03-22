using System.Collections.Generic;
using System.Linq;
using ScriptAtRestServer.Helpers;
using ScriptAtRestServer.Entities;

namespace ScriptAtRestServer.Services
{
    public interface IUserService
    {
        User Authenticate(string username, string password);
        IEnumerable<User> GetAll();
        User Create(User user, string password);
        void Delete(int id);
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

        public IEnumerable<User> GetAll()
        {
            return _context.Users;
        }

        public User Create(User user, string password)
        {
            // validation
            if (string.IsNullOrWhiteSpace(password))
                throw new AppException("Password is required");

            if (_context.Users.Any(x => x.Username == user.Username))
                throw new AppException("Username \"" + user.Username + "\" is already taken");

            byte[] passwordHash, passwordSalt;
            PasswordHashHelpers.CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _context.Users.Add(user);
            _context.SaveChanges();

            return user;
        }

        public void Delete(int Id)
        {
            User user = _context.Users.Find(Id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }
    }
}