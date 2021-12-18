using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ApDownloader.MVVM.Models;

namespace ApDownloader.MVVM.ViewModels;

public class LoginViewModel
{
    private const string LoginUrl = @"https://www.armstrongpowerhouse.com/index.php?route=account/login";
    private HttpClient _client;
    public object _currentView;
    private Login _login;


    public async Task<bool> LoginUser()
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(_login.Email), "email");
        content.Add(new StringContent(_login.Password), "password");
        var response = await _client.PostAsync(LoginUrl, content);
        return response.StatusCode == HttpStatusCode.Redirect;
    }
}