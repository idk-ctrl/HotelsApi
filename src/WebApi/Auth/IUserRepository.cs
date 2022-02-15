namespace WebApi.Auth;

public interface IUserRepository
{
    UserDto GetUser(UserModel model);
}

public class UserRepository : IUserRepository
{

    private List<UserDto> _users = new()
    {
        new UserDto("John","123"),
        new UserDto("Monika", "123"),
        new UserDto("Nancy", "123")
    };
    public UserDto GetUser(UserModel model) => _users.FirstOrDefault(x => 
        string.Equals(x.UserName, model.UserName) &&
        string.Equals(x.Password, model.Password)) ?? 
        throw new Exception();
   
}
