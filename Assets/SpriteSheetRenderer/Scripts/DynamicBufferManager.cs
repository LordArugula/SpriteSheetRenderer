using System.Collections.Generic;
using System.IO;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ECSSpriteSheetAnimation
{
    //todo entity is a dictionary with spritesheetmaterial and is used to separate buffers from different material

    public static class DynamicBufferManager
    {
        private static EntityManager entityManager;

        public static EntityManager EntityManager
        {
            get
            {
                if (entityManager == default)
                    entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                return entityManager;
            }
        }

        //list of all the "Enities with all the buffers"
        //Each different material have a different bufferEntity
        private static List<Entity> bufferEntities = new List<Entity>();

        //contains the index of a bufferEntity inside the bufferEntities from a material
        private static Dictionary<Material, int> materialEntityBufferID = new Dictionary<Material, int>();

        private static Dictionary<Material, HashSet<int>> availableEntityID = new Dictionary<Material, HashSet<int>>();
        private static Dictionary<Material, bool> contiguousEntityID = new Dictionary<Material, bool>();

        //only use this when you didn't bake the uv yet
        public static void BakeUvBuffer(SpriteSheetMaterial spriteSheetMaterial, KeyValuePair<Material, float4[]> atlasData)
        {
            Entity entity = GetEntityBuffer(spriteSheetMaterial.material);
            var buffer = EntityManager.GetBuffer<UvBuffer>(entity);
            for (int j = 0; j < atlasData.Value.Length; j++)
                buffer.Add(atlasData.Value[j]);
        }

        public static void GenerateBuffers(SpriteSheetMaterial material, int entityCount = 0)
        {
            if (!materialEntityBufferID.ContainsKey(material.material))
            {
                CreateBuffersContainer(material);
                availableEntityID.Add(material.material, new HashSet<int>());
                for (int i = 0; i < entityCount; i++)
                    availableEntityID[material.material].Add(i);
                contiguousEntityID.Add(material.material, true);
                MassAddBuffers(bufferEntities.Last(), entityCount);
            }
        }

        //use this when it's the first time you are using that material
        //use this just to generate the buffers container
        public static void CreateBuffersContainer(SpriteSheetMaterial material)
        {
            var archetype = EntityManager.CreateArchetype(
              typeof(SpriteIndexBuffer),
              typeof(MatrixBuffer),
              typeof(SpriteColorBuffer),
              typeof(SpriteSheetMaterial),
              typeof(UvBuffer)
            );
            Entity e = EntityManager.CreateEntity(archetype);
            bufferEntities.Add(e);
            EntityManager.SetSharedComponentData(e, material);
            materialEntityBufferID.Add(material.material, materialEntityBufferID.Count);
        }

        /// <summary>
        /// when you create a new entity you need a new buffer for it in order for Rendering to work correctly 
        /// </summary>
        /// <param name="bufferEntity">Buffer </param>
        /// <param name="entityCount">Total amount of entities that will use this buffer</param>
        public static void MassAddBuffers(Entity bufferEntity, int entityCount)
        {
            var indexBuffer = EntityManager.GetBuffer<SpriteIndexBuffer>(bufferEntity);
            var colorBuffer = EntityManager.GetBuffer<SpriteColorBuffer>(bufferEntity);
            var matrixBuffer = EntityManager.GetBuffer<MatrixBuffer>(bufferEntity);
            for (int i = 0; i < entityCount; i++)
            {
                indexBuffer.Add(new SpriteIndexBuffer());
                matrixBuffer.Add(new MatrixBuffer());
                colorBuffer.Add(new SpriteColorBuffer());
            }
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AddDynamicBuffers(Entity bufferEntity, Material material)
        {
            int bufferId = NextIDForEntity(material);
            var indexBuffer = EntityManager.GetBuffer<SpriteIndexBuffer>(bufferEntity);
            var colorBuffer = EntityManager.GetBuffer<SpriteColorBuffer>(bufferEntity);
            var matrixBuffer = EntityManager.GetBuffer<MatrixBuffer>(bufferEntity);
            if (indexBuffer.Length <= bufferId)
            {
                indexBuffer.Add(new SpriteIndexBuffer());
                colorBuffer.Add(new SpriteColorBuffer());
                matrixBuffer.Add(new MatrixBuffer());
            }
            return bufferId;
        }

        public static BufferHook GetBufferHook(SpriteSheetMaterial material)
        {
            return new BufferHook { entityID = materialEntityBufferID[material.material], bufferID = NextIDForEntity(material.material) };
        }

        public static int GetEntityBufferID(SpriteSheetMaterial material)
        {
            return materialEntityBufferID[material.material];
        }

        public static Entity GetEntityBuffer(Material material)
        {
            return bufferEntities[materialEntityBufferID[material]];
        }

        public static int NextIDForEntity(Material material)
        {
            var ids = availableEntityID[material];
            if(contiguousEntityID[material])
            {
                int newId = ids.Count;
                ids.Add(newId);
                return newId;
            }
            
            // todo maybe storing free'd Ids so they are cheap to reuse would be a good idea in the RemoveBuffer method...
            for (int i = 0; i <= ids.Count; i++)
            {
                if(!ids.Contains(i))
                {
                    if(i == ids.Count)
                    {
                        // this item is outside the existing ids which means that the existing ids are all in use, so cache this knowledge so the
                        // next id doesn't have to do this horrible linear search 
                        contiguousEntityID[material] = true;
                    }
                    
                    ids.Add(i);
                    return i;
                }
            }

            throw new InvalidDataException("Failed to find valid ID for Entity!");
        }

        public static Material GetMaterial(int bufferEntityID)
        {
            foreach (KeyValuePair<Material, int> e in materialEntityBufferID)
                if (e.Value == bufferEntityID)
                    return e.Key;
            return null;
        }

        public static void RemoveBuffer(Material material, int bufferID)
        {
            Entity bufferEntity = GetEntityBuffer(material);
            contiguousEntityID[material] = contiguousEntityID[material] && availableEntityID[material].Count == bufferID; 
            availableEntityID[material].Remove(bufferID);
            CleanBuffer(bufferID, bufferEntity);
        }

        private static void CleanBuffer(int bufferID, Entity bufferEntity)
        {
            EntityManager.GetBuffer<SpriteIndexBuffer>(bufferEntity).RemoveAt(bufferID);
            EntityManager.GetBuffer<MatrixBuffer>(bufferEntity).RemoveAt(bufferID);
            EntityManager.GetBuffer<SpriteColorBuffer>(bufferEntity).RemoveAt(bufferID);

            EntityManager.GetBuffer<SpriteIndexBuffer>(bufferEntity).Insert(bufferID, new SpriteIndexBuffer { index = -1 });
            EntityManager.GetBuffer<MatrixBuffer>(bufferEntity).Insert(bufferID, new MatrixBuffer());
            EntityManager.GetBuffer<SpriteColorBuffer>(bufferEntity).Insert(bufferID, new SpriteColorBuffer());
        }

        public static DynamicBuffer<SpriteIndexBuffer>[] GetIndexBuffers()
        {
            DynamicBuffer<SpriteIndexBuffer>[] buffers = new DynamicBuffer<SpriteIndexBuffer>[bufferEntities.Count];
            for (int i = 0; i < buffers.Length; i++)
                buffers[i] = EntityManager.GetBuffer<SpriteIndexBuffer>(bufferEntities[i]);
            return buffers;
        }
        public static DynamicBuffer<MatrixBuffer>[] GetMatrixBuffers()
        {
            DynamicBuffer<MatrixBuffer>[] buffers = new DynamicBuffer<MatrixBuffer>[bufferEntities.Count];
            for (int i = 0; i < buffers.Length; i++)
                buffers[i] = EntityManager.GetBuffer<MatrixBuffer>(bufferEntities[i]);
            return buffers;
        }
        public static DynamicBuffer<SpriteColorBuffer>[] GetColorBuffers()
        {
            DynamicBuffer<SpriteColorBuffer>[] buffers = new DynamicBuffer<SpriteColorBuffer>[bufferEntities.Count];
            for (int i = 0; i < buffers.Length; i++)
                buffers[i] = EntityManager.GetBuffer<SpriteColorBuffer>(bufferEntities[i]);
            return buffers;
        }
        public static DynamicBuffer<UvBuffer>[] GetUvBuffers()
        {
            DynamicBuffer<UvBuffer>[] buffers = new DynamicBuffer<UvBuffer>[bufferEntities.Count];
            for (int i = 0; i < buffers.Length; i++)
                buffers[i] = EntityManager.GetBuffer<UvBuffer>(bufferEntities[i]);
            return buffers;
        }
    } 
}
