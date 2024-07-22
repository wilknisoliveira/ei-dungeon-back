using ei_back.Application.Api.User.Dtos;
using ei_back.Domain.User.Interfaces;
using ei_back.Infrastructure.Token;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace ei_back.Domain.User
{
    public class LoginService : ILoginService
    {
        private const string DATE_FORMAT = "yyy-MM-dd HH:mm:ss";
        private TokenConfiguration _tokenConfiguration;

        private IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;

        public LoginService(TokenConfiguration tokenConfiguration, IUserRepository userRepository, ITokenService tokenService, IUserService userService)
        {
            _tokenConfiguration = tokenConfiguration;
            _userRepository = userRepository;
            _tokenService = tokenService;
            _userService = userService;
        }

        public TokenDtoReponse ValidateCredentials(LoginDtoRequest userDtoRequest)
        {
            //Validate the credentials in DB
            var pass = _userService.ComputeHash(userDtoRequest.Password, SHA256.Create());

            var user = _userRepository.ValidateCredentials(userDtoRequest.UserName, pass);
            if (user == null) return null;

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
            };

            if (user.Roles != null)
            {
                foreach (var role in user.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Name.ToString()));
                }
            }

            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            //Set the values in user
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(_tokenConfiguration.DaysToExpiry);

            //Update the information
            _userRepository.RefreshUserInfo(user);

            DateTime createDate = DateTime.Now;
            DateTime expirationDate = createDate.AddMinutes(_tokenConfiguration.Minutes);

            //Set the token information
            return new TokenDtoReponse(
                true,
                createDate.ToString(DATE_FORMAT),
                expirationDate.ToString(DATE_FORMAT),
                accessToken,
                refreshToken
                );
        }
    }
}
