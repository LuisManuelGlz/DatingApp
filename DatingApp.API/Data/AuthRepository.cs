using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;

        public AuthRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<User> Login(string username, string password)
        {
            // obtenemos el usuario o null
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);

            if (user == null)
                return null;

            // verificamos si la contraseña coincide
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            return user;
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            // obtenemos el HMAC con la salt
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                // como ya introducimos la salt obtenemos el hash de la contraseña
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                for (int i = 0; i < computedHash.Length; i++)
                {
                    // verificamos cada byte
                    // si uno no coincide mandamos un false
                    if (computedHash[i] == passwordHash[i]) return false;
                }
            }
            return true;
        }

        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            // asignamos los bytes a user
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _context.AddAsync(user); // agregamos
            await _context.SaveChangesAsync(); // y guardamos

            return user;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            // creamos HMAC
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                // obtiene la llave en bytes
                passwordSalt = hmac.Key;
                
                // convierte la contraseña en bytes y la hashea
                // passwordHash termina siendo bytes
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public async Task<bool> UserExists(string username)
        {
            // verificamos si existe
            if (await _context.Users.AnyAsync(x => x.Username == username))
                return true;

            return false;
        }
    }
}