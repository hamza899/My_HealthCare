using System.Net.Http.Headers;

namespace MyHealthcare.Web.Services;

public class AuthMessageHandler : DelegatingHandler
{
    private readonly AuthService _auth;

    public AuthMessageHandler(AuthService auth)
    {
        _auth = auth;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = _auth.GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
