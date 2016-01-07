/*author : Narendra
 * A Unity Editor that allows you to replace scene materials with a another material given by user
 Limitations:
 1. cache doesn't refresh when user Undos Editor events
 * */
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// stores stats of renderers
/// </summary>
public class RendererInfo
{
    public Material material;
    public Material[] sharedMaterials;
    public Renderer renderer;
    public int matIndex = -1;
    public bool isSelected = false;
}

/// <summary>
/// A Unity Editor that allow you to replace scene materials with a another material given by user
/// </summary>
public class ResetMaterial : EditorWindow 
{
    //global variables
    Material SourceMat = null;
    List<RendererInfo> SceneRenderers = new List<RendererInfo>();
    Renderer[] scene_renders;
    bool resetAll = false;
    Vector2 rendererListScrollPos = new Vector2(0, 0);
    //Menu shortcut
	[MenuItem ("NY_Tools/Reset Material")]
    //Initialize Editor window
    static void Init()
    {
        ResetMaterial window = (ResetMaterial)EditorWindow.GetWindow(typeof(ResetMaterial));
        window.minSize = new Vector2(455, 300);
		//get cache of the scene renderers when window starts
        window.LoadRenderers();
    }
	
	//Editor Windows's GUI update
    void OnGUI()
    {
		
        try//check for exceptions whenever user changes scenes
        {
			//Main UI
            SourceMat = (Material)EditorGUI.ObjectField(new Rect(0, 2, 430, 20), "Source Material: ", SourceMat, typeof(Material));
            GUILayout.Space(30);
            if (GUILayout.Button("Get Scene Materials")) LoadRenderers();//get renderer cache
            ListRenderers();//build UI based on cache
            
        }
        catch (MissingReferenceException ex)//If Exceptions found, reset all global variables
        {
            scene_renders = null;
            SceneRenderers.Clear();
            SourceMat = null;
            Init();
            Debug.Log(ex.GetType() + " Restarting UI");
        }

        if (GUILayout.Button("Assign Material to selection")) AssignMaterial();//assign variables
    }

	//A GUI builder that loads all the renderers from the caches and also an option to select gameObject associated with them
    void ListRenderers()
    {
        //create a scroll view
        rendererListScrollPos = EditorGUILayout.BeginScrollView(rendererListScrollPos);
		//loop through #SceneRenderers cache
        foreach(RendererInfo mRendererInfo in SceneRenderers)
        {
            GUILayout.BeginHorizontal();
			//if a renderer is selected, pass it to the cache
            mRendererInfo.isSelected = GUILayout.Toggle(mRendererInfo.isSelected, "Reset");
			//display renderer name along with its material
            GUILayout.Label(mRendererInfo.renderer.name + " | " + mRendererInfo.material.name, GUILayout.Width(200));
			//if user presses #Select, select the gameObject and focus on it
            if (GUILayout.Button("Select", GUILayout.Width(120)))
            {
                try//catch exception if user deletes gameObject
                {
                    SelectObject(mRendererInfo.renderer.gameObject);
                }
                catch (MissingReferenceException ex)
                {
                    Debug.LogWarning(ex.GetType() + " : Some of the Meshes are missing from the available cache, Please Refesh");
                }
            }
			//update cache
            if (mRendererInfo.isSelected) SceneRenderers[SceneRenderers.IndexOf(mRendererInfo)].isSelected = true;
            GUILayout.EndHorizontal();
        }
		//an option to select all renderers
        if(SceneRenderers.Count > 0) resetAll = GUILayout.Toggle(resetAll, "Reset All");
        EditorGUILayout.EndScrollView();
    }

	//creates a cache of scene renderers
    void LoadRenderers()
    {
		//empty the available cache
        SceneRenderers.Clear();
		//get the available renderers from scene
        scene_renders = FindObjects<Renderer>();
		//loop through the renderers and their materials
        foreach (Renderer mRenderer in scene_renders)
        {
            for (int i = 0; i < mRenderer.sharedMaterials.Length; i++)
            {
                RendererInfo mRendererInfo = new RendererInfo();
                mRendererInfo.matIndex = i;
                mRendererInfo.material = mRenderer.sharedMaterials[i];
                mRendererInfo.sharedMaterials = mRenderer.sharedMaterials;
                mRendererInfo.renderer = mRenderer;
				//add it to the cache
                SceneRenderers.Add(mRendererInfo);

            }
        }

    }
	
	//Assigns user's material to the selected or all the renderers
    void AssignMaterial()
    {
        int selected = 0;
		//register an undo state before the process starts
        if (SourceMat && SceneRenderers.Count > 0) Undo.RegisterCompleteObjectUndo(scene_renders, "ResetMaterial_01");
        //loo through the cache and assign material
        for (int i = 0; i < SceneRenderers.Count;i++ )
        {
            if(SceneRenderers[i].isSelected || resetAll)
            {
                selected++;
                if(SourceMat!=null)
                {
                    if(SceneRenderers[i].matIndex ==0)
                    {
                        SceneRenderers[i].renderer.sharedMaterial = SourceMat;//assign material
                        SceneRenderers[i].sharedMaterials[0] = SourceMat;//update cache.this is very important
                    }
                    else
                    {
                        SceneRenderers[i].sharedMaterials[SceneRenderers[i].matIndex] = SourceMat;//assign material
                        SceneRenderers[i].renderer.sharedMaterials = SceneRenderers[i].sharedMaterials;//update cache.this is very important
                    }
                    UpdateSceneRenderers(SceneRenderers[i]);//update renderer cache
                }
                //Debug.Log(SceneRenderers[i].renderer.name + " - " + SceneRenderers[i].matIndex);
            }
        }
        
        
    }
	
	//this is a place-holder function to check for duplicate storage
    RendererInfo CheckRendererInfo(Material mMaterial)
    {
        foreach(RendererInfo mRenderInfo in SceneRenderers)
        {
            if (mRenderInfo.material == mMaterial) return mRenderInfo;
        }
        return null;
    }

	//updates individual renderer cache object
    void UpdateSceneRenderers(RendererInfo refer)
    {
        foreach(RendererInfo item in SceneRenderers)
        {
            if(item.renderer == refer.renderer)
            {
                item.sharedMaterials = refer.sharedMaterials;
            }
        }
    }

	//returns scene's components array based on the type given
    private T[] FindObjects<T>() where T : Object
    {
        return (T[])FindObjectsOfType(typeof(T));
    }

	//select a gameObject in the editor and focus on it
    void SelectObject(Object selectedObject)
    {
        try
        {
            Selection.activeObject = selectedObject;
            SceneView.FrameLastActiveSceneView();
        }
        catch (UnityException ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }
}
