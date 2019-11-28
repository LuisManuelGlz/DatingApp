using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    // http://localhost:5000/api/auth/
    [Route("api/[controller]")]
    [ApiController]

    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;

        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;

        }

        // http://localhost:5000/api/auth/register/
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            /*
                userFprRegisterDto se definen sus valores al hacer post
                userForRegisterDto tiene consigo username y password
            */

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

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            // nos aseguramos de que exista un usuario y que su username y contraseña coincidan
            var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);

            // si no coincide
            if (userFromRepo == null)
                return Unauthorized();

            // contrucción del token

            // 
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };

            // obtenemos la llave
            var key = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(_config.GetSection("AppSettings:Token").Value));

            // hacemos una firma
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // creamos un descriptor de token, es como un esqueleto
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims), // 
                Expires = DateTime.Now.AddDays(1), // expira en un día
                SigningCredentials = creds // pasamos la firma
            };

            // creamos un manejador de tokens
            // esto nos permite crear un token basado en el descriptor
            var tokenHandler = new JwtSecurityTokenHandler();

            // creamos el token final
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // enviamos el token al cliente
            return Ok(new {
                token = tokenHandler.WriteToken(token)
            });
        }
    }
}