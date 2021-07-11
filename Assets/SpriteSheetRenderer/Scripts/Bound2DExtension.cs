using ECSSpriteSheetAnimation;
using Unity.Mathematics;
using UnityEngine;

public static class Bound2DExtension
{
    public static bool Intersects(float2 positionA, float2 scaleA, float2 positionB, float2 scaleB)
    {
        return
          (Abs(positionA.x - positionB.x) * 2 < (scaleA.x + scaleB.x)) &&
          (Abs(positionA.y - positionB.y) * 2 < (scaleA.y + scaleB.y));
    }
    
    public static bool Intersects(this Bound2D a, Bound2D b)
    {
        return
          (Abs(a.position.x - b.position.x) * 2 < (a.scale.x + b.scale.x)) &&
          (Abs(a.position.y - b.position.y) * 2 < (a.scale.y + b.scale.y));
    }
    
    //should be faster than math.abs
    static float Abs(float x)
    {
        return (x >= 0) ? x : -x;
    }
    public static float2[] BoundValuesFromCamera(Camera camera)
    {
        float screenAspect = (float)Screen.width / (float)Screen.height;
        float cameraHeight = camera.orthographicSize * 2;
        return new float2[2] { (Vector2)camera.transform.position, new float2(cameraHeight * screenAspect, cameraHeight) };
    }
}
