using prjBusTix.Dto.Auth;
using prjBusTix.Dto.Users;
using prjBusTix.Model;
using prjBusTix.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace prjBusTix.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    //api/account
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ClApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<ClApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        // api/account/register
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<string>> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ClApplicationUser
            {
                Email = registerDto.EmailAddress,
                NombreCompleto = registerDto.NombreCompleto,
                UserName = registerDto.EmailAddress,
                // Campos de Identity
                TipoDocumento = registerDto.TipoDocumento,
                NumeroDocumento = registerDto.NumeroDocumento,
                Estatus = 1,  // Activo por defecto
                FechaRegistro = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Filtrar roles válidos (excluir null, vacíos, "NULL")
            var validRoles = registerDto.Roles?
                .Where(r => !string.IsNullOrWhiteSpace(r) && !r.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (validRoles == null || validRoles.Count == 0)
            {
                // Si no hay roles válidos, asignar rol "User" por defecto
                await _userManager.AddToRoleAsync(user, "User");
            }
            else
            {
                // Validar que los roles existan antes de asignarlos
                var roleManager = HttpContext.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();
                foreach (var role in validRoles)
                {
                    var roleExists = await roleManager.RoleExistsAsync(role);
                    if (roleExists)
                    {
                        await _userManager.AddToRoleAsync(user, role);
                    }
                    else
                    {
                        // Si el rol no existe, eliminar el usuario creado y devolver error
                        await _userManager.DeleteAsync(user);
                        return BadRequest(new AuthResponseDto
                        {
                            IsSuccess = false,
                            Message = $"El rol '{role}' no existe en el sistema. Roles válidos: Admin, User, Manager, Operator"
                        });
                    }
                }
            }

            // ✅ ENVIAR EMAIL DE CONFIRMACIÓN AUTOMÁTICAMENTE
            try
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = $"http://localhost:4200/confirm-email?email={user.Email}&token={WebUtility.UrlEncode(token)}";

                var mailSettings = _configuration.GetSection("MailSettings");
                var senderEmail = mailSettings["SenderEmail"];
                var senderName = mailSettings["SenderName"];
                var smtpServer = mailSettings["Server"];
                var smtpPortString = mailSettings["Port"];
                var smtpPort = smtpPortString != null ? int.Parse(smtpPortString) : 587;
                var smtpUser = mailSettings["UserName"];
                var smtpPass = mailSettings["Password"];

                var message = new MimeKit.MimeMessage();
                message.From.Add(new MimeKit.MailboxAddress(senderName, senderEmail));
                message.To.Add(new MimeKit.MailboxAddress(user.NombreCompleto, user.Email)); // Eliminado operador ?? redundante
                message.Subject = "Confirma tu email - BusTix";

                var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #007bff; padding: 20px; text-align: center;'>
                        <h1 style='color: white; margin: 0;'>🚌 BusTix</h1>
                    </div>
                    <div style='padding: 30px; background-color: #f8f9fa;'>
                        <h2 style='color: #333;'>¡Bienvenido {user.NombreCompleto ?? "Usuario"}!</h2>
                        <p style='color: #666; font-size: 16px; line-height: 1.6;'>
                            Tu cuenta ha sido creada exitosamente. Para completar tu registro,
                            por favor confirma tu dirección de email haciendo clic en el siguiente botón:
                        </p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{confirmationLink}' style='
                                display: inline-block;
                                padding: 15px 30px;
                                background-color: #28a745;
                                color: white;
                                text-decoration: none;
                                border-radius: 5px;
                                font-weight: bold;
                                font-size: 16px;
                            '>✅ Confirmar Email</a>
                        </div>
                        <p style='color: #999; font-size: 14px;'>
                            Este link expira en 24 horas.
                        </p>
                    </div>
                </div>";

                message.Body = new MimeKit.TextPart("html") { Text = htmlBody };

                using var client = new MailKit.Net.Smtp.SmtpClient();
                await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpUser, smtpPass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                // Log pero no falla el registro
                Console.WriteLine($"Error al enviar email de confirmación: {ex.Message}");
            }

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Cuenta creada exitosamente. Por favor revisa tu email para confirmar tu cuenta."
            });
        }

        //api/account/login
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
            {
                return Unauthorized(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Usuario no encontrado con este email"
                });
            }

            // ✅ VERIFICAR SI EL EMAIL ESTÁ CONFIRMADO
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return Unauthorized(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Por favor confirma tu email antes de iniciar sesión. Revisa tu bandeja de entrada."
                });
            }

            // VERIFICAR SI LA CUENTA ESTÁ BLOQUEADA
            if (await _userManager.IsLockedOutAsync(user))
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                var remainingTime = lockoutEnd.HasValue 
                    ? (lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes 
                    : 0;

                return Unauthorized(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = $"Tu cuenta está bloqueada por seguridad. Intenta nuevamente en {Math.Ceiling(remainingTime)} minutos."
                });
            }

            // VERIFICAR LA CONTRASEÑA
            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!result)
            {
                // REGISTRAR INTENTO FALLIDO - Esto incrementa AccessFailedCount automáticamente
                await _userManager.AccessFailedAsync(user);
                
                // Obtener intentos restantes
                var failedAttempts = await _userManager.GetAccessFailedCountAsync(user);
                var maxAttempts = _userManager.Options.Lockout.MaxFailedAccessAttempts;
                var remainingAttempts = maxAttempts - failedAttempts;

                // Si se bloqueó la cuenta
                if (await _userManager.IsLockedOutAsync(user))
                {
                    var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                    var lockoutMinutes = lockoutEnd.HasValue 
                        ? (lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes 
                        : 0;

                    return Unauthorized(new AuthResponseDto
                    {
                        IsSuccess = false,
                        Message = $"Demasiados intentos fallidos. Tu cuenta ha sido bloqueada por {Math.Ceiling(lockoutMinutes)} minutos."
                    });
                }

                return Unauthorized(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = $"Contraseña incorrecta. Te quedan {remainingAttempts} intentos antes de que tu cuenta sea bloqueada."
                });
            }

            // LOGIN EXITOSO - Resetear contador de intentos fallidos
            await _userManager.ResetAccessFailedCountAsync(user);
            
            // Actualizar última conexión
            user.UltimaConexion = DateTime.Now;
            
            var token = await GenerateToken(user);
            var refreshToken = GenerateRefreshToken();
            _ = int.TryParse(_configuration.GetSection("JWTSettings").GetSection("RefreshTokenValidityIn").Value!, out int refreshTokenValidityIn);
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(refreshTokenValidityIn);
            await _userManager.UpdateAsync(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                IsSuccess = true,
                Message = "Login exitoso",
                RefreshToken = refreshToken
            });
        }


        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        // api/account/forgot-password
        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<ActionResult<AuthResponseDto>> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);

            if (user is null)
            {
                return Ok(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "User does not exist with this email"
                });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"http://localhost:4200/reset-password?email={user.Email}&token={WebUtility.UrlEncode(token)}";

            // Leer configuración de correo
            var mailSettings = _configuration.GetSection("MailSettings");
            var email = mailSettings["SenderEmail"];
            var displayName = mailSettings["SenderName"];
            var smtpServer = mailSettings["Server"];
            var smtpPortString = mailSettings["Port"];
            var smtpPort = smtpPortString != null ? int.Parse(smtpPortString) : 587;
            var smtpUser = mailSettings["UserName"];
            var smtpPass = mailSettings["Password"];

            // Enviar correo con HTML
            var message = new MimeKit.MimeMessage();
            message.From.Add(new MimeKit.MailboxAddress(displayName, email));
            message.To.Add(new MimeKit.MailboxAddress(user.NombreCompleto, user.Email)); // Eliminado operador ?? redundante
            message.Subject = "Recuperación de contraseña";

            var htmlBody = $@"
        <div style='font-family: Arial, sans-serif; color: #333;'>
            <h2>Hola {user.NombreCompleto ?? "Usuario"},</h2>
            <p>Recibimos una solicitud para restablecer tu contraseña.</p>
            <p>
                <a href='{resetLink}' style='
                    display: inline-block;
                    padding: 10px 20px;
                    background-color: #4CAF50;
                    color: white;
                    text-decoration: none;
                    border-radius: 5px;
                    font-weight: bold;
                '>Restablecer contraseña</a>
            </p>
            <p>Si no solicitaste este cambio, puedes ignorar este correo.</p>
            <br>
            <p style='font-size:12px;color:#888;'>ReptiTrack &copy; {DateTime.Now.Year}</p>
        </div>";

            message.Body = new MimeKit.TextPart("html") { Text = htmlBody };

            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpUser, smtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Se ha enviado un correo con las instrucciones para restablecer la contraseña."
            });
        }




        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto change)
        {
            var user = await _userManager.FindByEmailAsync(change.Email);
            if (user is null)
            {
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "User does not exist with this email"
                });
            }

            var result = await _userManager.ChangePasswordAsync(user, change.CurrentPassword, change.NewPassword);
            if (result.Succeeded)
            {
                return Ok(new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "Password changed successfully"
                });
            }

            return BadRequest(new AuthResponseDto
            {
                IsSuccess = false,
                Message = result.Errors.FirstOrDefault()?.Description ?? "Error changing password"
            });
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);

            if (user is null)
            {
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "User does not exist with this email"
                });
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
            if (result.Succeeded)
            {
                return Ok(new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "Password reset Successfully"
                });
            }

            return BadRequest(new AuthResponseDto
            {
                IsSuccess = false,
                Message = result.Errors.FirstOrDefault()?.Description ?? "Error resetting password"
            });
        }

        private async Task<string> GenerateToken(ClApplicationUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSection = _configuration.GetSection("JWTSetting");
            var key = Encoding.ASCII.GetBytes(jwtSection["securityKey"]!);
            var roles = await _userManager.GetRolesAsync(user);

            // Obtener permisos basados en roles
            var permissions = new HashSet<string>();
            foreach (var role in roles)
            {
                if (AppPermissions.RolePermissions.ContainsKey(role))
                {
                    foreach (var permission in AppPermissions.RolePermissions[role])
                    {
                        permissions.Add(permission);
                    }
                }
            }

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new(JwtRegisteredClaimNames.Name, user.NombreCompleto), // Eliminado operador ?? redundante
                new("estatus", user.Estatus.ToString()),
                new("emailVerified", user.EmailConfirmed.ToString()),
                new(JwtRegisteredClaimNames.Aud, jwtSection["ValidAudience"]!),
                new(JwtRegisteredClaimNames.Iss, jwtSection["ValidIssuer"]!),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            // Agregar roles
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Agregar permisos
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
            }


            var expireInMinutes = int.Parse(jwtSection["expireInMinutes"] ?? "60");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expireInMinutes),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        //api/account/detail
        [Authorize]
        [HttpGet("detail")]
        public async Task<ActionResult<UserDetailDto>> GetUserDetail()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(currentUserId!);

            if (user == null)
            {
                return NotFound(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "User not found"
                });
            }

            // Obtener el nombre del estatus desde la BD
            var dbContext = HttpContext.RequestServices.GetRequiredService<Data.AppDbContext>();
            var estatusNombre = await dbContext.EstatusGenerales
                .Where(e => e.Id_Estatus == user.Estatus)
                .Select(e => e.Nombre)
                .FirstOrDefaultAsync();

            return Ok(new UserDetailDto
            {
                Id = user.Id,
                Email = user.Email,
                NombreCompleto = user.NombreCompleto,
                Roles = (await _userManager.GetRolesAsync(user)).ToArray(),
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                AccessFailedCount = user.AccessFailedCount,
                // Campos adicionales
                Estatus = user.Estatus,
                EstatusNombre = estatusNombre,
                FechaRegistro = user.FechaRegistro,
                TipoDocumento = user.TipoDocumento,
                NumeroDocumento = user.NumeroDocumento,
                EmailConfirmed = user.EmailConfirmed
            });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserDetailDto>>> GetUsers()
        {
            var dbContext = HttpContext.RequestServices.GetRequiredService<Data.AppDbContext>();
            var users = await _userManager.Users.ToListAsync();
            var userDtos = new List<UserDetailDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var estatusNombre = await dbContext.EstatusGenerales
                    .Where(e => e.Id_Estatus == user.Estatus)
                    .Select(e => e.Nombre)
                    .FirstOrDefaultAsync();

                userDtos.Add(new UserDetailDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    NombreCompleto = user.NombreCompleto,
                    Roles = roles.ToArray(),
                    PhoneNumber = user.PhoneNumber,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    AccessFailedCount = user.AccessFailedCount,
                    // Campos adicionales
                    Estatus = user.Estatus,
                    EstatusNombre = estatusNombre,
                    FechaRegistro = user.FechaRegistro,
                    TipoDocumento = user.TipoDocumento,
                    NumeroDocumento = user.NumeroDocumento,
                    EmailConfirmed = user.EmailConfirmed
                });
            }

            return Ok(userDtos);
        }

        // api/account/logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(currentUserId!);

            if (user != null)
            {
                // Invalidar refresh token
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = DateTime.MinValue;
                await _userManager.UpdateAsync(user);
            }

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Sesión cerrada exitosamente"
            });
        }

        // api/account/refresh-token
        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(refreshTokenDto.Email);

            if (user == null)
            {
                return Unauthorized(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Usuario no encontrado"
                });
            }

            // Verificar que el refresh token sea válido y no haya expirado
            if (user.RefreshToken != refreshTokenDto.RefreshToken || 
                user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return Unauthorized(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Token de actualización inválido o expirado. Por favor inicia sesión nuevamente."
                });
            }

            // Generar nuevo access token y refresh token
            var newAccessToken = await GenerateToken(user);
            var newRefreshToken = GenerateRefreshToken();

            _ = int.TryParse(_configuration.GetSection("JWTSettings").GetSection("RefreshTokenValidityIn").Value!, out int refreshTokenValidityIn);
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(refreshTokenValidityIn);
            await _userManager.UpdateAsync(user);

            return Ok(new AuthResponseDto
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                IsSuccess = true,
                Message = "Token actualizado exitosamente"
            });
        }

        // api/account/revoke-token/{userId}
        [HttpPost("revoke-token/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RevokeToken(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Usuario no encontrado"
                });
            }

            // Revocar refresh token del usuario
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = DateTime.MinValue;
            await _userManager.UpdateAsync(user);

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Message = $"Token del usuario {user.Email} revocado exitosamente. El usuario deberá iniciar sesión nuevamente."
            });
        }

        // api/account/permissions
        [HttpGet("permissions")]
        public IActionResult GetMyPermissions()
        {
            var permissions = User.Claims
                .Where(c => c.Type == "permission")
                .Select(c => c.Value)
                .ToList();

            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            return Ok(new
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                email = User.FindFirstValue(ClaimTypes.Email),
                name = User.FindFirstValue(ClaimTypes.Name),
                roles,
                permissions,
                sessionId = User.FindFirstValue("sessionId"),
                accountStatus = User.FindFirstValue("accountStatus")
            });
        }


        // ========== NUEVOS ENDPOINTS PARA GESTIÓN MEJORADA DE USUARIOS ==========

        // api/account/update-profile
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDto updateDto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(currentUserId!);

            if (user == null)
            {
                return NotFound(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "User not found"
                });
            }

            // Actualizar campos
            if (!string.IsNullOrWhiteSpace(updateDto.NombreCompleto))
                user.NombreCompleto = updateDto.NombreCompleto;

            if (!string.IsNullOrWhiteSpace(updateDto.PhoneNumber))
                user.PhoneNumber = updateDto.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(updateDto.TipoDocumento))
                user.TipoDocumento = updateDto.TipoDocumento;

            if (!string.IsNullOrWhiteSpace(updateDto.NumeroDocumento))
                user.NumeroDocumento = updateDto.NumeroDocumento;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Error updating profile"
                });
            }

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Profile updated successfully"
            });
        }

        // api/account/{userId}/status (Solo Admin)
        [HttpPut("{userId}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserStatus(string userId, [FromBody] UpdateUserStatusDto statusDto)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "User not found"
                });
            }

            // Validar que el estatus exista
            var dbContext = HttpContext.RequestServices.GetRequiredService<Data.AppDbContext>();
            var estatusExists = await dbContext.EstatusGenerales
                .AnyAsync(e => e.Id_Estatus == statusDto.Estatus);

            if (!estatusExists)
            {
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Invalid status. Valid values: 1=Activo, 2=Inactivo, 3=Validado, 4=Pendiente, 5=Cancelado, 6=Suspendido, 7=Bloqueado"
                });
            }

            user.Estatus = statusDto.Estatus;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Error updating user status"
                });
            }

            var estatusNombre = await dbContext.EstatusGenerales
                .Where(e => e.Id_Estatus == statusDto.Estatus)
                .Select(e => e.Nombre)
                .FirstOrDefaultAsync();

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Message = $"User status updated to '{estatusNombre}' successfully"
            });
        }

        // api/account/statuses (Obtener catálogo de estatus)
        [HttpGet("statuses")]
        public async Task<IActionResult> GetStatuses()
        {
            var dbContext = HttpContext.RequestServices.GetRequiredService<Data.AppDbContext>();
            var statuses = await dbContext.EstatusGenerales
                .Select(e => new { e.Id_Estatus, e.Nombre })
                .ToListAsync();

            return Ok(statuses);
        }

        // api/account/users/by-status/{estatusId} (Obtener usuarios por estatus - Solo Admin)
        [HttpGet("users/by-status/{estatusId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserDetailDto>>> GetUsersByStatus(int estatusId)
        {
            var dbContext = HttpContext.RequestServices.GetRequiredService<Data.AppDbContext>();
            
            // Validar que el estatus exista
            var estatusExists = await dbContext.EstatusGenerales
                .AnyAsync(e => e.Id_Estatus == estatusId);

            if (!estatusExists)
            {
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Invalid status ID"
                });
            }

            var users = await _userManager.Users
                .Where(u => u.Estatus == estatusId)
                .ToListAsync();

            var userDtos = new List<UserDetailDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var estatusNombre = await dbContext.EstatusGenerales
                    .Where(e => e.Id_Estatus == user.Estatus)
                    .Select(e => e.Nombre)
                    .FirstOrDefaultAsync();

                userDtos.Add(new UserDetailDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    NombreCompleto = user.NombreCompleto,
                    Roles = roles.ToArray(),
                    PhoneNumber = user.PhoneNumber,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    AccessFailedCount = user.AccessFailedCount,
                    Estatus = user.Estatus,
                    EstatusNombre = estatusNombre,
                    FechaRegistro = user.FechaRegistro,
                    TipoDocumento = user.TipoDocumento,
                    NumeroDocumento = user.NumeroDocumento,
                    EmailConfirmed = user.EmailConfirmed
                });
            }

            return Ok(userDtos);
        }

        // api/account/{userId} (Obtener usuario por ID - Solo Admin)
        [HttpGet("{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDetailDto>> GetUserById(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "User not found"
                });
            }

            var dbContext = HttpContext.RequestServices.GetRequiredService<Data.AppDbContext>();
            var estatusNombre = await dbContext.EstatusGenerales
                .Where(e => e.Id_Estatus == user.Estatus)
                .Select(e => e.Nombre)
                .FirstOrDefaultAsync();

            return Ok(new UserDetailDto
            {
                Id = user.Id,
                Email = user.Email,
                NombreCompleto = user.NombreCompleto,
                Roles = (await _userManager.GetRolesAsync(user)).ToArray(),
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                AccessFailedCount = user.AccessFailedCount,
                Estatus = user.Estatus,
                EstatusNombre = estatusNombre,
                FechaRegistro = user.FechaRegistro,
                TipoDocumento = user.TipoDocumento,
                NumeroDocumento = user.NumeroDocumento,
                EmailConfirmed = user.EmailConfirmed
            });
        }

        // api/account/stats (Estadísticas de usuarios - Solo Admin)
        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserStats()
        {
            var dbContext = HttpContext.RequestServices.GetRequiredService<Data.AppDbContext>();
            var totalUsers = await _userManager.Users.CountAsync();
            
            var usersByStatus = await _userManager.Users
                .GroupBy(u => u.Estatus)
                .Select(g => new
                {
                    EstatusId = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var statusDetails = new List<object>();
            foreach (var stat in usersByStatus)
            {
                var estatusNombre = await dbContext.EstatusGenerales
                    .Where(e => e.Id_Estatus == stat.EstatusId)
                    .Select(e => e.Nombre)
                    .FirstOrDefaultAsync();

                statusDetails.Add(new
                {
                    EstatusId = stat.EstatusId,
                    EstatusNombre = estatusNombre,
                    Count = stat.Count,
                    Percentage = totalUsers > 0 ? Math.Round((double)stat.Count / totalUsers * 100, 2) : 0
                });
            }

            var usersRegisteredToday = await _userManager.Users
                .Where(u => u.FechaRegistro.Date == DateTime.Today)
                .CountAsync();

            var usersRegisteredThisWeek = await _userManager.Users
                .Where(u => u.FechaRegistro >= DateTime.Today.AddDays(-7))
                .CountAsync();

            var usersRegisteredThisMonth = await _userManager.Users
                .Where(u => u.FechaRegistro.Month == DateTime.Now.Month && u.FechaRegistro.Year == DateTime.Now.Year)
                .CountAsync();

            return Ok(new
            {
                TotalUsers = totalUsers,
                UsersByStatus = statusDetails,
                NewUsers = new
                {
                    Today = usersRegisteredToday,
                    ThisWeek = usersRegisteredThisWeek,
                    ThisMonth = usersRegisteredThisMonth
                }
            });
        }

        
        // ========== ENDPOINTS PARA GESTIÓN DE BLOQUEOS DE USUARIOS ==========

        // api/account/{userId}/unlock (Desbloquear usuario - Solo Admin)
        [HttpPost("{userId}/unlock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnlockUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Usuario no encontrado"
                });
            }

            // Verificar si el usuario está bloqueado
            var isLockedOut = await _userManager.IsLockedOutAsync(user);

            if (!isLockedOut)
            {
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "El usuario no está bloqueado"
                });
            }

            // Desbloquear usuario y resetear contador de intentos fallidos
            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Message = $"Usuario '{user.Email}' desbloqueado exitosamente"
            });
        }

        // api/account/{userId}/lock (Bloquear usuario manualmente - Solo Admin)
        [HttpPost("{userId}/lock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> LockUser(string userId, [FromQuery] int minutes = 30)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Usuario no encontrado"
                });
            }

            // Verificar que no sea el mismo admin
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == userId)
            {
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "No puedes bloquear tu propia cuenta"
                });
            }

            // Bloquear usuario por X minutos
            var lockoutEnd = DateTimeOffset.UtcNow.AddMinutes(minutes);
            await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
            await _userManager.SetLockoutEnabledAsync(user, true);

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Message = $"Usuario '{user.Email}' bloqueado por {minutes} minutos"
            });
        }

        // api/account/{userId}/reset-failed-attempts (Resetear intentos fallidos - Solo Admin)
        [HttpPost("{userId}/reset-failed-attempts")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetFailedAttempts(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Usuario no encontrado"
                });
            }

            var currentFailedAttempts = await _userManager.GetAccessFailedCountAsync(user);
            await _userManager.ResetAccessFailedCountAsync(user);

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Message = $"Contador de intentos fallidos reseteado. Intentos previos: {currentFailedAttempts}"
            });
        }

        // api/account/locked-users (Obtener usuarios bloqueados - Solo Admin)
        [HttpGet("locked-users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetLockedUsers()
        {
            var allUsers = await _userManager.Users.ToListAsync();
            var lockedUsers = new List<object>();

            foreach (var user in allUsers)
            {
                var isLockedOut = await _userManager.IsLockedOutAsync(user);
                if (isLockedOut)
                {
                    var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                    var failedAttempts = await _userManager.GetAccessFailedCountAsync(user);
                    var roles = await _userManager.GetRolesAsync(user);

                    lockedUsers.Add(new
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        NombreCompleto = user.NombreCompleto,
                        Roles = roles,
                        FailedAttempts = failedAttempts,
                        LockoutEnd = lockoutEnd,
                        RemainingMinutes = lockoutEnd.HasValue 
                            ? Math.Ceiling((lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes) 
                            : 0
                    });
                }
            }

            return Ok(new
            {
                TotalLocked = lockedUsers.Count,
                LockedUsers = lockedUsers
            });
        }

        // api/account/{userId}/lockout-info (Obtener información de bloqueo de un usuario - Solo Admin)
        [HttpGet("{userId}/lockout-info")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserLockoutInfo(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Usuario no encontrado"
                });
            }

            var isLockedOut = await _userManager.IsLockedOutAsync(user);
            var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
            var failedAttempts = await _userManager.GetAccessFailedCountAsync(user);
            var lockoutEnabled = await _userManager.GetLockoutEnabledAsync(user);
            var maxAttempts = _userManager.Options.Lockout.MaxFailedAccessAttempts;

            return Ok(new
            {
                UserId = user.Id,
                Email = user.Email,
                NombreCompleto = user.NombreCompleto,
                IsLockedOut = isLockedOut,
                LockoutEnabled = lockoutEnabled,
                LockoutEnd = lockoutEnd,
                RemainingMinutes = isLockedOut && lockoutEnd.HasValue 
                    ? Math.Ceiling((lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes) 
                    : 0,
                FailedAccessAttempts = failedAttempts,
                MaxAllowedAttempts = maxAttempts,
                RemainingAttempts = Math.Max(0, maxAttempts - failedAttempts)
            });
        }

        // api/account/users-at-risk (Usuarios con muchos intentos fallidos - Solo Admin)
        [HttpGet("users-at-risk")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsersAtRisk()
        {
            var allUsers = await _userManager.Users.ToListAsync();
            var usersAtRisk = new List<object>();
            var maxAttempts = _userManager.Options.Lockout.MaxFailedAccessAttempts;
            var threshold = Math.Max(1, maxAttempts - 2); // Usuarios con intentos >= maxAttempts - 2

            foreach (var user in allUsers)
            {
                var failedAttempts = await _userManager.GetAccessFailedCountAsync(user);
                if (failedAttempts >= threshold)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    usersAtRisk.Add(new
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        NombreCompleto = user.NombreCompleto,
                        Roles = roles,
                        FailedAttempts = failedAttempts,
                        RemainingAttempts = Math.Max(0, maxAttempts - failedAttempts)
                    });
                }
            }

            return Ok(new
            {
                TotalUsersAtRisk = usersAtRisk.Count,
                UsersAtRisk = usersAtRisk
            });
        }

        // ✅ api/account/confirm-email (Confirmar email - Devuelve HTML)
        [AllowAnonymous]
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
        {
            string htmlResponse;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            {
                htmlResponse = GenerateConfirmationHtml(false, "Email y token son requeridos", email);
                return Content(htmlResponse, "text/html");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                htmlResponse = GenerateConfirmationHtml(false, "Usuario no encontrado", email);
                return Content(htmlResponse, "text/html");
            }

            if (user.EmailConfirmed)
            {
                htmlResponse = GenerateConfirmationHtml(true, "Tu email ya ha sido confirmado anteriormente. Ya puedes iniciar sesión.", email);
                return Content(htmlResponse, "text/html");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            {
                htmlResponse = GenerateConfirmationHtml(false, "Error al confirmar el email. El token puede haber expirado.", email);
                return Content(htmlResponse, "text/html");
            }

            htmlResponse = GenerateConfirmationHtml(true, "¡Email confirmado exitosamente! Ya puedes iniciar sesión en BusTix.", email);
            return Content(htmlResponse, "text/html");
        }

        // Método helper para generar HTML de confirmación
        private string GenerateConfirmationHtml(bool success, string message, string? email)
        {
            var iconColor = success ? "#28a745" : "#dc3545";
            var icon = success ? "✅" : "❌";
            var title = success ? "¡Confirmación Exitosa!" : "Error de Confirmación";
            var backgroundColor = success ? "#d4edda" : "#f8d7da";
            var borderColor = success ? "#c3e6cb" : "#f5c6cb";
            var textColor = success ? "#155724" : "#721c24";

            return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{title} - BusTix</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 20px;
        }}
        .container {{
            background: white;
            border-radius: 20px;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
            max-width: 500px;
            width: 100%;
            padding: 40px;
            text-align: center;
        }}
        .icon {{
            font-size: 80px;
            margin-bottom: 20px;
        }}
        h1 {{
            color: {iconColor};
            margin-bottom: 20px;
            font-size: 28px;
        }}
        .message-box {{
            background-color: {backgroundColor};
            border: 1px solid {borderColor};
            color: {textColor};
            padding: 20px;
            border-radius: 10px;
            margin-bottom: 30px;
            line-height: 1.6;
        }}
        .email {{
            color: #666;
            font-size: 14px;
            margin-bottom: 30px;
        }}
        .btn {{
            display: inline-block;
            padding: 15px 30px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            text-decoration: none;
            border-radius: 10px;
            font-weight: bold;
            transition: transform 0.3s;
        }}
        .btn:hover {{
            transform: translateY(-2px);
            box-shadow: 0 5px 15px rgba(102, 126, 234, 0.4);
        }}
        .footer {{
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #e0e0e0;
            color: #666;
            font-size: 12px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='icon'>{icon}</div>
        <h1>{title}</h1>
        <div class='message-box'>
            {message}
        </div>
        {(string.IsNullOrEmpty(email) ? "" : $"<p class='email'>📧 {email}</p>")}
        {(success ? "<a href='http://localhost:4200/login' class='btn'>Ir al Login</a>" : "<a href='javascript:history.back()' class='btn'>Volver</a>")}
        <div class='footer'>
            <p>🚌 BusTix - Sistema de Gestión de Boletos</p>
            <p>© 2025 Todos los derechos reservados</p>
        </div>
    </div>
</body>
</html>";
        }

        // ✅ api/account/resend-confirmation-email (Reenviar email de confirmación)
        [AllowAnonymous]
        [HttpPost("resend-confirmation-email")]
        public async Task<ActionResult<AuthResponseDto>> ResendConfirmationEmail([FromBody] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Email es requerido"
                });
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Por seguridad, no revelar si el usuario existe
                return Ok(new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "Si el email existe, recibirás un correo de confirmación"
                });
            }

            if (user.EmailConfirmed)
            {
                return Ok(new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "El email ya está confirmado"
                });
            }

            // Generar token de confirmación
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = $"http://localhost:4200/confirm-email?email={user.Email}&token={WebUtility.UrlEncode(token)}";

            // Leer configuración de correo
            var mailSettings = _configuration.GetSection("MailSettings");
            var senderEmail = mailSettings["SenderEmail"];
            var senderName = mailSettings["SenderName"];
            var smtpServer = mailSettings["Server"];
            var smtpPortString = mailSettings["Port"];
            var smtpPort = smtpPortString != null ? int.Parse(smtpPortString) : 587;
            var smtpUser = mailSettings["UserName"];
            var smtpPass = mailSettings["Password"];

            try
            {
                // Enviar correo con HTML
                var message = new MimeKit.MimeMessage();
                message.From.Add(new MimeKit.MailboxAddress(senderName, senderEmail));
                message.To.Add(new MimeKit.MailboxAddress(user.NombreCompleto, user.Email)); // Eliminado operador ?? redundante
                message.Subject = "Confirma tu email - BusTix";

                var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #007bff; padding: 20px; text-align: center;'>
                        <h1 style='color: white; margin: 0;'>🚌 BusTix</h1>
                    </div>
                    <div style='padding: 30px; background-color: #f8f9fa;'>
                        <h2 style='color: #333;'>¡Hola {user.NombreCompleto ?? "Usuario"}!</h2>
                        <p style='color: #666; font-size: 16px; line-height: 1.6;'>
                            Gracias por registrarte en BusTix. Para completar tu registro y poder acceder a tu cuenta,
                            por favor confirma tu dirección de email haciendo clic en el siguiente botón:
                        </p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{confirmationLink}' style='
                                display: inline-block;
                                padding: 15px 30px;
                                background-color: #28a745;
                                color: white;
                                text-decoration: none;
                                border-radius: 5px;
                                font-weight: bold;
                                font-size: 16px;
                            '>✅ Confirmar Email</a>
                        </div>
                        <p style='color: #999; font-size: 14px;'>
                            Si no solicitaste esta confirmación, puedes ignorar este email.
                        </p>
                        <p style='color: #999; font-size: 12px;'>
                            Este link expira en 24 horas.
                        </p>
                    </div>
                    <div style='background-color: #343a40; padding: 15px; text-align: center;'>
                        <p style='color: #adb5bd; font-size: 12px; margin: 0;'>
                            © 2025 BusTix - Sistema de Gestión de Boletos
                        </p>
                    </div>
                </div>";

                message.Body = new MimeKit.TextPart("html") { Text = htmlBody };

                using var client = new MailKit.Net.Smtp.SmtpClient();
                await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpUser, smtpPass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return Ok(new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "Email de confirmación enviado. Revisa tu bandeja de entrada."
                });
            }
            catch (Exception ex)
            {
                // Log el error pero no revelar detalles al usuario
                Console.WriteLine($"Error al enviar email: {ex.Message}");
                return StatusCode(500, new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Error al enviar el email. Por favor intenta más tarde."
                });
            }
        }

        // ✅ api/account/confirm-email-direct (Confirmar email directamente desde el botón)
        [AllowAnonymous]
        [HttpPost("confirm-email-direct")]
        public async Task<ActionResult<AuthResponseDto>> ConfirmEmailDirect([FromBody] ConfirmEmailDto confirmEmailDto)
        {
            if (string.IsNullOrWhiteSpace(confirmEmailDto.Email) || string.IsNullOrWhiteSpace(confirmEmailDto.Token))
            {
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Email y token son requeridos"
                });
            }

            var user = await _userManager.FindByEmailAsync(confirmEmailDto.Email);
            if (user == null)
            {
                return NotFound(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Usuario no encontrado"
                });
            }

            if (user.EmailConfirmed)
            {
                return Ok(new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "El email ya ha sido confirmado anteriormente"
                });
            }

            var result = await _userManager.ConfirmEmailAsync(user, confirmEmailDto.Token);

            if (!result.Succeeded)
            {
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Error al confirmar el email. El token puede haber expirado."
                });
            }

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Message = "✅ Email confirmado exitosamente. Ya puedes iniciar sesión."
            });
        }
    }
}
