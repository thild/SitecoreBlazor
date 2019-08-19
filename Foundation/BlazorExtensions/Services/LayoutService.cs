﻿using Foundation.BlazorExtensions.Extensions;
using Foundation.BlazorExtensions.Factories;
using Microsoft.AspNetCore.Components;
using SitecoreBlazorHosted.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Foundation.BlazorExtensions.Services
{
    public class LayoutService
    {
        private readonly ComponentFactory _componentFactory;
        private readonly RouteService _routeService;
        private readonly IUriHelper _uriHelper;
        private readonly BlazorStateMachine _blazorStateMachine;

        public LayoutService(ComponentFactory componentFactory, RouteService routeService, IUriHelper uriHelper, BlazorStateMachine blazorStateMachine)
        {
            _componentFactory = componentFactory;
            _routeService = routeService;
            _uriHelper = uriHelper;
            _blazorStateMachine = blazorStateMachine;
        }

        public event EventHandler LanguageSwitch;


        public void SwitchLanguage(Language language)
        {
            LanguageSwitchArgs args = new LanguageSwitchArgs
            {
                Language = language
            };
            LanguageSwitch?.Invoke(this, args);
        }

        /// <summary>
        /// LoadRoute is called on state changed(each "request")
        /// </summary>
        /// <param name="language"></param>
        /// <param name="hasRouteError"></param>
        /// <returns></returns>
        public async Task LoadRoute(string language, bool hasRouteError)
        {

            if (_blazorStateMachine.CurrentPlaceholders == null || !_routeService.UrlIsCurrent().IsCurrentUrl)
            {

                ComponentsInDynamicPlaceholdersAllreadyRenderdPerStateChanged = new List<string>();

                await _routeService.LoadRoute(language, hasRouteError);
            }
        }


        private List<string> ComponentsInDynamicPlaceholdersAllreadyRenderdPerStateChanged
        {
            get; set;
        }


        public Task<List<RenderFragment>> RenderPlaceholders(string placeholder, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                List<RenderFragment> list = new List<RenderFragment>();

                try
                {

                    IList<KeyValuePair<string, IList<Placeholder>>> placeHoldersList = _blazorStateMachine.CurrentPlaceholders?.Where(ph => ph.Key.ExtractPlaceholderName() == placeholder).ToList();

                    if (placeHoldersList == null)
                        return null;

                    foreach (KeyValuePair<string, IList<Placeholder>> keyVal in placeHoldersList)
                    {

                        cancellationToken.ThrowIfCancellationRequested();

                        foreach (Placeholder placeholderData in keyVal.Value)
                        {

                            cancellationToken.ThrowIfCancellationRequested();

                            if (placeholderData == null)
                                continue;

                            string keyName = $"{keyVal.Key}-{placeholderData.ComponentName}";

                            if (ComponentsInDynamicPlaceholdersAllreadyRenderdPerStateChanged.Any(comp => comp == keyName))
                                continue;


                            list.Add(_componentFactory.CreateComponent(placeholderData));

                            if (keyVal.Key.IsDynamicPlaceholder())
                                ComponentsInDynamicPlaceholdersAllreadyRenderdPerStateChanged.Add(keyName);

                        }
                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error RenderPlaceholders {ex.Message}");
                }

                return list;

            }, cancellationToken);
        }

    }
}
