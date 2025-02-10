using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.SimpleNotificationService.Model;
using API.Constants;
using API.Models.Auth;
using API.Models.Config;
using API.Models.Projects;
using API.Repositories;
using Microsoft.IdentityModel.Tokens;
using NotFoundException = Amazon.SimpleEmailV2.Model.NotFoundException;

namespace API.Handlers
{
    public interface IAuthHandler
    {
        public string HashUserPassword(string salt, string password);
        public Task<Authorization> CreateUserAuthorisation(AuthUser user);
        public Task<Authorization> RefreshAccessToken(string refreshToken, string accessToken);
        public Task<Authorization> CreateProjectUserAuthorization(Project project, ProjectUser user);
        public Authorization CreateAdminUserAuthorisation(AuthUser user);
        public string CreateResetPasswordToken(AuthUser user);
        public string CreateRegisterToken(AuthUser user);
    }

    public class AuthHandler : IAuthHandler
    {
        private readonly JWTConfig _jwtConfig;
        private readonly DynamoDBContext _dynamoDbContext;
        private readonly JwtSecurityTokenHandler _tokenHandler;
        private readonly SigningCredentials _credentials;
        private readonly IRoleContext _roleContext;

        public AuthHandler(JWTConfig jwtConfig, IAmazonDynamoDB client, IRoleContext roleContext)
        {
            _jwtConfig = jwtConfig;
            _roleContext = roleContext;
            _dynamoDbContext = new DynamoDBContext(client);
            _tokenHandler = new JwtSecurityTokenHandler();
            _credentials = new SigningCredentials(_jwtConfig.SigningKey, SecurityAlgorithms.HmacSha256Signature);
        }

        public async Task<Authorization> CreateUserAuthorisation(AuthUser user)
        {
            var expiresAt = DateTime.UtcNow.Add(_jwtConfig.TokenLifetime);
            var securityToken = _tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                SigningCredentials = _credentials,
                Expires = expiresAt,
                Audience = _jwtConfig.Audience,
                Subject = new ClaimsIdentity(new List<Claim>
                {
                    new Claim(JwtClaimNames.AccessTokenId, Guid.NewGuid().ToString()),
                    new Claim(JwtClaimNames.UserId, user.Id ?? ""),
                    new Claim(JwtClaimNames.UserEmail, user.Email ?? ""),
                    new Claim(JwtClaimNames.UserDisplayName, user.Name ?? ""),
                    new Claim(JwtClaimNames.TextLanguage, user.TextLanguage ??  Languages.Default),
                }.Where(c => !string.IsNullOrEmpty(c.Value))),
            });

            var accessToken = _tokenHandler.WriteToken(securityToken);
            var refreshToken = Guid.NewGuid().ToString();
            var authorization = new Authorization
            {
                Id = refreshToken,
                TokenType = "Bearer",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                TextLanguage = user.TextLanguage ?? Languages.Default,
            };

            await _dynamoDbContext.SaveAsync(authorization);
            return authorization;
        }

        public async Task<Authorization> CreateProjectUserAuthorization(Project project, ProjectUser user)
        {
            var expiresAt = DateTime.UtcNow.Add(_jwtConfig.TokenLifetime);

            var role = await _roleContext.Roles.Read(user.RoleId);
            var permissionClaims =
                role.Permissions.Select(permission => new Claim(JwtClaimNames.Permission, permission));
            
            var securityToken = _tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                SigningCredentials = _credentials,
                Expires = expiresAt,
                Audience = _jwtConfig.Audience,
                Subject = new ClaimsIdentity(new List<Claim>
                {
                    new Claim(JwtClaimNames.AccessTokenId, Guid.NewGuid().ToString()),
                    new Claim(JwtClaimNames.UserId, user.Id ?? ""),
                    new Claim(JwtClaimNames.UserDisplayName, user.Name ?? ""),
                    new Claim(JwtClaimNames.ProjectId, project.Id ?? ""),
                    new Claim(JwtClaimNames.ProjectName, project.Name ?? ""),
                    new Claim(JwtClaimNames.RoleId, user.RoleId ?? ""),
                    new Claim(JwtClaimNames.TextLanguage, project.TextLanguage ?? Languages.Default),
                }
                    .Concat(permissionClaims)
                    .Where(c => !string.IsNullOrEmpty(c.Value))),
            });

            var accessToken = _tokenHandler.WriteToken(securityToken);
            return new Authorization
            {
                TokenType = "Bearer",
                AccessToken = accessToken,
                ExpiresAt = expiresAt,
                TextLanguage = project.TextLanguage,
            };
        }

        public Authorization CreateAdminUserAuthorisation(AuthUser user)
        {
            var expiresAt = DateTime.UtcNow.Add(_jwtConfig.TokenLifetime);
            var securityToken = _tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                SigningCredentials = _credentials,
                Expires = expiresAt,
                Audience = _jwtConfig.Audience,
                Subject = new ClaimsIdentity(new List<Claim>
                {
                    new Claim(JwtClaimNames.AccessTokenId, Guid.NewGuid().ToString()),
                    new Claim(JwtClaimNames.AdminId, user.Id ?? ""),
                    new Claim(JwtClaimNames.UserId, user.Id ?? ""),
                    new Claim(JwtClaimNames.UserDisplayName, user.Name ?? ""),
                    new Claim(PolicyNames.Permissions, Permissions.Admin.All),
                    new Claim(JwtClaimNames.TextLanguage, user.TextLanguage ?? Languages.Default),
                }.Where(c => !string.IsNullOrEmpty(c.Value))),
            });

            var accessToken = _tokenHandler.WriteToken(securityToken);
            return new Authorization
            {
                TokenType = "Bearer",
                AccessToken = accessToken,
                ExpiresAt = expiresAt,
                TextLanguage = user.TextLanguage,
            };
        }
        
        public async Task<Authorization> RefreshAccessToken(string refreshToken, string accessToken)
        {
            var authorization = await _dynamoDbContext.LoadAsync<Authorization>(refreshToken);
            if (authorization == null)
            {
                throw new NotFoundException("Token not found");
            }

            var principal = _tokenHandler.ValidateToken(accessToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _jwtConfig.SigningKey,
                ValidateLifetime = false, // Don't validate lifetime on refresh
                RequireExpirationTime = false,
                ValidateIssuer = false,
                ValidateAudience = false,
            }, out _);

            if (principal == null)
            {
                throw new ValidationException("Failed to validate access token");
            }

            var expiresAt = DateTime.UtcNow.Add(_jwtConfig.TokenLifetime);
            var securityToken = _tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                SigningCredentials = _credentials,
                Audience = _jwtConfig.Audience,
                Expires = expiresAt,
                Subject = new ClaimsIdentity(principal.Claims),
            });

            authorization.AccessToken = _tokenHandler.WriteToken(securityToken);
            authorization.ExpiresAt = expiresAt;
            await _dynamoDbContext.SaveAsync(authorization);

            return authorization;
        }

        public string HashUserPassword(string salt, string password)
        {
            var sha256 = SHA256.Create();
            var passwordHash = sha256.ComputeHash(Encoding.UTF8.GetBytes($"{salt}.{_jwtConfig.Secret}.{password}"));
            var passwordHashB64 = Convert.ToBase64String(passwordHash);
            return passwordHashB64;
        }


        public string CreateResetPasswordToken(AuthUser user) => CreateUserActionToken(user.Id,
            new List<Claim> {new Claim(PolicyNames.Permissions, Permissions.Auth.UpdatePassword)});

        public string CreateRegisterToken(AuthUser user) => CreateUserActionToken(user.Id,
            new List<Claim>
            {
                new Claim(PolicyNames.Permissions, Permissions.Auth.CreatePassword),
                new Claim(JwtClaimNames.UserDisplayName, user.FirstName),
                new Claim(JwtClaimNames.UserEmail, user.Email),
            });

        private string CreateUserActionToken(string uid, List<Claim> claims)
        {
            claims.Add(new Claim(JwtClaimNames.AccessTokenId, Guid.NewGuid().ToString()));
            claims.Add(new Claim(JwtClaimNames.UserId, uid));

            var expiresAt = DateTime.UtcNow.Add(new TimeSpan(7, 0, 0, 0));
            var registerToken = _tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                SigningCredentials = _credentials,
                Expires = expiresAt,
                Audience = _jwtConfig.Audience,
                Subject = new ClaimsIdentity(claims),
            });

            return _tokenHandler.WriteToken(registerToken);
        }
    }
}