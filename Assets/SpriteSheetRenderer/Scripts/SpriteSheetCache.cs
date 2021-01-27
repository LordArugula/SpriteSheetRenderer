using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

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
                var s = sprites[i];
                uvs[i] = new float4(x: 1f * (s.rect.width / w),
                                    y: 1f * (s.rect.height / h),
                                    z: 1f * (s.rect.x / w),
                                    w: 1f * (s.rect.y / h));
            }
            materialNameMaterial.Add(materialName, new KeyValuePair<Material, int>(material, sprites.Length));
            materialToName.Add(material, materialName);
            return new KeyValuePair<Material, float4[]>(material, uvs);
        }

        public static void AddAnimator(Entity entity, SpriteSheetAnimator animator)
        {
            entityAnimator.Add(entity, animator);
        }

        public static int TotalLength() => materialNameMaterial.Count;

        public static int GetLength(string spriteSheetName) => materialNameMaterial[spriteSheetName].Value;

        public static Material GetMaterial(string spriteSheetName) => materialNameMaterial[spriteSheetName].Key;

        public static SpriteSheetAnimator GetAnimator(Entity e) => entityAnimator[e];

        public static string GetMaterialName(Material material) => materialToName[material];

        public static int GetLength(Material material) => GetLength(GetMaterialName(material));
    }
}
