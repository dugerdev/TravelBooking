using TravelBooking.Web.Helpers;
using System.Net;

namespace TravelBooking.Web.Services.TravelBookingApi;

public sealed class TravelBookingApiAuthHandler : DelegatingHandler
{
    private readonly ICookieHelper _cookieHelper;

    public TravelBookingApiAuthHandler(ICookieHelper cookieHelper)
    {
        _cookieHelper = cookieHelper;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _cookieHelper.GetAccessToken();
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}
