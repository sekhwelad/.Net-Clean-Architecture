using Blazored.LocalStorage;
using System.Net.Http.Headers;

namespace HR.LeaveManagement.BlazorUI.Services.Base
{
    public class BaseHttpService
    {
        protected IClient _client;
        protected readonly ILocalStorageService _localStorage;

        public BaseHttpService(IClient client, ILocalStorageService localStorage)
        {
            _client = client;
            _localStorage = localStorage;   

        }

        protected Response<Guid> ConvertApiExceptions<Guid>(ApiException exception)
        {
            if (exception.StatusCode == 400)
            {
                return new Response<Guid>()
                {
                    Message = "Invalid data was submitted",
                    ValidationErrors = exception.Response,
                    Success = false
                };
                
            }
            else if(exception.StatusCode == 404)
            {
                return new Response<Guid>()
                {
                    Message = "The record was not found",
                    ValidationErrors = exception.Response,
                    Success = false
                };
            }
            else
            {
                return new Response<Guid>()
                {
                    Message = "Something went wrong, please try again later.",
                    ValidationErrors = exception.Response,
                    Success = false
                };
            }
        }

        protected async Task AddBearerToken()
        {
            var xx = await _localStorage.ContainKeyAsync("token");

            if (await _localStorage.ContainKeyAsync("token"))
                _client.HttpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", await _localStorage.GetItemAsync<string>("token"));
        }
    }
}
