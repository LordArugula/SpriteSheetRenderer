using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECSSpriteSheetAnimation
{
    public static class SpriteSheetManager
    {
        private static EntityManager entityManager;
        public static List<RenderInformation> renderInformation = new List<RenderInformation>();

        public static EntityManager EntityManager
        {
            get
            {
                if (entityManager == default)
                    entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                return entityManager;
            }
        }

        public static Entity Instantiate(EntityArchetype archetype, List<IComponentData> componentDatas, string spriteSheetName)
        {
            Entity e = EntityManager.CreateEntity(archetype);
            Material material = SpriteSheetCache.GetMaterial(spriteSheetName);
            int bufferID = DynamicBufferManager.AddDynamicBuffers(DynamicBufferManager.GetEntityBuffer(material), material);
            foreach (IComponentData Idata in componentDatas)
                EntityManager.SetComponentData(e, (dynamic)Idata);

            var spriteSheetMaterial = new SpriteSheetMaterial { material = material };
            BufferHook bh = new BufferHook { bufferID = bufferID, bufferEntityID = DynamicBufferManager.GetEntityBufferID(spriteSheetMaterial) };
            EntityManager.SetComponentData(e, bh);
            EntityManager.SetSharedComponentData(e, spriteSheetMaterial);
            return e;
        }

        public static Entity Instantiate(EntityArchetype archetype, List<IComponentData> componentDatas, SpriteSheetAnimator animator)
        {
            Entity e = EntityManager.CreateEntity(archetype);
            animator.currentAnimationIndex = animator.defaultAnimationIndex;
            SpriteSheetAnimationClip startAnim = animator.animations[animator.defaultAnimationIndex];
            int maxSprites = startAnim.FrameCount;
            Material material = SpriteSheetCache.GetMaterial(animator.animations[animator.defaultAnimationIndex].AnimationName);
            int bufferID = DynamicBufferManager.AddDynamicBuffers(DynamicBufferManager.GetEntityBuffer(material), material);
            foreach (IComponentData Idata in componentDatas)
                EntityManager.SetComponentData(e, (dynamic)Idata);

            var spriteSheetMaterial = new SpriteSheetMaterial { material = material };
            BufferHook bh = new BufferHook { bufferID = bufferID, bufferEntityID = DynamicBufferManager.GetEntityBufferID(spriteSheetMaterial) };
            EntityManager.SetComponentData(e, bh);
            EntityManager.SetComponentData(e, new SpriteSheetAnimation { frameCount = maxSprites, framesPerSecond = startAnim.FramesPerSecond, playMode = startAnim.PlayMode });
            EntityManager.SetComponentData(e, new SpriteIndex { Value = 0 });
            EntityManager.SetSharedComponentData(e, spriteSheetMaterial);
            animator.managedEntity = e;
            SpriteSheetCache.AddAnimator(e, animator);
            return e;
        }

        public static void SetAnimation(Entity e, SpriteSheetAnimationClip animation)
        {
            int bufferEntityID = EntityManager.GetComponentData<BufferHook>(e).bufferEntityID;
            int bufferID = EntityManager.GetComponentData<BufferHook>(e).bufferID;
            Material oldMaterial = DynamicBufferManager.GetMaterial(bufferEntityID);
            string oldAnimation = SpriteSheetCache.GetMaterialName(oldMaterial);
            if (animation.AnimationName != oldAnimation)
            {
                Material material = SpriteSheetCache.GetMaterial(animation.AnimationName);
                var spriteSheetMaterial = new SpriteSheetMaterial { material = material };

                DynamicBufferManager.RemoveBuffer(oldMaterial, bufferID);

                //use new buffer
                bufferID = DynamicBufferManager.AddDynamicBuffers(DynamicBufferManager.GetEntityBuffer(material), material);
                BufferHook bh = new BufferHook { bufferID = bufferID, bufferEntityID = DynamicBufferManager.GetEntityBufferID(spriteSheetMaterial) };

                EntityManager.SetSharedComponentData(e, spriteSheetMaterial);
                EntityManager.SetComponentData(e, bh);
            }
            EntityManager.SetComponentData(e, new SpriteSheetAnimation { frameCount = animation.Sprites.Length, framesPerSecond = animation.FramesPerSecond, playMode = animation.PlayMode, elapsedTime = 0 });
            EntityManager.SetComponentData(e, new SpriteIndex { Value = 0 });
        }

        public static void SetAnimation(EntityCommandBuffer commandBuffer, Entity e, SpriteSheetAnimationClip animation, BufferHook hook)
        {
            Material oldMaterial = DynamicBufferManager.GetMaterial(hook.bufferEntityID);
            string oldAnimation = SpriteSheetCache.GetMaterialName(oldMaterial);
            if (animation.AnimationName != oldAnimation)
            {
                Material material = SpriteSheetCache.GetMaterial(animation.AnimationName);
                var spriteSheetMaterial = new SpriteSheetMaterial { material = material };

                //clean old buffer
                DynamicBufferManager.RemoveBuffer(oldMaterial, hook.bufferID);

                //use new buffer
                int bufferID = DynamicBufferManager.AddDynamicBuffers(DynamicBufferManager.GetEntityBuffer(material), material);
                BufferHook bh = new BufferHook { bufferID = bufferID, bufferEntityID = DynamicBufferManager.GetEntityBufferID(spriteSheetMaterial) };

                commandBuffer.SetSharedComponent(e, spriteSheetMaterial);
                commandBuffer.SetComponent(e, bh);
            }
            commandBuffer.SetComponent(e, new SpriteSheetAnimation { frameCount = animation.FrameCount, framesPerSecond = animation.FramesPerSecond, playMode = animation.PlayMode, elapsedTime = 0 });
            commandBuffer.SetComponent(e, new SpriteIndex { Value = 0 });
        }

        public static void UpdateEntity(Entity entity, IComponentData componentData)
        {
            EntityManager.SetComponentData(entity, (dynamic)componentData);
        }

        public static void UpdateEntity(EntityCommandBuffer commandBuffer, Entity entity, IComponentData componentData)
        {
            commandBuffer.SetComponent(entity, (dynamic)componentData);
        }

        public static void DestroyEntity(Entity e, string materialName)
        {
            Material material = SpriteSheetCache.GetMaterial(materialName);
            int bufferID = EntityManager.GetComponentData<BufferHook>(e).bufferID;
            DynamicBufferManager.RemoveBuffer(material, bufferID);
            EntityManager.DestroyEntity(e);
        }

        public static void DestroyEntity(EntityCommandBuffer commandBuffer, Entity e, BufferHook hook)
        {
            commandBuffer.DestroyEntity(e);
            Material material = DynamicBufferManager.GetMaterial(hook.bufferEntityID);
            DynamicBufferManager.RemoveBuffer(material, hook.bufferID);
        }

        public static void RecordSpriteSheet(Sprite[] sprites, string spriteSheetName, int spriteCount = 0)
        {
            KeyValuePair<Material, float4[]> atlasData = SpriteSheetCache.BakeSprites(sprites, spriteSheetName);
            SpriteSheetMaterial material = new SpriteSheetMaterial { material = atlasData.Key };
            DynamicBufferManager.GenerateBuffers(material, spriteCount);
            DynamicBufferManager.BakeUvBuffer(material, atlasData);
            renderInformation.Add(new RenderInformation(material.material, DynamicBufferManager.GetEntityBuffer(material.material)));
        }

        public static void RecordAnimator(SpriteSheetAnimator animator)
        {
            foreach (SpriteSheetAnimationClip animation in animator.animations)
            {
                KeyValuePair<Material, float4[]> atlasData = SpriteSheetCache.BakeSprites(animation.Sprites, animation.AnimationName);
                SpriteSheetMaterial material = new SpriteSheetMaterial { material = atlasData.Key };
                DynamicBufferManager.GenerateBuffers(material);
                DynamicBufferManager.BakeUvBuffer(material, atlasData);
                renderInformation.Add(new RenderInformation(material.material, DynamicBufferManager.GetEntityBuffer(material.material)));
            }
        }

        public static void CleanBuffers()
        {
            for (int i = 0; i < renderInformation.Count; i++)
                renderInformation[i].DestroyBuffers();
            renderInformation.Clear();
        }

        public static void ReleaseUvBuffer(int bufferID)
        {
            if (renderInformation[bufferID].uvBuffer != null)
                renderInformation[bufferID].uvBuffer.Release();
        }

        public static void ReleaseBuffer(int bufferID)
        {
            if (renderInformation[bufferID].matrixBuffer != null)
                renderInformation[bufferID].matrixBuffer.Release();
            if (renderInformation[bufferID].colorsBuffer != null)
                renderInformation[bufferID].colorsBuffer.Release();
            //if(renderInformation[bufferID].uvBuffer != null)
            //renderInformation[bufferID].uvBuffer.Release();
            if (renderInformation[bufferID].indexBuffer != null)
                renderInformation[bufferID].indexBuffer.Release();
        }
    }
}
