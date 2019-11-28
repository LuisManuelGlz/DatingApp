using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    // http://localhost:5000/api/auth/
    [Route("api/[controller]")]
    [ApiController]

    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;

        public AuthController(IAuthRepository repo)
        {
            _repo = repo;
        }

        // http://localhost:5000/api/auth/register/
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            /*
                userFprRegisterDto se definen sus valores al hacer post
                userForRegisterDto tiene consigo username y password
            */

            // validate request

            // pasamos el nombre de usuario a lower
            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

            // verificamos si existe
            if (await _repo.UserExists(userForRegisterDto.Username))
                return BadRequest("User already exists");

            // seteamos un nuevo usuario solamente con su username
            var userToCreate = new User
            {
                Username = userForRegisterDto.Username
            };

            // registramos un nuevo usuario
            // Register recibe el usuario para crear (userToCreate) y la contraseña introducida
            var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

            return StatusCode(201);
        }
    }
}