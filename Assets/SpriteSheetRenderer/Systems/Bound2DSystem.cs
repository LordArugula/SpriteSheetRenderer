using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ECSSpriteSheetAnimation
{
    [UpdateInGroup(groupType: typeof(SpriteSheetPreperationGroup))]
    public class Bound2DSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // update buffer data
            Dependency = Entities
                .WithBurst()
                .ForEach((ref Bound2D bound, in Position2D translation, in Scale scale) =>
                {
                    bound.scale = scale.Value;
                    bound.position = translation.Value;
                })
                .ScheduleParallel(Dependency);
            
            CalculateMaterialBounds();
        }

        /// <summary>
        /// Work out the bounds for <see cref="SpriteSheetRendererSystem"/> to work correctly with a moving camera for each sprite material
        /// </summary>
        private void CalculateMaterialBounds()
        {
            int bufferCount = DynamicBufferManager.GetIndexBuffers().Length;
            JobHandle[] handles = new JobHandle[bufferCount];
            NativeReference<float2x2>[] materialBounds = new NativeReference<float2x2>[bufferCount];
            
            // calculate world space bounds
            for (int bufferID = 0; bufferID < bufferCount; bufferID++)
            {
                Material material = DynamicBufferManager.GetMaterial(bufferID);
                
                materialBounds[bufferID] = new NativeReference<float2x2>(
                    new float2x2(new float2(float.MaxValue), new float2(float.MinValue)),
                    Allocator.TempJob);
                NativeReference<float2x2> localBounds = materialBounds[bufferID]; 
                handles[bufferID] = Entities
                    .WithBurst()
                    .WithSharedComponentFilter(new SpriteSheetMaterial {material = material})
                    .ForEach((ref Bound2D bounds) =>
                    {
                        float2x2 value = localBounds.Value;
                        float2 relativeScale = bounds.scale / 2f;
                        // todo need to check if Position2D represents one of the sprite corners or the center (I assume center in this code) 
                        value[0].x = math.min(value[0].x, bounds.position.x - relativeScale.x);
                        value[0].y = math.min(value[0].y, bounds.position.y - relativeScale.y);
                        value[1].x = math.max(value[1].x, bounds.position.x + relativeScale.x);
                        value[1].y = math.max(value[1].y, bounds.position.y + relativeScale.y);
                        localBounds.Value = value;
                    })
                    .Schedule(Dependency);

#if UNITY_ASSERTIONS
                handles[bufferID].Complete(); // todo remove when unity likes shared component filter with concurrently scheduled jobs
#endif
            }

            // apply bounds to rendering data
            for (int bufferID = 0; bufferID < bufferCount; bufferID++)
            {
                handles[bufferID].Complete();

                float2x2 bounds = materialBounds[bufferID].Value;
                Vector3 size = new Vector3(
                    math.abs(bounds[1].x - bounds[0].x),
                    math.abs(bounds[1].y - bounds[0].y),
                    0f);
                SpriteSheetManager.renderInformation[bufferID].bounds = new Bounds(
                    new Vector3(
                        bounds[0].x + (size.x / 2),
                        bounds[0].y + (size.y / 2),
                        0f),
                    size);

                materialBounds[bufferID].Dispose();
            }
        }
    } 
}
