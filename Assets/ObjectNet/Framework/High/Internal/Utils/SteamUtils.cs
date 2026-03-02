using System;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Provide a bunch of util methods for STEAM
    /// </summary>
    public static class SteamUtils {
        
        /// <summary>
        /// The default Healten Steam Transport assembly
        /// </summary>
        const string HEATHEN_PACKAGE_ASSEMBLY = "Heathen.Steamworks";

        /// <summary>
        /// The default Heathen Steam Transport namespace
        /// </summary>
        const string HEATHEN_PACKAGE_NAMESPACE = "HeathenEngineering.SteamworksIntegration.SteamSettings";

        /// <summary>
        /// The default Healten Steam Transport namespace ( 2025 support )
        /// </summary>
        const string HEATHEN_2025_PACKAGE_NAMESPACE = "Heathen.SteamworksIntegration.SteamSettings";

        /// <summary>
        /// The default Healten Steam Transport namespace ( 2026 support )
        /// </summary>
        const string HEATHEN_2026_PACKAGE_NAMESPACE = "Heathen.SteamworksIntegration.SteamToolsSettings";

        /// <summary>
        /// The default Healten Steam Transport full namespace ( Class + Assembly )
        /// </summary>
        const string HEATHEN_PACKAGE_FULL_NAMESPACE = HEATHEN_PACKAGE_NAMESPACE + ", " + HEATHEN_PACKAGE_ASSEMBLY;

        /// <summary>
        /// The default Healten 2025 Steam Transport full namespace ( Class + Assembly )
        /// </summary>
        const string HEATHEN_2025_PACKAGE_FULL_NAMESPACE = HEATHEN_2025_PACKAGE_NAMESPACE + ", " + HEATHEN_PACKAGE_ASSEMBLY;

        /// <summary>
        /// The default Healten 2026 Steam Transport full namespace ( Class + Assembly )
        /// </summary>
        const string HEATHEN_2026_PACKAGE_FULL_NAMESPACE = HEATHEN_2026_PACKAGE_NAMESPACE + ", " + HEATHEN_PACKAGE_ASSEMBLY;


        public static bool IsHeathenInstalled() {
            // Check if unity transport is installed and activated
            bool heathenPackageInstalled = (Type.GetType(HEATHEN_PACKAGE_FULL_NAMESPACE) != null);

            // Check 2025 Healten version
            if (heathenPackageInstalled == false) {
                heathenPackageInstalled = (Type.GetType(HEATHEN_2025_PACKAGE_FULL_NAMESPACE) != null);
                // Check 2026 version
                if (heathenPackageInstalled == false) {
                    heathenPackageInstalled = (Type.GetType(HEATHEN_2026_PACKAGE_FULL_NAMESPACE) != null);
                }
            }
            return heathenPackageInstalled;
        }

        public static bool IsHeathenLite() {
            return (Type.GetType(HEATHEN_PACKAGE_FULL_NAMESPACE) != null);
        }
       
        public static bool IsHeathen2025() {
            return (Type.GetType(HEATHEN_2025_PACKAGE_FULL_NAMESPACE) != null);
        }

        public static bool IsHeathen2026() {
            return (Type.GetType(HEATHEN_2026_PACKAGE_FULL_NAMESPACE) != null);
        }
    }
}