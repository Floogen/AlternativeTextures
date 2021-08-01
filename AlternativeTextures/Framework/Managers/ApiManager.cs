using AlternativeTextures.Framework.Interfaces;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlternativeTextures.Framework.Managers
{
    internal class ApiManager
    {
        private IMonitor _monitor;
        private IJsonAssetsApi _jsonAssetsApi;

        public ApiManager(IMonitor monitor)
        {
            _monitor = monitor;
        }

        internal bool HookIntoJsonAssets(IModHelper helper)
        {
            _jsonAssetsApi = helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");

            if (_jsonAssetsApi is null)
            {
                _monitor.Log("Failed to hook into spacechase0.JsonAssets.", LogLevel.Error);
                return false;
            }

            _monitor.Log("Successfully hooked into spacechase0.JsonAssets.", LogLevel.Debug);
            return true;
        }

        internal IJsonAssetsApi GetJsonAssetsApi()
        {
            return _jsonAssetsApi;
        }
    }
}
