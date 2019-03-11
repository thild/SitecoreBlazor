window.blazorExtensions = {
    hardReload: function () {
        window.location.reload();
    },
    setOnbeforeunload: function () {
        window.onbeforeunload = function () {
            DotNet.invokeMethodAsync('SitecoreBlazorHosted.Client', 'ClosingBrowser');
        };
    }

};
