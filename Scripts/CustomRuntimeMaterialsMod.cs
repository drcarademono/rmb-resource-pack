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

namespace CustomRuntimeMaterials
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
        [Tooltip("Fallback: Woodlands")]
        public ClimateMaterials ocean = new ClimateMaterials();

        [Tooltip("Fallback: Woodlands")]
        public ClimateMaterials desert = new ClimateMaterials();

        [Tooltip("Falls back to Desert")]
        public ClimateMaterials desert2 = new ClimateMaterials();

        [Tooltip("Fallback: Woodlands")]
        public ClimateMaterials mountain = new ClimateMaterials();

        [Tooltip("Fallback: Woodlands")]
        public ClimateMaterials rainforest = new ClimateMaterials();

        [Tooltip("Fallback: Rainforest")]
        public ClimateMaterials swamp = new ClimateMaterials();

        [Tooltip("Fallback: Desert")]
        public ClimateMaterials subtropical = new ClimateMaterials();

        [Tooltip("Fallback: Woodlands")]
        public ClimateMaterials mountainWoods = new ClimateMaterials();

        [Tooltip("Fallback: Woodlands")]
        public ClimateMaterials woodlands = new ClimateMaterials();

        [Tooltip("Fallback: Woodlands")]
        public ClimateMaterials hauntedWoodlands = new ClimateMaterials();
    }

    [ImportedComponent]
    public class CustomRuntimeMaterialsMod : MonoBehaviour
    {
        private ClimateMaterialSettings climateMaterialSettings;
        private MeshRenderer meshRenderer;
        private static readonly fsSerializer _serializer = new fsSerializer();

        static Mod mod;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            Debug.Log("CustomRuntimeMaterialsMod: Init called.");
            mod = initParams.Mod;
            var go = new GameObject(mod.Title);
            go.AddComponent<CustomRuntimeMaterialsMod>();        
        }

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            Debug.Log($"[CustomRuntimeMaterials] Awake called for {gameObject.name}");
            LoadClimateMaterialSettings();
        }

        private void Start()
        {
            Debug.Log("[CustomRuntimeMaterials] Start called");
            UpdateMaterialBasedOnClimateAndSeason();
        }

        private void LoadClimateMaterialSettings()
        {
            string cleanName = gameObject.name.Replace("(Clone)", "").Trim();
            string filePath = Path.Combine(Application.streamingAssetsPath, cleanName + ".json");
            Debug.Log($"[CustomRuntimeMaterials] Attempting to load JSON from {filePath}");

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                Debug.Log($"[CustomRuntimeMaterials] JSON loaded successfully, contents: {json.Substring(0, Math.Min(json.Length, 500))}...");

                fsResult result = _serializer.TryDeserialize(fsJsonParser.Parse(json), ref climateMaterialSettings);
                if (!result.Succeeded)
                    Debug.LogError($"[CustomRuntimeMaterials] Deserialization failed: {result.FormattedMessages}");
                else
                    Debug.Log("[CustomRuntimeMaterials] Deserialization succeeded");
            }
            else
            {
                Debug.LogError("[CustomRuntimeMaterials] JSON file for material settings not found");
                climateMaterialSettings = new ClimateMaterialSettings(); // Fallback to default
            }
        }

        private Material[] LoadMaterialsFromDefinitions(MaterialDefinition[] definitions)
        {
            if (definitions == null || definitions.Length == 0)
                return null;

            List<Material> materials = new List<Material>();
            foreach (var def in definitions)
            {
                Material loadedMaterial = null;
                Rect rectOut; // Required by GetMaterial but not necessarily used afterwards in this context

                // Get the MaterialReader instance from DaggerfallUnity
                MaterialReader materialReader = DaggerfallUnity.Instance.MaterialReader;

                // Attempt to use GetMaterial first
                loadedMaterial = materialReader.GetMaterial(def.archive, def.record, def.frame, 0, out rectOut, 0, false, false);

                if (loadedMaterial == null)
                {
                    // Fallback to TryImportMaterial if GetMaterial fails
                    if (TextureReplacement.TryImportMaterial(def.archive, def.record, def.frame, out loadedMaterial))
                    {
                        materials.Add(loadedMaterial);
                    }
                    else
                    {
                        Debug.LogWarning($"Could not load material for archive: {def.archive}, record: {def.record}, frame: {def.frame}");
                        materials.Add(null); // Handle missing materials as needed
                    }
                }
                else
                {
                    materials.Add(loadedMaterial); // Successfully loaded with GetMaterial
                }
            }
            return materials.ToArray();
        }

        private ClimateMaterials GetMaterialsForClimate(MapsFile.Climates climate)
        {
            switch (climate)
            {
                case MapsFile.Climates.Ocean:
                case MapsFile.Climates.Mountain:
                case MapsFile.Climates.HauntedWoodlands:
                    return climateMaterialSettings.ocean.defaultMaterials.Length > 0 ? climateMaterialSettings.ocean : climateMaterialSettings.woodlands;

                case MapsFile.Climates.Desert:
                    return climateMaterialSettings.desert.defaultMaterials.Length > 0 ? climateMaterialSettings.desert : climateMaterialSettings.woodlands;

                case MapsFile.Climates.Desert2:
                    return climateMaterialSettings.desert2.defaultMaterials.Length > 0 ? climateMaterialSettings.desert2 : climateMaterialSettings.desert;

                case MapsFile.Climates.Rainforest:
                    return climateMaterialSettings.rainforest.defaultMaterials.Length > 0 ? climateMaterialSettings.rainforest : climateMaterialSettings.woodlands;

                case MapsFile.Climates.Swamp:
                    return climateMaterialSettings.swamp.defaultMaterials.Length > 0 ? climateMaterialSettings.swamp : climateMaterialSettings.rainforest;

                case MapsFile.Climates.Subtropical:
                    return climateMaterialSettings.subtropical.defaultMaterials.Length > 0 ? climateMaterialSettings.subtropical : climateMaterialSettings.desert;

                case MapsFile.Climates.MountainWoods:
                    return climateMaterialSettings.mountainWoods.defaultMaterials.Length > 0 ? climateMaterialSettings.mountainWoods : climateMaterialSettings.woodlands;

                default:
                    return climateMaterialSettings.woodlands; // Default fallback, also serves as its own fallback
            }
        }

        private void UpdateMaterialBasedOnClimateAndSeason()
        {
            MapsFile.Climates currentClimate = GetCurrentClimate();
            ClimateMaterials materialsForClimate = GetMaterialsForClimate(currentClimate);
            bool isWinter = IsWinter();

            // Determine which set of MaterialDefinition to use based on season
            MaterialDefinition[] definitions = isWinter ? materialsForClimate.winterMaterials : materialsForClimate.defaultMaterials;

            // Load materials from the definitions
            Material[] selectedMaterials = LoadMaterialsFromDefinitions(definitions);

            // Apply the loaded materials to the meshRenderer
            if (selectedMaterials != null && selectedMaterials.Length > 0 && meshRenderer != null)
            {
                // Ensure the meshRenderer can hold all the materials (in case it's less)
                Material[] materialsToApply = new Material[selectedMaterials.Length];
                for (int i = 0; i < selectedMaterials.Length; i++)
                {
                    materialsToApply[i] = selectedMaterials[i]; // Apply each loaded material
                }
                meshRenderer.materials = materialsToApply;
            }
            else
            {
                Debug.LogError("[CustomRuntimeMaterials] No valid materials found for the current climate and season.");
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
