using System;
using UnityEngine;
using Random = UnityEngine.Random;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.FallExe;
using Stats = DaggerfallConnect.DFCareer.Stats;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Utility;


[Serializable]
public class ClimateMaterials
{
    public Material[] defaultMaterials;
    public Material[] winterMaterials;
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

public class CustomRuntimeMaterials : MonoBehaviour
{
    [SerializeField]
    private ClimateMaterialSettings climateMaterialSettings = new ClimateMaterialSettings();

    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        UpdateMaterialBasedOnClimateAndSeason();
    }

    private ClimateMaterials GetMaterialsForClimate(MapsFile.Climates climate)
    {
        // Directly return the materials for the climate, or the appropriate fallback if not defined
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
                
            // Woodlands and HauntedWoodlands fallback directly to themselves, no need for special logic here
            case MapsFile.Climates.Woodlands:
            default:
                return climateMaterialSettings.woodlands; // Default fallback, also serves as its own fallback
        }
    }

    private void UpdateMaterialBasedOnClimateAndSeason()
    {
        MapsFile.Climates currentClimate = GetCurrentClimate();
        ClimateMaterials materialsForClimate = GetMaterialsForClimate(currentClimate);
        bool isWinter = IsWinter();

        Material[] selectedMaterials = isWinter ? materialsForClimate.winterMaterials : materialsForClimate.defaultMaterials;
        if (selectedMaterials != null && selectedMaterials.Length > 0 && meshRenderer != null)
        {
            Material[] materialsToApply = new Material[meshRenderer.materials.Length];
            for (int i = 0; i < materialsToApply.Length; i++)
            {
                materialsToApply[i] = i < selectedMaterials.Length ? selectedMaterials[i] : null; // Use null or a default material as filler
            }
            meshRenderer.materials = materialsToApply;
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

