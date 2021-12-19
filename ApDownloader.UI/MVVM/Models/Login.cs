namespace ApDownloader.UI.MVVM.Models;

public class Login
{
    public Login(string email, string password)
    {
        Email = email;
        Password = password;
    }

    public string Email { get; set; }
    public string Password { get; set; }
}