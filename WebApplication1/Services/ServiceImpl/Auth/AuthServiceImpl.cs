using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService.Auth;
using WebApplication1.Utils.Settings;

namespace WebApplication1.Services.ServiceImpl.Auth
{
    public class AuthServiceImpl : IAuthService
    {
        private readonly IUserRepository _repository;

        private readonly JwtSettings _jwtSettings;

        //logger: for auditing
        private readonly ILogger<AuthServiceImpl> _logger;

        // Constructor
        public AuthServiceImpl(IUserRepository repository, IOptions<JwtSettings> jwtOptions, ILogger<AuthServiceImpl> logger)
        {
            _repository = repository;
            _jwtSettings = jwtOptions.Value;
            _logger = logger;
        }

        public async Task<User> RegisterUserAsync(User user)
        {
            var dupliacte = await _repository.EmailExistsAsync(user.Email);
            if (dupliacte)
                throw new Exception($"User with email '{user.Email} already exists.");

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            await _repository.AddAsync(user);
            _logger.LogInformation("User created: Id={Id}, Email={Email}", user.UserID, user.Email);
            return user;
        }

        public async Task<(User user, string token)> LoginAsync(string email, string password)
        {
            var user = await _repository.GetByEmailAsync(email);

            if (user == null)
                throw new UnauthorizedAccessException("Invalid email or password");

            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
                throw new UnauthorizedAccessException("Invalid email or password");

            // Generate JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            return (user, jwtToken);
        }
    }
}
