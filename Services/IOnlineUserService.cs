namespace bikey.Services;

public interface IOnlineUserService
{
    void UserLoggedIn(int userId);
    void UserLoggedOut(int userId);
    bool IsUserOnline(int userId);
    IEnumerable<int> GetOnlineUserIds();
}