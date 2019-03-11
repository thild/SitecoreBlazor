using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Foundation.BlazorExtensions.Services
{
    public class InteropService
    {
        public Task<bool> HardReload(IJSRuntime jsRuntime)
        {
            return jsRuntime.InvokeAsync<bool>("blazorExtensions.hardReload");
        }

        public Task SetOnbeforeunload(IJSRuntime jsRuntime)
        {
            return jsRuntime.InvokeAsync<System.Action>(
                "blazorExtensions.setOnbeforeunload"
                );
        }

    }


}
