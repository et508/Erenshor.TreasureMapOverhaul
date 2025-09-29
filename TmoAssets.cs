using System;
using System.IO;
using UnityEngine;

namespace TreasureMapOverhaul
{
    internal static class TmoAssets
    {
        internal const string BundleFileName = "tmo_assets";

        private static AssetBundle _bundle;
        private static bool _loggedAttempt;

        internal static AssetBundle Bundle => _bundle;

        internal static bool EnsureLoaded()
        {
            if (_bundle != null) return true;

            // Only log the attempt once to keep logs clean
            if (!_loggedAttempt)
            {
                _loggedAttempt = true;
                Plugin.Log.LogInfo("[TMO] Loading shared asset bundle…");
            }

            try
            {
                var dllDir = Path.GetDirectoryName(typeof(Plugin).Assembly.Location);
                var bundlePath = Path.Combine(dllDir ?? ".", BundleFileName);

                _bundle = AssetBundle.LoadFromFile(bundlePath);
                if (_bundle == null)
                {
                    Plugin.Log.LogError($"[TMO] Failed to load AssetBundle at: {bundlePath}");
                    return false;
                }

                Plugin.Log.LogInfo($"[TMO] Asset bundle loaded: {bundlePath}");
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TMO] EnsureLoaded exception: {ex}");
                return false;
            }
        }

        internal static T Load<T>(string name) where T : UnityEngine.Object
        {
            if (!EnsureLoaded()) return null;
            try { return _bundle.LoadAsset<T>(name); }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TMO] Load<{typeof(T).Name}>('{name}') failed: {ex}");
                return null;
            }
        }

        internal static T[] LoadAll<T>() where T : UnityEngine.Object
        {
            if (!EnsureLoaded()) return Array.Empty<T>();
            try { return _bundle.LoadAllAssets<T>(); }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TMO] LoadAll<{typeof(T).Name}> failed: {ex}");
                return Array.Empty<T>();
            }
        }
    }
}
