using System;
using System.IO;
using System.Reflection;
using UnityEngine;


namespace tos1UI
{
    public class LoadEmbeddedResources
    {
        public static Sprite LoadSprite(
            string FilePath,
            float PixelsPerUnit = 100f,
            SpriteMeshType spriteType = SpriteMeshType.Tight)
        {
            Texture2D texture2D = LoadEmbeddedResources.LoadTexture(FilePath);
            return Sprite.Create(texture2D, new Rect(0.0f, 0.0f, (float) ((Texture) texture2D).width, (float) ((Texture) texture2D).height), new Vector2(0.0f, 0.0f), PixelsPerUnit, 0U, spriteType);
        }

        public static Texture2D LoadTexture(string FilePath)
        {
            Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(FilePath);
            Texture2D texture2D = new Texture2D(2, 2);
            if (ImageConversion.LoadImage(texture2D, LoadEmbeddedResources.ReadFully(manifestResourceStream)))
                return texture2D;
            Console.WriteLine("File does not exist!");
            return (Texture2D) null;
        }

        public static byte[] ReadFully(Stream input)
        {
            using (MemoryStream destination = new MemoryStream())
            {
                input.CopyTo((Stream) destination);
                return destination.ToArray();
            }
        }
    }
}