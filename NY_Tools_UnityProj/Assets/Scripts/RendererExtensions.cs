/*Renderer Extension.cs
 * A Renderer extension to know whether a renderer is in Camera Clipping
 * */
using UnityEngine;

/// <summary>
/// UnityEngine.Renderer extension
/// </summary>
public static class RendererExtensions
{
    /// <summary>
    /// Calucates if a renderer is in Camera Clipping
    /// </summary>
    /// <param name="renderer">renderer to check if it falls in Camera clippin</param>
    /// <param name="camera">camera to use for clipping</param>
    /// <returns>type bool</returns>
    public static bool IsVisibleFrom(this Renderer renderer, Camera camera)
    {
        if (camera == null) return false;
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }
}