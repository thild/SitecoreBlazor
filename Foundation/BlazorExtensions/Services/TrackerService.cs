using Sitecore.UniversalTrackerClient.Entities;
using Sitecore.UniversalTrackerClient.Request.RequestBuilder;
using Sitecore.UniversalTrackerClient.Response;
using Sitecore.UniversalTrackerClient.Session;
using Sitecore.UniversalTrackerClient.Session.SessionBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Foundation.BlazorExtensions.Services
{
    public class TrackerService : IDisposable
    {
        private readonly Microsoft.AspNetCore.Components.Services.IUriHelper _uriHelper;

        private ISitecoreUTSession CurrentSession { get; set; }

        private string DefinitionId { get; set; }

        public TrackerService(Microsoft.AspNetCore.Components.Services.IUriHelper uriHelper)
        {
            _uriHelper = uriHelper;
        }

        public void CreateSession()
        {

            string instanceUrl = "http://my.site.com";
            string channelId = "27b4e611-a73d-4a95-b20a-811d295bdf65";
            DefinitionId = "01f8ffbf-d662-4a87-beee-413307055c48";


            var defaultInteraction = UTEntitiesBuilder.Interaction()
                                                      .ChannelId(channelId)
                                                      .Initiator(InteractionInitiator.Contact)
                                                      .Contact("jsdemo", "demo")
                                                      .Build();

            CurrentSession = SitecoreUTSessionBuilder.SessionWithHost(instanceUrl)  //https://utwebtests
                                                   .DefaultInteraction(defaultInteraction)
                                                   .DeviceIdentifier("Blazor")
                                                   .BuildSession();
        }


        public Task<UTResponse> SendPageViewEvent(string language)
        {

            string relativeUrl = $"{_uriHelper.ToBaseRelativePath(_uriHelper.GetBaseUri(), _uriHelper.GetAbsoluteUri())}";

            var pageViewRequest = UTRequestBuilder.PageViewWithDefenitionId(DefinitionId)
                                                  .Timestamp(DateTime.Now)
                                                  .Url(relativeUrl)
                                                  .ItemId("01f8ffbf-d662-4a87-beee-413307055c48")
                                                  .ItemLanguage(language)
                                                  .AddCustomValues("key", "value")
                                                  .Build();

            return CurrentSession?.TrackPageViewEventAsync(pageViewRequest);
        }



        public void EndSession()
        {
            CurrentSession?.Dispose();
        }

        public void Dispose()
        {
            EndSession();
        }

    }
}
