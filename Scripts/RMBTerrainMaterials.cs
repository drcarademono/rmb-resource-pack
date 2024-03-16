using System;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Utility.AssetInjection;
using DaggerfallWorkshop.Utility;
using System.IO;
using FullSerializer;

namespace RMBTerrainMaterials
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
        public MaterialDefinition[] defaultMaterials;
        public MaterialDefinition[] winterMaterials;
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
    }

    [ImportedComponent]
    public class RMBTerrainMaterials : MonoBehaviour
    {
        private ClimateMaterialSettings climateMaterialSettings;
        private MeshRenderer meshRenderer;
        private static readonly fsSerializer _serializer = new fsSerializer();

        static Mod mod;
        static bool WorldOfDaggerfallBiomesModEnabled = false;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            Debug.Log("RMBTerrainMaterials: Init called.");
            mod = initParams.Mod;
            GameObject modGameObject = new GameObject(mod.Title);
            modGameObject.AddComponent<RMBTerrainMaterials>();

            Mod worldOfDaggerfallBiomesMod = ModManager.Instance.GetModFromGUID("3b4319ac-34bb-411d-aa2c-d52b7b9eb69d");
            if (worldOfDaggerfallBiomesMod != null && worldOfDaggerfallBiomesMod.Enabled)
            {
                WorldOfDaggerfallBiomesModEnabled = true;
                Debug.Log("RMBTerrainMaterials: World of Daggerfall Biomes Mod is active");
            }
        }

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            Debug.Log($"[RMBTerrainMaterials] Awake called for {gameObject.name}");
            LoadClimateMaterialSettings();
        }

        private void Start()
        {
            Debug.Log("[RMBTerrainMaterials] Start called");
            UpdateMaterialBasedOnClimateAndSeason();
        }

        private void LoadClimateMaterialSettings()
        {
            string cleanName = gameObject.name.Replace("(Clone)", "").Trim();
            Debug.Log($"[RMBTerrainMaterials] Attempting to load JSON for '{cleanName}'");

            if (ModManager.Instance.TryGetAsset(cleanName + ".json", clone: false, out TextAsset jsonAsset))
            {
                string json = jsonAsset.text;
                Debug.Log($"[RMBTerrainMaterials] JSON loaded successfully, contents: {json.Substring(0, Math.Min(json.Length, 500))}...");

                fsResult result = _serializer.TryDeserialize(fsJsonParser.Parse(json), ref climateMaterialSettings);
                if (!result.Succeeded)
                {
                    Debug.LogError($"[RMBTerrainMaterials] Deserialization failed: {result.FormattedMessages}");
                }
                else
                {
                    Debug.Log("[RMBTerrainMaterials] Deserialization succeeded");
                }
            }
            else
            {
                Debug.LogError("[RMBTerrainMaterials] JSON file for material settings not found");
                climateMaterialSettings = new ClimateMaterialSettings(); // Fallback to default
            }
        }

        private Material[] LoadMaterialsFromDefinitions(MaterialDefinition[] definitions)
        {
            if (definitions == null || definitions.Length == 0) return null;

            List<Material> materials = new List<Material>();
            foreach (var def in definitions)
            {
                Material loadedMaterial = null;
                Rect rectOut;

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
                        materials.Add(null);
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
            ClimateMaterials selectedMaterials = null;

            if (!WorldOfDaggerfallBiomesModEnabled)
            {
                switch (climate)
                {
                    case MapsFile.Climates.Desert:
                    case MapsFile.Climates.Mountain:
                    case MapsFile.Climates.Rainforest:
                    case MapsFile.Climates.Swamp:
                    case MapsFile.Climates.Woodlands:
                        selectedMaterials = climateMaterialSettings.GetType().GetField(climate.ToString().ToLowerInvariant()).GetValue(climateMaterialSettings) as ClimateMaterials;
                        break;
                    default:
                        selectedMaterials = GetFallbackMaterialsForClimate(climate);
                        break;
                }
            }
            else
            {
                // Non-modified behavior; you might want to adjust this as needed for your use case.
                selectedMaterials = climateMaterialSettings.GetType().GetField(climate.ToString().ToLowerInvariant()).GetValue(climateMaterialSettings) as ClimateMaterials;
            }

            return selectedMaterials ?? climateMaterialSettings.woodlands; // Ensure a valid selection is always returned
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

            ClimateMaterials materialsForClimate = GetMaterialsForClimate(currentClimate, isWinter);
            MaterialDefinition[] definitions = isWinter ? materialsForClimate.winterMaterials : materialsForClimate.defaultMaterials;

            if (definitions == null || definitions.Length == 0)
            {
                Debug.LogError("[RMBTerrainMaterials] No definitions found for the current climate and season.");
                return;
            }

            Material[] selectedMaterials = LoadMaterialsFromDefinitions(definitions);
            if (selectedMaterials != null && selectedMaterials.Length > 0 && meshRenderer != null)
            {
                meshRenderer.materials = selectedMaterials;
            }
            else
            {
                Debug.LogError("[RMBTerrainMaterials] No valid materials found for the current climate and season.");
            }
        }

        private MapsFile.Climates GetCurrentClimate()
        {
            return (MapsFile.Climates)GameManager.Instance.PlayerGPS.CurrentClimateIndex;
        }

        private bool IsWinter()
        {
            DaggerfallDateTime now = DaggerfallUnity.Instance.WorldTime.Now;
            return now.SeasonValue == DaggerfallDateTime.Seasons.Winter;
        }
    }
}

