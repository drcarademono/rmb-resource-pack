//#define RMB_ROCKS_FULL_LOGS

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Utility.AssetInjection;
using DaggerfallWorkshop.Utility;
using System.IO;
using FullSerializer;

namespace RMBRocksMaterials
{
    [Serializable]
    public class MaterialDefinition
    {
        public int archive;
        public int record;
        public int frame;
    }

    [Serializable]
    public class ClimateMaterials
    {
        public MaterialDefinition[] defaultMaterials = new MaterialDefinition[0];
        public MaterialDefinition[] winterMaterials = new MaterialDefinition[0];
    }

    [Serializable]
    public class ClimateMaterialSettings
    {
        public ClimateMaterials ocean = new ClimateMaterials();
        public ClimateMaterials desert = new ClimateMaterials();
        public ClimateMaterials desert2 = new ClimateMaterials();
        public ClimateMaterials mountain = new ClimateMaterials();
        public ClimateMaterials rainforest = new ClimateMaterials();
        public ClimateMaterials swamp = new ClimateMaterials();
        public ClimateMaterials subtropical = new ClimateMaterials();
        public ClimateMaterials mountainWoods = new ClimateMaterials();
        public ClimateMaterials woodlands = new ClimateMaterials();
        public ClimateMaterials hauntedWoodlands = new ClimateMaterials();
        public ClimateMaterials mountainBalfiera = new ClimateMaterials();
        public ClimateMaterials mountainHammerfell = new ClimateMaterials();
    }

    [ImportedComponent]
    public class RMBRocksMaterials : MonoBehaviour
    {
        private ClimateMaterialSettings climateMaterialSettings;
        private MeshRenderer meshRenderer;
        private static readonly fsSerializer _serializer = new fsSerializer();

        static Mod mod;
        static bool WorldOfDaggerfallBiomesModEnabled = false;

        static bool snowlessModEnabled;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            GameObject modGameObject = new GameObject(mod.Title);
            modGameObject.AddComponent<RMBRocksMaterials>();

            Mod worldOfDaggerfallBiomesMod = ModManager.Instance.GetModFromGUID("3b4319ac-34bb-411d-aa2c-d52b7b9eb69d");
            WorldOfDaggerfallBiomesModEnabled = worldOfDaggerfallBiomesMod != null && worldOfDaggerfallBiomesMod.Enabled;

            var snowlessMod1 = ModManager.Instance.GetModFromGUID("4f7f8aa1-7bd8-4f33-bd02-bbb5ac758a5d");
            var snowlessMod2 = ModManager.Instance.GetModFromGUID("510e24c8-8fc4-44c0-8927-8786b5bd0fe4");
            snowlessModEnabled = (snowlessMod1 != null && snowlessMod1.Enabled)
                              || (snowlessMod2 != null && snowlessMod2.Enabled);

            Debug.Log("RMBRocksMaterials: Init called.");
        }

        private void Awake()
        {
            // Check if the game object's name is "RMB Resource Pack" and skip loading materials if it is.
            if(gameObject.name == "RMB Resource Pack")
            {
        #if RMB_ROCKS_FULL_LOGS
                Debug.Log("[RMBRocksMaterials] Skipping material loading for 'RMB Resource Pack'.");
        #endif
                return; // Skip loading materials for this game object.
            }

            meshRenderer = GetComponent<MeshRenderer>();

        #if RMB_ROCKS_FULL_LOGS
            Debug.Log($"[RMBRocksMaterials] Awake called for {gameObject.name}");
        #endif
            LoadClimateMaterialSettings();
        }

        private void Start()
        {
            // Check if the game object's name is "RMB Resource Pack" and skip loading materials if it is.
            if(gameObject.name == "RMB Resource Pack")
            {
        #if RMB_ROCKS_FULL_LOGS
                Debug.Log("[RMBRocksMaterials] Skipping material loading for 'RMB Resource Pack'.");
        #endif
                return; // Skip loading materials for this game object.
            }

#if RMB_ROCKS_FULL_LOGS
            Debug.Log($"[RMBRocksMaterials] Start called for {gameObject.name}.");
#endif
            UpdateMaterialBasedOnClimateAndSeason();
        }

        private void LoadClimateMaterialSettings()
        {
            string cleanName = gameObject.name.Replace("(Clone)", "").Replace(".prefab", "").Trim();

            // Check if the name contains the pattern and extract the ID
            var match = System.Text.RegularExpressions.Regex.Match(cleanName, @"DaggerfallMesh \[ID=(\d+)\]");
            if (match.Success)
            {
                cleanName = match.Groups[1].Value; // Extract the ID as the cleanName
            }

        #if RMB_ROCKS_FULL_LOGS
            Debug.Log($"[RMBRocksMaterials] Attempting to load JSON for '{cleanName}'");
        #endif

            if (ModManager.Instance.TryGetAsset(cleanName + ".json", clone: false, out TextAsset jsonAsset))
            {
                string json = jsonAsset.text;
        #if RMB_ROCKS_FULL_LOGS
                Debug.Log($"[RMBRocksMaterials] JSON loaded successfully, contents: {json.Substring(0, Math.Min(json.Length, 500))}...");
        #endif

                fsResult result = _serializer.TryDeserialize(fsJsonParser.Parse(json), ref climateMaterialSettings);
                if (!result.Succeeded)
                {
                    Debug.LogError($"[RMBRocksMaterials] Deserialization failed for {gameObject.name}: {result.FormattedMessages}");
                }
                else
                {
        #if RMB_ROCKS_FULL_LOGS
                    Debug.Log($"[RMBRocksMaterials] Deserialization succeeded for {gameObject.name}");
        #endif
                }
            }
            else
            {
                Debug.LogError($"[RMBRocksMaterials] JSON file for material settings not found for {gameObject.name}.");
                climateMaterialSettings = new ClimateMaterialSettings(); // Fallback to default
            }
        }

        private Material[] LoadMaterialsFromDefinitions(MaterialDefinition[] definitions)
        {
            if (definitions == null || definitions.Length == 0)
            {
#if RMB_ROCKS_FULL_LOGS
                Debug.LogWarning($"No definitions provided to LoadMaterialsFromDefinitions for {gameObject.name}.");
#endif
                return new Material[0]; // Return an empty array.
            }

            if (DaggerfallUnity.Instance == null || DaggerfallUnity.Instance.MaterialReader == null)
            {
#if RMB_ROCKS_FULL_LOGS
                Debug.LogError("DaggerfallUnity.Instance or MaterialReader is not initialized.");
#endif
                return null; // Return null or handle appropriately.
            }

            List<Material> materials = new List<Material>();
            foreach (var def in definitions)
            {
                Material loadedMaterial = null;
                Rect rectOut;

                // This is now safe to call after the null checks
                MaterialReader materialReader = DaggerfallUnity.Instance.MaterialReader;
                
                loadedMaterial = materialReader.GetMaterial(def.archive, def.record, def.frame, 0, out rectOut, 0, false, false);

                if (loadedMaterial == null)
                {
                    if (TextureReplacement.TryImportMaterial(def.archive, def.record, def.frame, out loadedMaterial))
                    {
                        materials.Add(loadedMaterial);
                    }
                    else
                    {
                        Debug.LogWarning($"Could not load material for archive: {def.archive}, record: {def.record}, frame: {def.frame}");
                        // Consider not adding nulls to the list to avoid potential issues downstream
                    }
                }
                else
                {
                    materials.Add(loadedMaterial);
                }
            }
            return materials.ToArray();
        }

        private ClimateMaterials GetMaterialsForClimate(MapsFile.Climates climate, bool isWinter)
        {
            // Directly match the climate with its corresponding ClimateMaterials
            switch (climate)
            {
                case MapsFile.Climates.Desert:
                    return climateMaterialSettings.desert;
                case MapsFile.Climates.Desert2:
                    // Desert2 falls back to Desert if not explicitly defined
                    return climateMaterialSettings.desert2.defaultMaterials.Length > 0 ? climateMaterialSettings.desert2 : climateMaterialSettings.desert;
                case MapsFile.Climates.Mountain:
                    return climateMaterialSettings.mountain;
                case MapsFile.Climates.Rainforest:
                    return climateMaterialSettings.rainforest;
                case MapsFile.Climates.Swamp:
                    // Swamp falls back to Rainforest if not explicitly defined
                    return climateMaterialSettings.swamp.defaultMaterials.Length > 0 ? climateMaterialSettings.swamp : climateMaterialSettings.rainforest;
                case MapsFile.Climates.Subtropical:
                    // Subtropical falls back to Desert if not explicitly defined
                    return climateMaterialSettings.subtropical.defaultMaterials.Length > 0 ? climateMaterialSettings.subtropical : climateMaterialSettings.desert;
                case MapsFile.Climates.MountainWoods:
                    // MountainWoods falls back to Mountain if not explicitly defined
                    return climateMaterialSettings.mountainWoods.defaultMaterials.Length > 0 ? climateMaterialSettings.mountainWoods : climateMaterialSettings.mountain;
                case MapsFile.Climates.HauntedWoodlands:
                case MapsFile.Climates.Ocean:
                    // HauntedWoodlands and Ocean fall back to Woodlands if not explicitly defined
                    return climateMaterialSettings.hauntedWoodlands.defaultMaterials.Length > 0 ? climateMaterialSettings.hauntedWoodlands : climateMaterialSettings.woodlands;
                case MapsFile.Climates.Woodlands:
                    return climateMaterialSettings.woodlands;
                // Add additional cases as necessary for other climates
                default:
                    // Fallback for any undefined or unhandled climates
                    return climateMaterialSettings.woodlands;
            }
        }

        private ClimateMaterials GetFallbackMaterialsForClimate(MapsFile.Climates climate)
        {
            switch (climate)
            {
                case MapsFile.Climates.HauntedWoodlands:
                    return climateMaterialSettings.woodlands;
                case MapsFile.Climates.Desert2:
                    return climateMaterialSettings.desert;
                case MapsFile.Climates.MountainWoods:
                    return climateMaterialSettings.mountain;
                case MapsFile.Climates.Ocean:
                case MapsFile.Climates.Subtropical:
                    // Assume a generalized fallback for climates not explicitly handled
                    return climateMaterialSettings.desert;
                default:
                    return climateMaterialSettings.woodlands; // Default fallback
            }
        }

        private void UpdateMaterialBasedOnClimateAndSeason()
        {
            MapsFile.Climates currentClimate = GetCurrentClimate();
            bool isWinter = IsWinter();
            string currentRegionName = GameManager.Instance.PlayerGPS.CurrentRegionName;

            // Start with the default materials for the current climate
            ClimateMaterials materialsForClimate = GetMaterialsForClimate(currentClimate, isWinter);

            // Adjust materials based on specific regions and their climates
            if (currentClimate == MapsFile.Climates.Mountain)
            {
                string[] hammerfellRegions = new string[] { "Alik'r Desert", "Dragontail Mountains", "Dak'fron", "Lainlyn", "Tigonus", "Ephesus", "Santaki" };
                string[] balfieraRegion = new string[] { "Isle of Balfiera" };

                // Check for Balfiera region and apply Balfiera mountains setting regardless of the mod status
                if (balfieraRegion.Contains(currentRegionName) && climateMaterialSettings.mountainBalfiera != null && (climateMaterialSettings.mountainBalfiera.defaultMaterials?.Length > 0 || climateMaterialSettings.mountainBalfiera.winterMaterials?.Length > 0))
                {
                    materialsForClimate = climateMaterialSettings.mountainBalfiera;
                }
                // Apply Hammerfell mountains setting only if the RMB Resource Pack - Biomes mod is present
                else if (WorldOfDaggerfallBiomesModEnabled && hammerfellRegions.Contains(currentRegionName) && climateMaterialSettings.mountainHammerfell != null && (climateMaterialSettings.mountainHammerfell.defaultMaterials?.Length > 0 || climateMaterialSettings.mountainHammerfell.winterMaterials?.Length > 0))
                {
                    materialsForClimate = climateMaterialSettings.mountainHammerfell;
                } else if (!WorldOfDaggerfallBiomesModEnabled && hammerfellRegions.Contains(currentRegionName) && climateMaterialSettings.mountainBalfiera != null && (climateMaterialSettings.mountainBalfiera.defaultMaterials?.Length > 0 || climateMaterialSettings.mountainBalfiera.winterMaterials?.Length > 0))
                {
                    materialsForClimate = climateMaterialSettings.mountainBalfiera; // Use Balfiera settings for Hammerfell if RMB Resource Pack - Biomes mod not present
                }
            }

            // Load and apply materials based on the definitions
            MaterialDefinition[] definitions = isWinter ? materialsForClimate.winterMaterials : materialsForClimate.defaultMaterials;
            Material[] selectedMaterials = LoadMaterialsFromDefinitions(definitions);

            if (selectedMaterials != null && selectedMaterials.Length > 0 && meshRenderer != null)
            {
                meshRenderer.materials = selectedMaterials;
            }
            else
            {
                Debug.LogError($"[RMBRocksMaterials] No valid materials found for the current climate and season for {gameObject.name}.");
            }
        }

        private MapsFile.Climates GetCurrentClimate()
        {
            return (MapsFile.Climates)GameManager.Instance.PlayerGPS.CurrentClimateIndex;
        }

        private bool IsWinter()
        {
            if (GameManager.Instance?.PlayerGPS == null) return false;
            var now = DaggerfallUnity.Instance.WorldTime.Now;
            int c = GameManager.Instance.PlayerGPS.CurrentClimateIndex;

            return now.SeasonValue == DaggerfallDateTime.Seasons.Winter
                && c != (int)MapsFile.Climates.Desert
                && c != (int)MapsFile.Climates.Desert2
                && c != (int)MapsFile.Climates.Subtropical
                && (!snowlessModEnabled 
                    || (c != (int)MapsFile.Climates.Rainforest 
                     && c != (int)MapsFile.Climates.Swamp));
        }
    }
}

