using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Profiling;

namespace ECSSpriteSheetAnimation
{
    [UpdateInGroup(groupType: typeof(SpriteSheetPreperationGroup))]
    public class Bound2DSystem : SystemBase
    {
        private EntityQuery m_jobQuery;

        protected override void OnStartRunning()
        {
            m_jobQuery = EntityManager.CreateEntityQuery(
                ComponentType.ReadOnly(typeof(Bound2D)),
                ComponentType.ReadOnly(typeof(SpriteSheetMaterial)));
            base.OnStartRunning();
        }

        protected override void OnStopRunning()
        {
            m_jobQuery.Dispose();
            base.OnStopRunning();
        }

        protected override void OnUpdate()
        {
            int bufferCount = DynamicBufferManager.GetIndexBuffers().Length;
            JobHandle[] handles = new JobHandle[bufferCount];
            BoundsCheckJob[] materialBounds = new BoundsCheckJob[bufferCount];
            
            CalculateMaterialBoundsBegin(ref handles, ref materialBounds);
            
            // update buffer data
            Dependency = Entities
                .WithBurst()
                .ForEach((ref Bound2D bound, in Position2D translation, in Scale scale) =>
                {
                    bound.scale = scale.Value;
                    bound.position = translation.Value;
                })
                .ScheduleParallel(Dependency);
            
            CalculateMaterialBoundsEnd(ref handles, ref materialBounds);
        }

        /// <summary>
        /// Schedule the bounds calculations for <see cref="SpriteSheetRendererSystem"/> to work correctly with a moving camera for each sprite material
        /// </summary>
        private void CalculateMaterialBoundsBegin(ref JobHandle[] handles, ref BoundsCheckJob[] materialBounds)
        {
            for (int bufferID = 0; bufferID < handles.Length; bufferID++)
            {
                Material material = DynamicBufferManager.GetMaterial(bufferID); 
                m_jobQuery.SetSharedComponentFilter(new SpriteSheetMaterial(){material = material});

                materialBounds[bufferID] = new BoundsCheckJob(m_jobQuery);
                handles[bufferID] = materialBounds[bufferID].Schedule(materialBounds[bufferID].Ready);
            }
        }

        /// <summary>
        /// Work out the bounds for <see cref="SpriteSheetRendererSystem"/> to work correctly with a moving camera for each sprite material
        /// </summary>
        private void CalculateMaterialBoundsEnd(ref JobHandle[] handles, ref BoundsCheckJob[] materialBounds)
        {
            // apply bounds to rendering data
            for (int bufferID = 0; bufferID < handles.Length; bufferID++)
            {
                handles[bufferID].Complete();

                float2x2 bounds = materialBounds[bufferID].CalculatedBounds;
                materialBounds[bufferID].Dispose();
                
                Vector3 size = new Vector3(
                    math.abs(bounds[1].x - bounds[0].x),
                    math.abs(bounds[1].y - bounds[0].y),
                    0f);
                /*Bounds bufferBounds = new Bounds(
                    new Vector3(
                        bounds[0].x + (size.x / 2),
                        bounds[0].y + (size.y / 2),
                        0f),
                    size);*/
                
                // the code commented out above will produce a perfect bounding box but has issues with ofsetting the rendered output
                // inside the render system when using it in the "Graphics.DrawMeshInstancedIndirect" call
                Bounds bufferBounds = new Bounds(
                    new Vector3(0f, 0f, 0f),
                    new Vector3(
                        (bounds[0].x + size.x) * 2,
                        (bounds[0].y + size.y) * 2,
                        0f));
                SpriteSheetManager.renderInformation[bufferID].bounds = bufferBounds;
                
#if DEBUG && false
                Debug.DrawLine(new Vector3(bufferBounds.min.x, bufferBounds.min.y, 0), new Vector3(bufferBounds.max.x, bufferBounds.min.y, 0));
                Debug.DrawLine(new Vector3(bufferBounds.max.x, bufferBounds.min.y, 0), new Vector3(bufferBounds.max.x, bufferBounds.max.y, 0));
                Debug.DrawLine(new Vector3(bufferBounds.max.x, bufferBounds.max.y, 0), new Vector3(bufferBounds.min.x, bufferBounds.max.y, 0));
                Debug.DrawLine(new Vector3(bufferBounds.min.x, bufferBounds.max.y, 0), new Vector3(bufferBounds.min.x, bufferBounds.min.y, 0));
#endif
            }
        }

        [BurstCompile(CompileSynchronously = true)]
        private struct BoundsCheckJob : IJobParallelForBatch
        {
            public float2x2 CalculatedBounds
            {
                get
                {
                    float2x2 value = m_threadBounds[0];
                    for (int i = 1; i < m_threadBounds.Length; i++)
                    {
                        value[0] = math.min(value[0], m_threadBounds[i][0]);
                        value[1] = math.max(value[1], m_threadBounds[i][1]);
                    }
                    return value;
                }
            }
            
            public JobHandle Ready { get; }
            
            [NativeSetThreadIndex]
            private int m_jobThread;
            [NativeDisableContainerSafetyRestriction]
            private NativeArray<float2x2> m_threadBounds;
            private NativeArray<Bound2D> m_entityBounds;
            
            public BoundsCheckJob(EntityQuery query)
            {
                JobHandle handle;
                m_entityBounds = query.ToComponentDataArrayAsync<Bound2D>(Allocator.TempJob, out handle);
                Ready = handle;
                
                m_threadBounds = new NativeArray<float2x2>(JobsUtility.MaxJobThreadCount, Allocator.TempJob, NativeArrayOptions.ClearMemory);
                for (int i = 0; i < m_threadBounds.Length; i++)
                    m_threadBounds[i] = new float2x2(new float2(float.MaxValue), new float2(float.MinValue));

                m_jobThread = 0; // just needed to compile
            }

            public JobHandle Schedule(JobHandle handle = default(JobHandle))
            {
                return this.ScheduleBatch(m_entityBounds.Length, 1024, handle);
            }

            public void Execute(int startIndex, int count)
            {
                float2x2 value = m_threadBounds[m_jobThread];

                for (int i = startIndex; i < startIndex + count; i++)
                {
                    Bound2D bounds = m_entityBounds[i];
                    
                    float2 relativeScale = bounds.scale / 2f;
                    // todo need to check if Position2D represents one of the sprite corners or the center (I assume center in this code) 
                    value[0] = math.min(value[0], bounds.position - relativeScale);
                    value[1] = math.max(value[1], bounds.position + relativeScale);
                }
                
                m_threadBounds[m_jobThread] = value;
            }

            public void Dispose()
            {
                m_entityBounds.Dispose();
                m_threadBounds.Dispose();
            }
        }
    } 
}
