/*author : Narendra
 * A Unity Editor that allows you to view materials, Textures, Meshes and Triangle count of a scene and camera
 * */
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// stores stats of textures
/// </summary>
public class TextureStats
{
	public Texture texture;
    public List<Object> FoundInMaterials = new List<Object>();
    public List<Object> FoundInRenderers = new List<Object>();
    public bool isMainCamera = false;
	public TextureStats()
	{
		
	}
};

/// <summary>
/// stores stats of Materials
/// </summary>
public class MaterialStats
{
	
	public Material material;
    public bool isMainCamera = false;

    public List<Renderer> FoundInRenderers = new List<Renderer>();
    public MaterialStats()
    {

    }
};

/// <summary>
/// stores stats of Meshes
/// </summary>
public class MeshStats
{
	
	public Mesh mesh;
    public bool isMainCamera = false;

    public List<MeshFilter> FoundInMeshFilters = new List<MeshFilter>();
    public List<SkinnedMeshRenderer> FoundInSkinnedMeshRenderer = new List<SkinnedMeshRenderer>();
    
	public MeshStats()
	{
		
	}
};

/// <summary>
/// A Unity Editor that allows you to view materials, Textures, Meshes and Triangle count of a scene and camera
/// </summary>
public class ArtStats : EditorWindow 
{
	//global variables
    string[] inspectToolbarStrings = { "Materials", "Textures", "Meshes" };
	//use enum for declaring stats type
    enum StatsType
    {
        Materials, Textures, Meshes
    };

    bool useMainCamera = false;
    Camera cameraMask = (Camera.main != null) ? Camera.main : Camera.current;
    

    StatsType CurrentStatType = StatsType.Materials;
	
	float ThumbnailWidth=40;
	float ThumbnailHeight=40;

    List<TextureStats> SceneTextures = new List<TextureStats>();
    List<MaterialStats> SceneMaterials = new List<MaterialStats>();
    List<MeshStats> SceneMeshes = new List<MeshStats>();

    Vector2 textureScrollListPos = new Vector2(0, 0);
    Vector2 materialScrollListPos = new Vector2(0, 0);
    Vector2 meshScrollListPos = new Vector2(0, 0);

    int CameraMaterials = 0;
    int CameraMeshes = 0;
    int CameraTextures = 0;
    int CameraMeshTriangles = 0;
    int TotalMeshTriangles = 0;


    bool ctrlPressed = false;

    static int MinWidth = 455;
	//Menu shortcut
    [MenuItem ("NY_Tools/Art Stats")]
    static void Init ()
	{
        ArtStats window = (ArtStats)EditorWindow.GetWindow(typeof(ArtStats));
        window.useMainCamera = false;
		//get cache of the scene data when window starts
        window.LoadStats();

        window.minSize = new Vector2(MinWidth, 300);
    }
    
	//Editor Windows's GUI update
    void OnGUI ()
	{
        //Main UI
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh Scene")) { useMainCamera = false; LoadStats();}//load cache from scene
        if (GUILayout.Button("Refresh Camera")) { useMainCamera = true; LoadStats();}//load cache usin camera as mask
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
		//show counts of graphic assets found in the scene or camera
        GUILayout.Label("Materials " + (useMainCamera ? CameraMaterials : SceneMaterials.Count));
        GUILayout.Label("Textures " + (useMainCamera ? CameraTextures : SceneTextures.Count));
        GUILayout.Label("Meshes " + (useMainCamera ? CameraMeshes : SceneMeshes.Count) + " - " + (useMainCamera ? CameraMeshTriangles: TotalMeshTriangles) + " Tris");
        GUILayout.EndHorizontal();
        CurrentStatType = (StatsType)GUILayout.Toolbar((int)CurrentStatType, inspectToolbarStrings);

        try//check for exceptions whenever user changes scenes
        {
            switch (CurrentStatType)//load UI list based on user selection
            {
                case StatsType.Textures:
                    ListTextures();
                    break;
                case StatsType.Materials:
                    ListMaterials();
                    break;
                case StatsType.Meshes:
                    ListMeshes();
                    break;
            }
        }
        catch(MissingReferenceException ex)//If Exceptions found, reset all global variables
        {
            Init();
            Debug.Log(ex.GetType() + " Restarting UI");
        }

        GUILayout.Label( useMainCamera ? "*Using Camera" : "*Using Scene");//show a label to note user what algorithm was being used
	}

	//select an asset in the project window
	void SelectObject(Object selectedObject)
	{
        try
        {
            Selection.activeObject = selectedObject;
        }
        catch (UnityException ex)
        {
            Debug.LogWarning(ex.Message);
        }
	}
	
	//select gameObjects/assets in the editor/project window and focus on it
	void SelectObjects(List<Object> selectedObjects)
	{
        try
        {
            Selection.objects = selectedObjects.ToArray();
            SceneView.FrameLastActiveSceneView();
        }
        catch (UnityException ex)
        {
            Debug.LogWarning(ex.GetType() + " : Some of the Meshes are missing from the available cache, Please Refesh");
        }
	}
	
	//A GUI builder that loads all the textures from the cache and also an option to select gameObjects/materials associated with them
	void ListTextures()
	{
		//create a scroll view
		textureScrollListPos = EditorGUILayout.BeginScrollView(textureScrollListPos);
        CameraTextures = 0;
		//loop through #SceneTextures cache
		foreach (TextureStats mStats in SceneTextures)
		{
			//if user uses camera-mask then check for #isMainCamera and filter textures
            if (useMainCamera && !mStats.isMainCamera) continue;
            CameraTextures++;
			GUILayout.BeginHorizontal ();
			//show texture as a thumbail
			GUILayout.Box(mStats.texture, GUILayout.Width(ThumbnailWidth), GUILayout.Height(ThumbnailHeight));
			
			if(GUILayout.Button(mStats.texture.name,GUILayout.Width(150)))
			{
				SelectObject(mStats.texture);
			}
			//show texture width and height
			string sizeLabel=""+mStats.texture.width+"x"+mStats.texture.height;
		
			GUILayout.Label (sizeLabel,GUILayout.Width(120));
					
			if(GUILayout.Button(mStats.FoundInMaterials.Count+" Mat",GUILayout.Width(50)))
			{
				SelectObjects(mStats.FoundInMaterials);
			}
			//select gameObjects if user wants to know where the texture is being used
			List<Object> FoundObjects = new List<Object>();
			foreach (Renderer renderer in mStats.FoundInRenderers) FoundObjects.Add(renderer.gameObject);
			if (GUILayout.Button(FoundObjects.Count+" GO",GUILayout.Width(50)))
			{
				SelectObjects(new List<Object>(FoundObjects));
			}
			
			GUILayout.EndHorizontal();	
		}
		//An option to select all textures
		if (SceneTextures.Count>0)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Box(" ",GUILayout.Width(ThumbnailWidth),GUILayout.Height(ThumbnailHeight));
			
			if(GUILayout.Button("Select All",GUILayout.Width(150)))
			{
				List<Object> AllTextures=new List<Object>();
				foreach (TextureStats tDetails in SceneTextures) AllTextures.Add(tDetails.texture);
				SelectObjects(AllTextures);
			}
			GUILayout.EndHorizontal();
		}
		EditorGUILayout.EndScrollView();
    }
	//A GUI builder that loads all the Materials from the cache and also an option to select GameObjects associated with them
	void ListMaterials()
	{
		//create a scroll view
		materialScrollListPos = EditorGUILayout.BeginScrollView(materialScrollListPos);
        CameraMaterials = 0;
		//loop through #SceneMaterials cache
		foreach (MaterialStats mStats in SceneMaterials)
		{
			//if user uses camera-mask then check for #isMainCamera and filter materials
            if (useMainCamera && !mStats.isMainCamera) continue;
            CameraMaterials++;
			if (mStats.material!=null)
			{
				GUILayout.BeginHorizontal ();
				//show texture of material or "n/a" as a thumbail
				if (mStats.material.mainTexture!=null) GUILayout.Box(mStats.material.mainTexture, GUILayout.Width(ThumbnailWidth), GUILayout.Height(ThumbnailHeight));
				else	
				{
					GUILayout.Box("n/a",GUILayout.Width(ThumbnailWidth),GUILayout.Height(ThumbnailHeight));
				}
				//select material in the project-window
				if(GUILayout.Button(mStats.material.name,GUILayout.Width(150)))
				{
					SelectObject(mStats.material);
				}
				//Show shader of material
				string shaderLabel = mStats.material.shader != null ? mStats.material.shader.name : "no shader";
				GUILayout.Label (shaderLabel, GUILayout.Width(200));
				//select gameObjects if user wants to know where the materials is being used
				if(GUILayout.Button(mStats.FoundInRenderers.Count+" GO",GUILayout.Width(50)))
				{
					List<Object> FoundObjects=new List<Object>();
					foreach (Renderer renderer in mStats.FoundInRenderers) FoundObjects.Add(renderer.gameObject);
					SelectObjects(FoundObjects);
				}
				
				
				GUILayout.EndHorizontal();	
			}
		}
		EditorGUILayout.EndScrollView();		
    }
	
	//A GUI builder that loads all the base-meshes from the cache and also an option to select Normal/Skinned GameObjects associated with them
	void ListMeshes()
	{
		//create a scroll view
		meshScrollListPos = EditorGUILayout.BeginScrollView(meshScrollListPos);
        CameraMeshes = 0;
		//loop through #SceneMeshes cache
		foreach (MeshStats mStats in SceneMeshes)
		{			
			if (mStats.mesh!=null)
			{
				//if user uses camera-mask then check for #isMainCamera and filter meshes
                if (useMainCamera && !mStats.isMainCamera) continue;
                CameraMeshes++;
				GUILayout.BeginHorizontal ();
				//select base-mesh in the project-window
				if(GUILayout.Button(mStats.mesh.name,GUILayout.Width(150)))
				{
					SelectObject(mStats.mesh);
				}
                string sizeLabel = "" + mStats.mesh.triangles.Length / 3 + " Tris";
				
				GUILayout.Label (sizeLabel,GUILayout.Width(100));
				
				//select gameObjects if user wants to know where the meshes is being used
				if(GUILayout.Button(mStats.FoundInMeshFilters.Count + " GO",GUILayout.Width(50)))
				{
                    List<Object> FoundObjects = new List<Object>();
                    try
                    {
                        foreach (MeshFilter meshFilter in mStats.FoundInMeshFilters) FoundObjects.Add(meshFilter.gameObject);
                    }
                    catch (MissingReferenceException ex)
                    {
                        Debug.LogWarning(ex.GetType() + " : Some of the Meshes are missing from the available cache, Please Refesh");
                    }
                    SelectObjects(FoundObjects);
				}
				//select gameObjects if user wants to know where the meshes is being used
				if(GUILayout.Button(mStats.FoundInSkinnedMeshRenderer.Count + " SKIN",GUILayout.Width(50)))
				{
					List<Object> FoundObjects=new List<Object>();
                    try
                    {
                        foreach (SkinnedMeshRenderer skinnedMeshRenderer in mStats.FoundInSkinnedMeshRenderer) FoundObjects.Add(skinnedMeshRenderer.gameObject);
                    }
                    catch (MissingReferenceException ex)
                    {
                        Debug.LogWarning(ex.GetType() + " : Some of the Meshes are missing from the available cache, Please Refesh");
                    }
					SelectObjects(FoundObjects);
				}
				
				
				GUILayout.EndHorizontal();	
			}
		}
		EditorGUILayout.EndScrollView();		
    }
	
	//checks for duplicate cahce-object of #SceneTextures
	TextureStats CheckTextureStats(Texture mTexture)
	{
		foreach (TextureStats mTextureStats in SceneTextures)
		{
			if (mTextureStats.texture==mTexture) return mTextureStats;
		}
		return null;
		
	}
	
	//checks for duplicate cahce-object of #SceneMaterials
	MaterialStats CheckMaterialStats(Material mMaterial)
	{
		foreach (MaterialStats mMaterialStats in SceneMaterials)
		{
			if (mMaterialStats.material==mMaterial) return mMaterialStats;

		}
		return null;
		
	}
	
	//checks for duplicate cahce-object of #SceneMeshes
	MeshStats CheckMeshStats(Mesh mMesh)
	{
		foreach (MeshStats mMeshStats in SceneMeshes)
		{
			if (mMeshStats.mesh==mMesh) return mMeshStats;
		}
		return null;
		
	}

	//create cache for #SceneMaterials, #SceneMeshes, #SceneTextures
	void LoadStats()
	{
		//empty the available cache
		SceneTextures.Clear();
		SceneMaterials.Clear();
		SceneMeshes.Clear();
		//get camera again, assigning just in case if user changes/loads scene
        cameraMask = (Camera.main != null) ? Camera.main : Camera.current;
		//get the available renderers from scene
		Renderer[] Renderers = FindObjects<Renderer>();
		//loop through the renderers and their materials
		foreach (Renderer mRenderer in Renderers)
		{
            bool mIsMainCamera = mRenderer.IsVisibleFrom(cameraMask);
			foreach (Material mMaterial in mRenderer.sharedMaterials)
			{

				MaterialStats mMaterialStats = CheckMaterialStats(mMaterial);
				if (mMaterialStats == null)
				{
					mMaterialStats = new MaterialStats();
					mMaterialStats.material = mMaterial;
                    mMaterialStats.isMainCamera = mIsMainCamera;
					//add it to the cache
					SceneMaterials.Add(mMaterialStats);
				}
                else if(mIsMainCamera)//update available cache
                {
                    mMaterialStats.isMainCamera = mIsMainCamera;
                    int id = SceneMaterials.IndexOf(mMaterialStats);
                    SceneMaterials[id] = mMaterialStats;
                }
				//filter for camera-mask
                if (useMainCamera && !mIsMainCamera) continue;
				mMaterialStats.FoundInRenderers.Add(mRenderer);//add it to the cache
			}
			//get texture stats from renderer
			if (mRenderer is SpriteRenderer)
			{
				SpriteRenderer mSpriteRenderer = (SpriteRenderer)mRenderer;

				if (mSpriteRenderer.sprite != null)
				{
					TextureStats mSpriteTextureStats = GetTextureStats(mSpriteRenderer.sprite.texture, mRenderer);
					if (!SceneTextures.Contains(mSpriteTextureStats))
					{
                        mSpriteTextureStats.isMainCamera = mIsMainCamera;
						//add it to the cache
						SceneTextures.Add(mSpriteTextureStats);
					}
                    else//update available cache
                    {
                        mSpriteTextureStats.isMainCamera = mIsMainCamera;
                        int id = SceneTextures.IndexOf(mSpriteTextureStats);
                        SceneTextures[id] = mSpriteTextureStats;
                    }
				}
			}
		}
		//loop through #SceneMaterials and textures to its cache
		foreach (MaterialStats mMaterialStats in SceneMaterials)
		{
			Material mMaterial = mMaterialStats.material;
			if (mMaterial != null)
			{
				var dependencies = EditorUtility.CollectDependencies(new UnityEngine.Object[] { mMaterial });
				foreach (Object obj in dependencies)
				{
					if (obj is Texture)
					{
						Texture mTexture = obj as Texture;
						var mTextureStats = GetTextureStats(mTexture, mMaterial, mMaterialStats);
                        if (mMaterialStats.isMainCamera) mTextureStats.isMainCamera = true;
						SceneTextures.Add(mTextureStats);//add it to the cache
					}
				}

				//if the texture was downloaded, it won't be included in the editor dependencies
				if (mMaterial.mainTexture != null && !dependencies.Contains(mMaterial.mainTexture))
				{
					var mTextureStats = GetTextureStats(mMaterial.mainTexture, mMaterial, mMaterialStats);
                    if (mMaterialStats.isMainCamera) mTextureStats.isMainCamera = true;
					SceneTextures.Add(mTextureStats);//add it to the cache
				}
			}
		}

		//get the available meshFilters from scene
		MeshFilter[] meshFilters = FindObjects<MeshFilter>();
		//loop through the meshFilters
		foreach (MeshFilter mMeshFilter in meshFilters)
		{
            bool mIsMainCamera = mMeshFilter.renderer.IsVisibleFrom(cameraMask);
            
			Mesh mMesh = mMeshFilter.sharedMesh;
			if (mMesh != null)
			{
				MeshStats mMeshStats = CheckMeshStats(mMesh);
				if (mMeshStats == null)
				{
					mMeshStats = new MeshStats();
					mMeshStats.mesh = mMesh;
                    mMeshStats.isMainCamera = mIsMainCamera;
					SceneMeshes.Add(mMeshStats);//add it to the cache
				}
                else if(mIsMainCamera)//update available cache
                {
                    mMeshStats.isMainCamera = mIsMainCamera;
                    int id = SceneMeshes.IndexOf(mMeshStats);
                    SceneMeshes[id] = mMeshStats;
                }
				//filter for camera-mask
                if (useMainCamera && !mIsMainCamera) continue;
				mMeshStats.FoundInMeshFilters.Add(mMeshFilter);//add it to the cache
			}
		}
		
		//get the available skinnedMeshRenderers from scene
		SkinnedMeshRenderer[] skinnedMeshRenderers = FindObjects<SkinnedMeshRenderer>();
		//loop through the skinnedMeshRenderers
		foreach (SkinnedMeshRenderer mSkinnedMeshRenderer in skinnedMeshRenderers)
		{
            bool mIsMainCamera = mSkinnedMeshRenderer.IsVisibleFrom(cameraMask);
			Mesh mMesh = mSkinnedMeshRenderer.sharedMesh;
			if (mMesh != null)
			{
				MeshStats mMeshStats = CheckMeshStats(mMesh);
				if (mMeshStats == null)
				{
					mMeshStats = new MeshStats();
					mMeshStats.mesh = mMesh;
                    mMeshStats.isMainCamera = mIsMainCamera;
					SceneMeshes.Add(mMeshStats);//add it to the cache
				}
                else if(mIsMainCamera)//update available cache
                {
                    mMeshStats.isMainCamera = mIsMainCamera;
                    int id = SceneMeshes.IndexOf(mMeshStats);
                    SceneMeshes[id] = mMeshStats;
                }
				//filter for camera-mask
                if (useMainCamera && !mIsMainCamera) continue;
				mMeshStats.FoundInSkinnedMeshRenderer.Add(mSkinnedMeshRenderer);
			}
		}

//        
		//calulate mesh count
		TotalMeshTriangles = 0;
        CameraMeshTriangles = 0;
        CameraMeshes = 0;
        foreach (MeshStats mMeshStats in SceneMeshes)
        {
            int tris = (mMeshStats.mesh.triangles.Length / 3) * (mMeshStats.FoundInMeshFilters.Count + mMeshStats.FoundInSkinnedMeshRenderer.Count);
            TotalMeshTriangles += tris;
            if (mMeshStats.isMainCamera)
            {
                CameraMeshTriangles += tris;
                CameraMeshes++;
            }
        }
		//calulate materials count
        CameraMaterials = 0;
        foreach (MaterialStats mMaterialStats in SceneMaterials)
        {
            if (mMaterialStats.isMainCamera) CameraMaterials++;
        }
		//calulate textures count
        CameraTextures = 0;
        foreach(TextureStats mtextureStas in SceneTextures)
        {
            if (mtextureStas.isMainCamera) CameraTextures++;
        }

		// Sort by size, descending
		SceneMeshes.Sort(delegate(MeshStats stats1, MeshStats stats2) { return stats2.mesh.vertexCount - stats1.mesh.vertexCount; });

	}

	//returns scene's components array based on the type given
	private T[] FindObjects<T>() where T : Object
	{
		return (T[])FindObjectsOfType(typeof(T));
	}

	//Add material and renderer to #SceneTextures object
	private TextureStats GetTextureStats(Texture mTexture, Material mMaterial, MaterialStats mMaterialStats)
	{
		TextureStats mTextureStats = GetTextureStats(mTexture);

		mTextureStats.FoundInMaterials.Add(mMaterial);
		foreach (Renderer renderer in mMaterialStats.FoundInRenderers)
		{
			if (!mTextureStats.FoundInRenderers.Contains(renderer)) mTextureStats.FoundInRenderers.Add(renderer);
		}
		return mTextureStats;
	}

	//Add renderer to #SceneTextures object
	private TextureStats GetTextureStats(Texture mTexture, Renderer renderer)
	{
		TextureStats mTextureStats = GetTextureStats(mTexture);

		mTextureStats.FoundInRenderers.Add(renderer);
		return mTextureStats;
	}

	//check for duplicate #SceneTextures object. if not found create a new #SceneMaterials object
	private TextureStats GetTextureStats(Texture mTexture)
	{
		TextureStats mTextureStats = CheckTextureStats(mTexture);
		if (mTextureStats == null)
		{
			mTextureStats = new TextureStats();
			mTextureStats.texture = mTexture;
		}

		return mTextureStats;
	}
	
}
