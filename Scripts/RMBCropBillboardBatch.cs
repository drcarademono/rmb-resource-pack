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
    public int textureRecord = 0; // The index for the texture record to use within the archive
    public float gridSpacing = 4.0f; // Spacing between billboards in the grid
    public float noiseAmount = 1.0f; // Amount of random noise to apply to billboard positions
    private Material billboardMaterial;
    private List<GameObject> billboards;
    private Vector2 billboardSize;

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
        InitializeBillboardBatch();
    }

    void InitializeBillboardBatch()
    {
        Debug.Log("RMBCropBillboardBatch: Initializing billboard batch...");

        // Get the material for the billboards
        billboardMaterial = GetBillboardMaterial(textureArchive, textureRecord);

        // Ensure the material was successfully created
        if (billboardMaterial == null)
        {
            Debug.LogError("RMBCropBillboardBatch: Failed to create billboard material.");
            return;
        }

        // Create and position billboards
        billboards = new List<GameObject>();
        List<Vector3> billboardPositions = GenerateBillboardPositions();
        AddBillboardsToBatch(billboardPositions);

        Debug.Log("RMBCropBillboardBatch: Billboard batch initialization complete.");
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

        for (int x = -10; x <= 10; x++)
        {
            for (int z = -10; z <= 10; z++)
            {
                float offsetX = Random.Range(-noiseAmount, noiseAmount);
                float offsetZ = Random.Range(-noiseAmount, noiseAmount);
                Vector3 position = new Vector3(x * gridSpacing + offsetX, 0, z * gridSpacing + offsetZ);
                positions.Add(position);
            }
        }

        Debug.Log("RMBCropBillboardBatch: Billboard positions generated.");
        return positions;
    }

    void AddBillboardsToBatch(List<Vector3> billboardPositions)
    {
        Debug.Log("RMBCropBillboardBatch: Adding billboards to batch...");
        foreach (Vector3 position in billboardPositions)
        {
            Vector3 relativePosition = transform.position + position;
            GameObject billboard = CreateBillboard(relativePosition);
            if (billboard != null)
            {
                billboards.Add(billboard);
            }
        }
        Debug.Log("RMBCropBillboardBatch: Billboards added to batch.");
    }

    GameObject CreateBillboard(Vector3 position)
    {
        Debug.Log($"RMBCropBillboardBatch: Creating billboard at position {position}...");
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
            renderer.material = billboardMaterial;
        }

        billboard.transform.SetParent(transform); // Ensure the billboard is a child of the current GameObject
        billboard.AddComponent<FaceCamera>(); // Add the FaceCamera script to the billboard

        Debug.Log("RMBCropBillboardBatch: Billboard created.");
        return billboard;
    }

    void OnDestroy()
    {
        Debug.Log("RMBCropBillboardBatch: OnDestroy called.");
        // Clean up resources when the game object is destroyed
        if (modGameObject != null)
        {
            Destroy(modGameObject);
            modGameObject = null;
        }

        // Destroy all created billboards
        if (billboards != null)
        {
            foreach (GameObject billboard in billboards)
            {
                Destroy(billboard);
            }
            billboards.Clear();
        }

        Debug.Log("RMBCropBillboardBatch: Resources cleaned up.");
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

