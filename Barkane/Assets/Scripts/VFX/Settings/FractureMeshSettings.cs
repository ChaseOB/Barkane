using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Barkane/Settings/Fracture Mesh Settings")]
public class FractureMeshSettings : ScriptableObject
{
    [Header("Tile Face Geometry")]
    [Range(0, 0.5f), Tooltip("A large margin means the pivots of the triangle must be very close to the middle of the tile, be careful setting it high if triangleArea is low")]
    public float margin;
    [Range(0, 0.5f), Tooltip("How much each vertex can go up/down on the distorted tile, be careful for intersection/floating artifacts when set too high")]
    public float height;
    [Range(0, 1), Tooltip("A small triangle area means the generator can create tiles with a very thin/small triangle section, meaning there is more distribution and less even-looking")]
    public float mainTriangleArea;
    [Range(0, 0.2f), Tooltip("Prevent all triangles from being too small/narrow")]
    public float allTriangleArea;
    [Tooltip("Level of detail of the distorted normal, multiple of 8 (or some multiple of the BATCH_SIZE variable (see FractureMeshSetting code)")]
    public int resolution;

    [Range(0, 20), Tooltip("Blurring fixes sharp changes in the render texture due to mismatch between render texture and screen coordinates around edges")]
    public int blurLoop;

    // CAREFUL: change the batch size in CrumbleNorm.compute as well if you are changing this value
    // changing it might impact compatibility on different platforms
    private readonly static int BATCH_SIZE = 8;

    [Header("Sprinkles")]
    [Range(0, 50)]
    public int sprinkleCount;
    [Range(0, 50)]
    public int sprinkleBonus;
    [Range(0, 0.1f)]
    public float sprinkleElevation;

    public int groupSize => resolution / BATCH_SIZE;
}
