namespace BackendTest.Services;

public interface IUserManager
{
    bool CompareHashedPasswords(string oldPassword, string newPassword);
}