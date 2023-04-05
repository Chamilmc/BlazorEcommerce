namespace BlazorEcommerce.Client.Services.AuthService
{
    public interface IAuthService
    {
        Task<ServiceResponse<int>> Resister(UserRegister requesr);
        Task<ServiceResponse<string>> Login(UserLogin request);
        Task<ServiceResponse<bool>> ChangePassword(UserChangePassword request);
        Task<bool> IsUserAuthenticated();
    }
}
