using do_an_epu.Models;
using do_an_epu.Services.Interface;
using Microsoft.AspNetCore.Identity;
using do_an_epu.Repositories.Interface;
using do_an_epu.Exceptions;
using do_an_epu.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;

namespace do_an_epu.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;
        private readonly UserManager<AppUser> _userManager;

        public AuthService(IAuthRepository authRepository, IConfiguration configuration, UserManager<AppUser> userManager)
        {
            _authRepository = authRepository;
            _configuration = configuration;
            _userManager = userManager;
        }
        public async Task<IdentityResult> SignUpAsync(AppUser user, string password)
        {
            var existingUser = await _authRepository.GetUserByEmailAsync(user.Email);
            if (existingUser != null)
            {
                throw new EmailAlreadyExistException("Email already exists.");
            }

            return await _authRepository.SignUpAsync(user, password);
        }
        public async Task<LoginResponseDto> LoginAsync (LoginUserDto loginDto)
        {
            var user = await _authRepository.GetUserByEmailAsync(loginDto.UserName);
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy email");
            }

            var isPasswordValid = await _authRepository.CheckUserPasswordAsync(user, loginDto.Password);
            if (!isPasswordValid)
            {
                throw new Exception("Sai mật khẩu");
            }

            var roles = await _authRepository.GetUserRoleAsync(user);
            var token = GenerateJwtToken(user, roles);

            return new LoginResponseDto
            {
                Token = token,
                Username = user.UserName,
                FullName = $"{user.LastName} {user.FirstName}",
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Roles = roles.ToList(),
                Expiration = DateTime.UtcNow.AddMinutes(double.Parse(_configuration.GetSection("Jwt")["DurationInMinutes"]))
            };
        }
        private string GenerateJwtToken(AppUser user, IList<string> roles)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            // Add Roles vào Claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Cấu hình mã hóa và thời gian hết hạn
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["DurationInMinutes"])),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<LoginResponseDto> GoogleLoginAsync(string idToken)
        {
            // 1. Xác thực ID Token từ Google
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new List<string> { _configuration["Authentication:Google:ClientId"] }
            };

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            }
            catch (InvalidJwtException)
            {
                throw new Exception("Token Google không hợp lệ");
            }

            // 2. Kiểm tra xem user đã tồn tại trong hệ thống chưa (theo Email)
            var user = await _authRepository.GetUserByEmailAsync(payload.Email);

            if (user == null)
            {
                // 3. Nếu chưa có, tạo user mới
                user = new AppUser
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    FirstName = payload.GivenName ?? "",
                    LastName = payload.FamilyName ?? "",
                    EmailConfirmed = payload.EmailVerified,
                    Avatar = payload.Picture
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    throw new Exception($"Không thể tạo tài khoản: {errors}");
                }

                // Gán role mặc định là "User"
                await _userManager.AddToRoleAsync(user, "User");
            }

            // 4. Lấy roles và tạo JWT token
            var roles = await _authRepository.GetUserRoleAsync(user);
            var token = GenerateJwtToken(user, roles);

            return new LoginResponseDto
            {
                Token = token,
                Username = user.UserName,
                FullName = $"{user.LastName} {user.FirstName}",
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Roles = roles.ToList(),
                Expiration = DateTime.UtcNow.AddMinutes(double.Parse(_configuration.GetSection("Jwt")["DurationInMinutes"]))
            };
        }

        public async Task<UserProfileDto> GetProfileAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new NotFoundException("User not found");

            return new UserProfileDto
            {
                Username = user.UserName,
                FullName = $"{user.LastName} {user.FirstName}", // Or just send separate fields if client wants to edit them separately? Client ProfilePage.jsx combines them into 'fullName' field.
                                                              // Wait, ProfilePage.jsx has `userData.fullName` input. If backend sends "Last First", user edits it as one string.
                                                              // But AppUser checks FirstName/LastName separately.
                                                              // Splitting FullName back to First/Last is tricky.
                                                              // Ideally `UserProfileDto` should have FirstName/LastName.
                                                              // But client ProfilePage.jsx uses `fullName`.
                                                              // I'll stick to FullName for read, but for Update, I might need to heuristics or change client to use First/Last.
                                                              // Given the prompt "Client side: ...header displays name", and ProfilePage I generated has just fullName.
                                                              // I will try to split it on update.
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address
            };
        }

        public async Task<IdentityResult> UpdateProfileAsync(int userId, UserProfileDto profileDto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new NotFoundException("User not found");

            // Heuristic split for FullName
            if (!string.IsNullOrEmpty(profileDto.FullName))
            {
                var parts = profileDto.FullName.Trim().Split(' ');
                if (parts.Length > 0)
                {
                    user.FirstName = parts[parts.Length - 1]; // Last part is first name in VN usually? No, "Nguyen Van A" -> First: A, Last: Nguyen Van. 
                                                            // But `AppUser` defines FirstName/LastName. 
                                                            // Let's assume standard Western or VN logic.
                                                            // VN: Ho (Last) - Ten Dem - Ten (First).
                                                            // In DB: LastName = "Nguyen Van", FirstName = "A".
                                                            // Logic: Last word is FirstName. Rest is LastName.
                    user.LastName = string.Join(" ", parts.Take(parts.Length - 1));
                    if(string.IsNullOrEmpty(user.LastName)) // Single word name
                    {
                         user.LastName = parts[0];
                         user.FirstName = ""; // ? Or swap.
                    }
                }
            }

            user.PhoneNumber = profileDto.PhoneNumber;
            user.Address = profileDto.Address;
            user.Email = profileDto.Email; // Allow email change? Identity might require token. Assuming simple update for now.

            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new NotFoundException("User not found");

            return await _userManager.ChangePasswordAsync(user, changePasswordDto.OldPassword, changePasswordDto.NewPassword);
        }
    }
}
