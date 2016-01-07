/*
author : Narendra Yenugula
brief :This tools grabs textures from models, duplicates them, compresses its size and creates a prefab with new textures created. For use in Unity3D.
*/
using UnityEngine;
using UnityEditor;
using System.Collections;

public class TextureCompressor_UI : EditorWindow 
{
    // Add menu item to the Menu Bar    
    [MenuItem("HB_Studios/Compress UI")]
    // Initializing the window
	static void Init()
    {
        TextureCompressor_UI window = (TextureCompressor_UI)EditorWindow.GetWindow(typeof(TextureCompressor_UI));
        window.title = "Texture Compressor";
        //sizing allows to trim for unnecessary space
        window.maxSize = new Vector2(460, 205);
        window.minSize = window.maxSize;
        window.Show();
    }

    // attributes
    bool isValid = false;
    string postFix = "_low";
    bool duplicateMats = true;

    string[] texIDs = new string[] { "_MainTex", "_BumpMap" };
    Object[] sel;
    int texSizeDivider = 50;

    // Gui update
    private void OnGUI()
    {
        // if the selection is valid
        if(isValid)
        {
            //postfix for the assets created. default #postFix is "_low"
            postFix = EditorGUILayout.TextField("Postfix for Assets:", postFix);
            //check whether to duplicate materials
            EditorGUILayout.BeginHorizontal();
            duplicateMats = EditorGUILayout.Toggle("Duplicate Materials", duplicateMats);
            EditorGUILayout.EndHorizontal();
            //get the percentage to reduce the textures. default is 50
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Reduce Texture by Percentage:");
            texSizeDivider = EditorGUILayout.IntSlider(texSizeDivider, 1, 100);
            EditorGUILayout.EndHorizontal();
            //Compress
            if(GUILayout.Button("Compress Textures"))
            {
                //check how the process went
                if (ProcessSelection()) { this.Close(); }
                else { Debug.LogError("Process failed");isValid = false; }
            }
        }
        else
        {
            //Get the selection and change #isValid
            EditorGUILayout.LabelField("Select models in the Project window");
            if(GUILayout.Button("Get Selection from Project"))
            {
                sel = Selection.GetFiltered(typeof(GameObject), SelectionMode.Assets);
                if (sel.Length > 0)
                    isValid = true;
            }
        }
    }

    //process selection here
    public bool ProcessSelection()
    {
        sel = Selection.GetFiltered(typeof(GameObject), SelectionMode.Assets);
        //Debug.Log(sel.Length + " object(s) selected");

        //store processed materials and textures so that the program doesn't process again that is already processed 
        System.Collections.Generic.List<string> processedMats = new System.Collections.Generic.List<string>();
        System.Collections.Generic.List<string> processedTexs = new System.Collections.Generic.List<string>();

        //Handles multiple gameObjects
        foreach (GameObject ob in sel)
        {
            //get the path of the asset
            string path = AssetDatabase.GetAssetPath(ob);
            //Debug.Log(ob.name + " | " + path + " | " + AssetDatabase.Contains(ob) + " | " + AssetDatabase.IsMainAsset(ob));

            //there is a chance of getting proper data when the asset is in hierarchy and also the renders are enabled in the hierarchy
            GameObject go = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath(path, typeof(GameObject))) as GameObject;
            Renderer[] renders;
            //get renders from the @go
            try
            {
                renders = go.GetComponentsInChildren<Renderer>();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e.Message);
                Debug.LogWarning("Select Models from Project window");
                return false;
            }
            //Renderer[] renders = go.GetComponentsInChildren<Renderer>();
            Debug.Log("Renders/skinnedMeshRenders found = " + renders.Length);
            //process through all the @renders
            foreach (Renderer rnd in renders)
            {
                //get shared materials and process them
                Material[] mats = rnd.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    //get path and guid from the material. guid contains unique ID assigned by Unity
                    string pathMat = AssetDatabase.GetAssetPath(mats[i].GetInstanceID());
                    string guidMat = AssetDatabase.AssetPathToGUID(pathMat);
                    //checking if the material is already processed
                    if (!processedMats.Contains(guidMat))
                    {
                        processedMats.Add(guidMat);
                        //make a copy of material
                        if(duplicateMats)
                            mats[i] = CopyAssetExtnd(pathMat, mats[i].name + postFix + ".", typeof(Material)) as Material;
                        //cycle through all the textures using @texIDs
                        foreach (string texID in texIDs)
                        {
                            //checking if the material has the property or not
                            if (mats[i].HasProperty(texID))
                            {
                                //get the texture to make copy of it
                                Texture2D tex = mats[i].GetTexture(texID) as Texture2D;
                                //check if the texture is assigned to material or not, if it is not assigned #moveOn
                                if (tex == null) continue;
                                //get path and guid for textures also
                                string pathTex = AssetDatabase.GetAssetPath(tex.GetInstanceID());
                                string guidTex = AssetDatabase.AssetPathToGUID(pathTex);
                                //check for existing processed textures
                                if (!processedTexs.Contains(guidTex))
                                {
                                    //create a copy of texture and also compress its size
                                    tex = CopyTexture(pathTex, tex.name + postFix + ".", typeof(Texture2D), (int)((float)tex.width * (float)texSizeDivider/100)) as Texture2D;
                                    //assign the new compressed texture to the material
                                    mats[i].SetTexture(texID, tex);
                                }
                            }
                        }
                    }
                }
                //assign the material to renderer
                rnd.sharedMaterials = mats;
            }
            //create a new prefab to save the low_settings of selected prefab/model. NOTE: Models in the projects window doesn't allow you to replace materials
            string newPath = GetDirectory(path) + go.name + ".prefab";
            //OverWiting on existing asset if one available
            AssetDatabase.DeleteAsset(newPath);
            PrefabUtility.CreatePrefab(newPath, go);
            //destroy the created gameObject @go in the start
            DestroyImmediate(go);
            //create a gameObject in the hierarchy using the new prefab
            GameObject low = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath(newPath, typeof(GameObject))) as GameObject;
        }
        return true;
    }

    //get directory of the asset
    string GetDirectory(string path_)
    {
        return path_.Remove(path_.LastIndexOf('/') + 1);
    }

    //get extension of the asset
    string GetExtension(string path_)
    {
        return path_.Substring(path_.LastIndexOf('.') + 1);
    }

    //get name of the asset. used sometimes when the AssetDatabase returns null string
    string GetName(string path_)
    {
        string name = path_.Replace(GetDirectory(path_), "");
        name = name.Replace("." + GetExtension(name), "");
        return name;
    }

    //this is an extended version of @AssetDaabase.CopyAsset
    Object CopyAssetExtnd(string path_, string newName_, System.Type type_)
    {
        //create a string for @newPath
        string newPath = GetDirectory(path_) + newName_ + GetExtension(path_);
        //OverWiting on existing asset if one available
        AssetDatabase.DeleteAsset(newPath);
        //create a copy of the asset and import/reload it
        AssetDatabase.CopyAsset(path_, newPath);
        AssetDatabase.ImportAsset(newPath);
        //pass the asset back
        return AssetDatabase.LoadAssetAtPath(newPath, type_);
    }

    //this is an extended version of @CopyAssetExtnd. specifically used for textures
    Texture2D CopyTexture(string path_, string newName_, System.Type type_, int maxSize_)
    {
        //create a string for @newPath
        string newPath = GetDirectory(path_) + newName_ + GetExtension(path_);
        //OverWiting on existing asset if one available
        AssetDatabase.DeleteAsset(newPath);
        //create a copy of the asset
        AssetDatabase.CopyAsset(path_, newPath);
        //resize the texture with given @maxSize_
        TextureImporter texImp = TextureImporter.GetAtPath(newPath) as TextureImporter;
        texImp.maxTextureSize = maxSize_;
        //import/reload the asset
        AssetDatabase.ImportAsset(newPath);
        //pass the asset back
        return AssetDatabase.LoadAssetAtPath(newPath, type_) as Texture2D;
    }

}