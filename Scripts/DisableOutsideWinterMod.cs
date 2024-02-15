using System;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Utility;

/// <summary>
/// Disables the GameObject if the current season is not Winter.
/// </summary>
namespace DisableOutsideWinter
{
    [ImportedComponent]
    public class DisableOutsideWinterMod : MonoBehaviour
    {

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
        }

        private void Awake()
        {
            // Check if it's currently winter
            if (!IsWinter())
            {
                // If it's not winter, disable this GameObject
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Checks if the current season is winter.
        /// </summary>
        /// <returns>True if it's winter, false otherwise.</returns>
        private bool IsWinter()
        {
            // Checks the current climate against desert climates and the current season
            return GameManager.Instance.PlayerGPS.CurrentClimateIndex != (int)MapsFile.Climates.Desert &&
                   GameManager.Instance.PlayerGPS.CurrentClimateIndex != (int)MapsFile.Climates.Desert2 &&
                   DaggerfallUnity.Instance.WorldTime.Now.SeasonValue == DaggerfallDateTime.Seasons.Winter;
        }
    }
}
