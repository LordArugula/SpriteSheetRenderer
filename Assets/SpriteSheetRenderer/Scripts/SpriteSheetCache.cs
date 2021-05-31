using System;
using System.Collections.Generic;
using Unity.Assertions;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.U2D;

namespace ECSSpriteSheetAnimation
{
    public static class SpriteSheetCache
    {
        private static Dictionary<string, KeyValuePair<Material, int>> materialNameMaterial;
        private static Dictionary<Material, string> materialToName;
        private static Dictionary<Entity, SpriteSheetAnimator> entityAnimator;

        static SpriteSheetCache()
        {
            materialNameMaterial = new Dictionary<string, KeyValuePair<Material, int>>();
            materialToName = new Dictionary<Material, string>();
            entityAnimator = new Dictionary<Entity, SpriteSheetAnimator>();
        }

        public static KeyValuePair<Material, float4[]> BakeSprites(Sprite[] sprites, string materialName)
        {
            Material material = new Material(Shader.Find("Instanced/SpriteSheet"));
            Texture texture = sprites[0].texture;
            material.mainTexture = texture;

            float w = texture.width;
            float h = texture.height;
            float4[] uvs = new float4[sprites.Length];
            for (int i = 0; i < sprites.Length; i++)
            {
                Sprite s = sprites[i];
                if(s.packingMode == SpritePackingMode.Rectangle)
                {
                    uvs[i] = new float4(
                        x: 1f * (s.textureRect.width / w),
                        y: 1f * (s.textureRect.height / h),
                        z: 1f * (s.textureRect.x / w),
                        w: 1f * (s.textureRect.y / h));
                }
                else if(s.packingMode == SpritePackingMode.Tight)
                {
                    uvs[i] = new float4(
                        x: uvs[i].x,
                        y: uvs[i].y,
                        z: 1f,
                        w: 1f);
                    
                    // This is a naive implementation as 'tight' packing packs the UVs where a non-square texture could overlap with the AABB of other
                    // sprites. a better implementation would extract the sprite and place it into a sprite sheet style arrangement... One day :)
                    for (int j = 1; j < s.uv.Length; j++)
                    {
                        uvs[i].x = math.max(uvs[i].x, s.uv[j].x);
                        uvs[i].y = math.max(uvs[i].y, s.uv[j].y);
                        uvs[i].z = math.min(uvs[i].z, s.uv[j].x);
                        uvs[i].w = math.min(uvs[i].w, s.uv[j].y);
                    }
                }
                else
                {
                    // just in case there is a new sprite packing mode in future ;)
                    throw new NotImplementedException();
                }
            }
            materialNameMaterial.Add(materialName, new KeyValuePair<Material, int>(material, sprites.Length));
            materialToName.Add(material, materialName);
            return new KeyValuePair<Material, float4[]>(material, uvs);
        }

        public static KeyValuePair<Material, float4[]> BakeSprites(SpriteAtlas atlas, string materialName)
        {
            Sprite[] sprites = new Sprite[atlas.spriteCount];
            atlas.GetSprites(sprites);

            return BakeSprites(sprites, materialName);
        }

        public static void AddAnimator(Entity entity, SpriteSheetAnimator animator)
        {
            entityAnimator.Add(entity, animator);
        }

        public static int TotalLength() => materialNameMaterial.Count;

        public static int GetLength(string spriteSheetName) => materialNameMaterial[spriteSheetName].Value;

        public static bool HasMaterial(string spriteSheetName) => materialNameMaterial.ContainsKey(spriteSheetName);
        
        public static Material GetMaterial(string spriteSheetName) => materialNameMaterial[spriteSheetName].Key;

        public static SpriteSheetAnimator GetAnimator(Entity e) => entityAnimator[e];

        public static string GetMaterialName(Material material) => materialToName[material];

        public static int GetLength(Material material) => GetLength(GetMaterialName(material));
    }
}
