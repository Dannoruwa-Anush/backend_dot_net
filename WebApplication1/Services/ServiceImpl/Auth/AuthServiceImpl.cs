using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService.Auth;
using WebApplication1.UOW.IUOW;
using WebApplication1.Utils.Settings;

namespace WebApplication1.Services.ServiceImpl.Auth
{
    public class AuthServiceImpl : IAuthService
    {
        private readonly IUserRepository _repository;
        private readonly IAppUnitOfWork _unitOfWork;

        private readonly JwtSettings _jwtSettings;

        //logger: for auditing
        private readonly ILogger<AuthServiceImpl> _logger;

        // Constructor
        public AuthServiceImpl(IUserRepository repository, IAppUnitOfWork unitOfWork, IOptions<JwtSettings> jwtOptions, ILogger<AuthServiceImpl> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _jwtSettings = jwtOptions.Value;
            _logger = logger;
        }

        //Register
        public async Task<User> RegisterUserAsync(User user)
        {
            if (await _repository.EmailExistsAsync(user.Email))
                throw new Exception($"User with email '{user.Email}' already exists.");

            // Trim
            user.Email = user.Email.Trim();
            user.Password = user.Password.Trim();

            // Hash only if not already hashed
            if (!user.Password.StartsWith("$2a$") && !user.Password.StartsWith("$2b$"))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            }

            await _repository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User created: Id={Id}, Email={Email}", user.UserID, user.Email);
            return user;
        }

        //Login
        public async Task<(User user, string token)> LoginAsync(string email, string password)
        {
            var user = await _repository.GetByEmailAsync(email.Trim());
            if (user == null)
                throw new UnauthorizedAccessException("Invalid email or password");

            // Verify the password
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password.Trim(), user.Password);
            if (!isPasswordValid)
                throw new UnauthorizedAccessException("Invalid email or password");

            // Generate JWT token
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
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes),
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
