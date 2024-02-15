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
namespace CustomRuntimeMaterials
{
    public class CustomRuntimeMaterialsMod : MonoBehaviour
    {
        static Mod mod;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            Debug.Log("CustomRuntimeMaterialsMod: Init called.");
            mod = initParams.Mod;
            var go = new GameObject(mod.Title);
            go.AddComponent<CustomRuntimeMaterialsMod>();        
        }
    }
}

