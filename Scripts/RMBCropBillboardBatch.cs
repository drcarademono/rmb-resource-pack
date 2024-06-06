using UnityEngine;
using System.Collections.Generic;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Utility;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Utility.AssetInjection;

[ImportedComponent]
public class RMBCropBillboardBatch : MonoBehaviour
{
    private static Mod mod;
    private static GameObject modGameObject;
    public int textureArchive = 504; // The index for the texture archive to load billboard textures from
    public int initialTextureRecord = 0; // The initial index for the texture record to use within the archive
    public float gridSpacing = 4.0f; // Spacing between billboards in the grid
    public float noiseAmount = 1.0f; // Amount of random noise to apply to billboard positions
    public int rangeX = 40; // Range for x direction
    public int rangeY = 40; // Range for y direction
    public float overlapCheckRadius = 1.0f; // Radius for checking overlaps
    private Dictionary<int, Material> billboardMaterials;
    private Dictionary<int, List<GameObject>> billboardBatches;
    private Vector2 billboardSize;
    private int currentArchive;

    [Invoke(StateManager.StateTypes.Start, 0)]
    public static void Init(InitParams initParams)
    {
        mod = initParams.Mod;
        modGameObject = new GameObject(mod.Title);
        modGameObject.AddComponent<RMBCropBillboardBatch>();
        Debug.Log("RMBCropBillboardBatch: Init called and component added to game object.");
    }

    void Start()
    {
        Debug.Log("RMBCropBillboardBatch: Start called.");
        if (gameObject.name == "RMB Resource Pack")
        {
            return; // Skip loading materials for this game object.
        }
        InitializeBillboardBatch();
    }

    void InitializeBillboardBatch()
    {
        Debug.Log("RMBCropBillboardBatch: Initializing billboard batch...");

        // Determine the current archive based on the season
        currentArchive = IsWinter() ? 511 : textureArchive;

        // Get the possible texture records based on the climate and season
        int[] textureRecords = AdjustRecordBasedOnClimate(initialTextureRecord);

        // Initialize materials and batches for each texture record
        billboardMaterials = new Dictionary<int, Material>();
        billboardBatches = new Dictionary<int, List<GameObject>>();

        foreach (int record in textureRecords)
        {
            Material material = GetBillboardMaterial(currentArchive, record);
            if (material != null)
            {
                billboardMaterials[record] = material;
                billboardBatches[record] = new List<GameObject>();
            }
        }

        // Create and position billboards
        List<Vector3> billboardPositions = GenerateBillboardPositions();
        AddBillboardsToBatch(billboardPositions);

        Debug.Log("RMBCropBillboardBatch: Billboard batch initialization complete.");
    }

    int[] AdjustRecordBasedOnClimate(int initialRecord)
    {
        if (IsWinter())
        {
            return new[] { 22 }; // Only one record for winter
        }

        MapsFile.Climates currentClimate = GetCurrentClimate();
        switch (currentClimate)
        {
            case MapsFile.Climates.Mountain:
                return initialRecord == 0 ? new[] { 0, 1 } : new[] { 1 };
            case MapsFile.Climates.Desert:
                return new[] { 2 };
            case MapsFile.Climates.Desert2:
                return new[] { 20 };
            case MapsFile.Climates.Subtropical:
                return initialRecord == 0 ? new[] { 3, 4 } : new[] { 4 };
            case MapsFile.Climates.Rainforest:
            case MapsFile.Climates.Swamp:
                return initialRecord == 0 ? new[] { 7, 8 } : new[] { 8 };
            default:
                return initialRecord == 0 ? new[] { 19, 21 } : new[] { 21 };
        }
    }

    MapsFile.Climates GetCurrentClimate()
    {
        if (GameManager.Instance == null || GameManager.Instance.PlayerGPS == null || GameManager.Instance.PlayerObject == null)
        {
            return (MapsFile.Climates)231; // Return default climate as 231
        }
        return (MapsFile.Climates)GameManager.Instance.PlayerGPS.CurrentClimateIndex;
    }

    bool IsWinter()
    {
        if (GameManager.Instance == null || GameManager.Instance.PlayerGPS == null || GameManager.Instance.PlayerObject == null)
        {
            return false; // Return default value of false
        }
        DaggerfallDateTime now = DaggerfallUnity.Instance.WorldTime.Now;
        return now.SeasonValue == DaggerfallDateTime.Seasons.Winter &&
               GameManager.Instance.PlayerGPS.CurrentClimateIndex != (int)MapsFile.Climates.Subtropical &&
               GameManager.Instance.PlayerGPS.CurrentClimateIndex != (int)MapsFile.Climates.Desert &&
               GameManager.Instance.PlayerGPS.CurrentClimateIndex != (int)MapsFile.Climates.Desert2;
    }

    Material GetBillboardMaterial(int archive, int record)
    {
        Debug.Log($"RMBCropBillboardBatch: Getting material for archive {archive}, record {record}...");
        MaterialReader materialReader = DaggerfallUnity.Instance.MaterialReader;
        MeshReader meshReader = DaggerfallUnity.Instance.MeshReader;
        if (materialReader == null || meshReader == null)
        {
            Debug.LogError("RMBCropBillboardBatch: MaterialReader or MeshReader is null.");
            return null;
        }

        Rect rectOut;
        Vector2 sizeOut;
        Material material = materialReader.GetMaterial(archive, record, 0, 0, out rectOut, 4, true, true);
        Mesh mesh = meshReader.GetBillboardMesh(rectOut, archive, record, out sizeOut);

        if (material == null || mesh == null)
        {
            Debug.LogError("RMBCropBillboardBatch: Failed to load material or mesh.");
            return null;
        }

        // Ensure the texture is readable
        Texture2D mainTexture = material.mainTexture as Texture2D;
        if (mainTexture != null)
        {
            mainTexture.filterMode = FilterMode.Point; // Set point filter mode
            material.mainTexture = mainTexture;
        }

        // Store the billboard size
        billboardSize = sizeOut;

        // Force the renderer to use the transparent shader
        material.shader = Shader.Find("Daggerfall/Billboard");

        Debug.Log("RMBCropBillboardBatch: Material successfully retrieved.");
        return material;
    }

    List<Vector3> GenerateBillboardPositions()
    {
        //Debug.Log("RMBCropBillboardBatch: Generating billboard positions...");
        List<Vector3> positions = new List<Vector3>();

        for (int x = -rangeX; x <= rangeX; x++)
        {
            for (int z = -rangeY; z <= rangeY; z++)
            {
                float offsetX = Random.Range(-noiseAmount, noiseAmount);
                float offsetZ = Random.Range(-noiseAmount, noiseAmount);
                Vector3 position = new Vector3(x * gridSpacing + offsetX, 0, z * gridSpacing + offsetZ);
                positions.Add(position);
            }
        }

        //Debug.Log("RMBCropBillboardBatch: Billboard positions generated.");
        return positions;
    }

    void AddBillboardsToBatch(List<Vector3> billboardPositions)
    {
        //Debug.Log("RMBCropBillboardBatch: Adding billboards to batch...");
        foreach (Vector3 position in billboardPositions)
        {
            Vector3 relativePosition = transform.position + position;
            if (!IsOverlapping(relativePosition))
            {
                int randomRecord = GetRandomRecord();
                GameObject billboard = CreateBillboard(relativePosition, randomRecord);
                if (billboard != null)
                {
                    billboardBatches[randomRecord].Add(billboard);
                }
            }
        }
        //Debug.Log("RMBCropBillboardBatch: Billboards added to batch.");
    }

    bool IsOverlapping(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, overlapCheckRadius);
        foreach (var collider in colliders)
        {
            if (collider.gameObject.GetComponent<Terrain>() == null)
            {
                return true;
            }
        }
        return false;
    }

    int GetRandomRecord()
    {
        List<int> keys = new List<int>(billboardBatches.Keys);
        int randomIndex = Random.Range(0, keys.Count);
        return keys[randomIndex];
    }

    GameObject CreateBillboard(Vector3 position, int record)
    {
        //Debug.Log($"RMBCropBillboardBatch: Creating billboard at position {position} with record {record}...");
        GameObject billboard = GameObject.CreatePrimitive(PrimitiveType.Quad);

        // Align the bottom of the billboard to the y-coordinate of the parent GameObject
        float billboardHeight = billboardSize.y;
        billboard.transform.position = new Vector3(position.x, transform.position.y + billboardHeight / 2, position.z);
        billboard.transform.localScale = new Vector3(billboardSize.x, billboardHeight, 1); // Adjust scale based on billboard size

        // Remove collider from the billboard
        Destroy(billboard.GetComponent<Collider>());

        MeshRenderer renderer = billboard.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material = billboardMaterials[record];
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        billboard.transform.SetParent(transform); // Ensure the billboard is a child of the current GameObject
        billboard.AddComponent<FaceCamera>(); // Add the FaceCamera script to the billboard

        //Debug.Log("RMBCropBillboardBatch: Billboard created.");
        return billboard;
    }

    void OnDestroy()
    {
        //Debug.Log("RMBCropBillboardBatch: OnDestroy called.");
        // Clean up resources when the game object is destroyed
        if (modGameObject != null)
        {
            Destroy(modGameObject);
            modGameObject = null;
        }

        // Destroy all created billboards
        if (billboardBatches != null)
        {
            foreach (var batch in billboardBatches.Values)
            {
                foreach (GameObject billboard in batch)
                {
                    Destroy(billboard);
                }
            }
            billboardBatches.Clear();
        }

        //Debug.Log("RMBCropBillboardBatch: Resources cleaned up.");
    }

    private class FaceCamera : MonoBehaviour
    {
        private Camera mainCamera;

        void Start()
        {
            mainCamera = Camera.main;
        }

        void Update()
        {
            if (mainCamera)
            {
                Vector3 viewDirection = new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z);
                transform.LookAt(transform.position + viewDirection);
            }
        }
    }
}

