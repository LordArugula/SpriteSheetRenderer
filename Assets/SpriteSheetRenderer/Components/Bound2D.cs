using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct Bound2D : IComponentData
{
    public float2 position;
    public float2 scale;
    public bool visible;
}

    {
        float screenAspect = (float)Screen.width / (float)Screen.height;
        float cameraHeight = camera.orthographicSize * 2;
        return new float2[2] { (Vector2)camera.transform.position, new float2(cameraHeight * screenAspect, cameraHeight) };
    }
}
