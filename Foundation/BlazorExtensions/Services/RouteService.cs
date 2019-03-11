using Foundation.BlazorExtensions.Extensions;
using Microsoft.JSInterop;
using SitecoreBlazorHosted.Shared.Models;
using SitecoreBlazorHosted.Shared.Models.Sitecore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Foundation.BlazorExtensions.Services
{
    public class RouteService
    {
        private readonly RestService _restService;
        private readonly Microsoft.AspNetCore.Components.Services.IUriHelper _uriHelper;
        private readonly SitecoreItemService _sitecoreItemService;
        private readonly TrackerService _trackerService;

        public RouteService(RestService restService, Microsoft.AspNetCore.Components.Services.IUriHelper uriHelper, SitecoreItemService sitecoreItemService, BlazorContext blazorContext, TrackerService trackerService)
        {
            _restService = restService;
            _uriHelper = uriHelper;
            _sitecoreItemService = sitecoreItemService;
            BlazorContext = blazorContext;
            _trackerService = trackerService;
        }

        private BlazorContext BlazorContext { get; }
        private Route CurrentRoute { get; set; }

        public IEnumerable<KeyValuePair<string, IList<Placeholder>>> CurrentPlaceholders { get; set; }


        public string BuildRouteApiUrl(string language, bool? hasRouteError)
        {
            string baseUrl = $"{_uriHelper.GetBaseUri()}/data/routes";

            string relativeUrl = $"{_uriHelper.ToBaseRelativePath(_uriHelper.GetBaseUri(), _uriHelper.GetAbsoluteUri())}";

            //Incorrect url
            if (hasRouteError.HasValue && hasRouteError.Value)
                return $"{baseUrl}/error/{language}.json";

            ISitecoreItem rootItem = _sitecoreItemService.GetSitecoreItemRootMock(language);

            if (rootItem.GetItSelfAndDescendants().Any(item => item.Url == "/" + relativeUrl) || relativeUrl == "")
            {
                if (relativeUrl.Length <= language.Length)
                    return $"{baseUrl}/{language}.json";

                return $"{baseUrl}{relativeUrl.Substring(language.Length)}/{language}.json";

            }


            return $"{baseUrl}/error/{language}.json";


        }



        public (bool IsCurrentUrl, string CurrentUrl) UrlIsCurrent()
        {
            string relativeUrl = _uriHelper.ToBaseRelativePath(_uriHelper.GetBaseUri(), _uriHelper.GetAbsoluteUri());

            if (string.IsNullOrWhiteSpace(CurrentRoute?.ItemLanguage))
                return (false, $"/{relativeUrl}");

            ISitecoreItem rootItem = _sitecoreItemService.GetSitecoreItemRootMock(CurrentRoute.ItemLanguage);

            return CurrentRoute != null && rootItem.GetItSelfAndDescendants().Any(item => item.Url == "/" + relativeUrl && item.Id == CurrentRoute.Id)
              ? (true, $"/{relativeUrl}")
              : (false, $"/{relativeUrl}");
        }

        private async Task TestEvent()
        {
            string instanceUrl = "http://my.site.com";
            string channelId = "27b4e611-a73d-4a95-b20a-811d295bdf65";
            string definitionId = "01f8ffbf-d662-4a87-beee-413307055c48";

            try
            {

                Console.WriteLine("Start tracking ");

                var defaultInteraction = Sitecore.UniversalTrackerClient.Request.RequestBuilder.UTEntitiesBuilder.Interaction()
                                                     .ChannelId(channelId)
                                                     .Initiator(Sitecore.UniversalTrackerClient.Entities.InteractionInitiator.Contact)
                                                     .Contact("jsdemo", "demo")
                                                     .Build();


                Console.WriteLine("Build Interaction" + defaultInteraction.Contact.Value.Identifier);

                using (var session = Sitecore.UniversalTrackerClient.Session.SessionBuilder.SitecoreUTSessionBuilder.SessionWithHost(instanceUrl)
                                                         .DefaultInteraction(defaultInteraction)
                                                         .BuildSession()
                      )
                {
                    var eventRequest = Sitecore.UniversalTrackerClient.Request.RequestBuilder.UTRequestBuilder.EventWithDefenitionId(definitionId)
                                           .Build();

                    var eventResponse = await session.TrackEventAsync(eventRequest);

                    var pageViewRequest = Sitecore.UniversalTrackerClient.Request.RequestBuilder.UTRequestBuilder.PageViewWithDefenitionId(definitionId)
                                       .Build();

                    var pageViewResponse = await session.TrackPageViewEventAsync(pageViewRequest);

                    Console.WriteLine("Track EVENT RESULT: " + eventResponse.StatusCode.ToString());
                }
            }
            catch (Exception e)
            {

                Console.WriteLine("Error tracking " + e.Message);
            }


        }


        public async Task<(Route route, IEnumerable<KeyValuePair<string, IList<Placeholder>>> flattenedPlaceholders)> LoadRoute(IJSRuntime jsRuntime, string language = null, bool hasRouteError = false)
        {
            string routeUrl = BuildRouteApiUrl(language, hasRouteError);

            this.CurrentRoute = await _restService.ExecuteRestMethod<Route>(routeUrl);

            //TestEvent();

            //await _trackerService.SendPageViewEvent(language);

            this.CurrentPlaceholders = CurrentRoute.Placeholders.FlattenThePlaceholders();

            await BlazorContext.SetCurrentRouteIdAsync(CurrentRoute.Id, jsRuntime);
            await BlazorContext.SetContextLanguageAsync(language, jsRuntime);

            return new ValueTuple<Route, IEnumerable<KeyValuePair<string, IList<Placeholder>>>>(CurrentRoute, CurrentPlaceholders);
        }

    }
}
