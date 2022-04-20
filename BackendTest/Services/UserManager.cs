using System.Diagnostics.Eventing.Reader;

namespace BackendTest.Services;

public class UserManager : IUserManager
{
    public bool CompareHashedPasswords(string oldPassword, string newPassword)
    {
        return oldPassword == newPassword;
    }
}