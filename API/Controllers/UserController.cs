namespace API.Controllers;

/// <summary>
/// UserController
/// </summary>
/// <param name="mediator"></param>
/// <param name="logger"></param>
/// <param name="apiDbContext"></param>
/// <param name="userRepository"></param>
/// <param name="jwtConfig"></param>
/// <returns></returns>
[ApiVersion("1.0")]
[ApiVersion(0.9, Deprecated = true)]
public class UserController(IMediator mediator, ILogger logger, ApiDbContext apiDbContext,
    IUserRepository userRepository,
    MTokenInfo jwtConfig) : MControllerBase(mediator, logger)
{
    public IMUnitOfWork UnitOfWork => apiDbContext;

    //register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto command)
    {
        var result = new MResponse<object>();
        var userExist = await UserIsExistAsync(command.UserName);
        if (userExist)
        {
            result.AddApiErrorMessage("User is already exist", [command.UserName]);
            return Ok(result);
        }
        MUser userInfo = new()
        {
            Name = command.Name,
            Surname = command.Surname,
            UserName = command.UserName,
            EmailAddress = command.EmailAddress,
            Password = MPasswordHelper.HashPassword(command.Password, out string salf),
            Salf = salf
        };
        _ = await apiDbContext.AddAsync(userInfo);
        _ = await userRepository.UnitOfWork.SaveChangesAsync();
        GenerateToken(jwtConfig, userInfo,
        out string accessToken,
        out string refreshToken,
        out MUserToken loginToken);

        result.Result = new
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
        return Ok(result);
    }

    //login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto command)
    {
        var result = new MResponse<object>();
        var user = await apiDbContext.Users.FirstOrDefaultAsync(x => x.UserName == command.UserName);
        if (user == null)
        {
            result.AddApiErrorMessage("User is not exist", [command.UserName]);
            return Ok(result);
        }
        if (string.IsNullOrEmpty(user.Salf))
        {
            result.AddApiErrorMessage("Salf not exist", [command.UserName]);
            return Ok(result);
        }
        if (!MPasswordHelper.VerifyPassword(command.Password, user.Password, user.Salf))
        {
            result.AddApiErrorMessage("Password is wrong", [command.UserName]);
            return Ok(result);
        }

        GenerateToken(jwtConfig, user,
        out string accessToken,
        out string refreshToken,
        out MUserToken loginToken);

        result.Result = new
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
        _ = await apiDbContext.AddAsync(loginToken);
        _ = await apiDbContext.SaveChangesAsync();
        return Ok(result);
    }

    private static void GenerateToken(MTokenInfo jwtConfig, MUser user, out string accessToken, out string refreshToken, out MUserToken loginToken)
    {
        DateTime refreshExpirationTime = DateTime.UtcNow.AddMinutes(525960);
        MUserModel userModel = new(user.Id.ToString(), user.EntityId.ToString(), user.UserName, [user.Name]);
        accessToken = MAuthenticateTokenHelper.GenerateAuthenticateToken(jwtConfig, userModel, null);
        refreshToken = Guid.NewGuid().ToString();
        loginToken = new(user.Id, "System", "RefreshToken", accessToken, refreshExpirationTime);
    }

    private async Task<bool> UserIsExistAsync(string userName)
    {
        return await apiDbContext.Users.AnyAsync(x => x.UserName == userName);
    }
}