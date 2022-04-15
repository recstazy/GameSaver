using UnityEngine;
using System.IO;
using System.Text;

#if NEWTONSOFT
using Newtonsoft.Json;
#endif

namespace Recstazy.GameSaver
{
    public static class SaveSerializer
    {
        public static int EncryptionKey { get; set; }
        public static Serializator Serializator { get; set; }

        /// <summary>Serialize object as JSON</summary>
        /// <param name="objectToSave">What to save</param>
        /// <param name="directory">Folder to save the file</param>
        /// <param name="fileName">Without ".json"</param>
        public static void Serialize(object objectToSave, string directory, string fileName)
        {
            switch (Serializator)
            {
                case Serializator.JsonUtility:
                    SerializeJsonUtility(objectToSave, directory, fileName);
                    break;
                case Serializator.Newtonsoft:
                    SerializeNewtonsoft(objectToSave, directory, fileName);
                    break;
                default:
                    Debug.LogError($"Serializator {Serializator} is not supported");
                    break;
            }
        }

        /// <summary>Deserialize object from disk</summary>
        /// <param name="directory">Folder where file is located</param>
        /// <param name="fileName">Without ".json"</param>
        public static T Deserialize<T>(string directory, string fileName)
        {
            switch (Serializator)
            {
                case Serializator.JsonUtility:
                    return DeserializeJsonUtility<T>(directory, fileName);
                case Serializator.Newtonsoft:
                    return DeserializeNewtonsoft<T>(directory, fileName);
                default:
                    Debug.LogError($"Serializator {Serializator} is not supported");
                    return default;
            }
        }

        /// <summary>
        /// Check if directory exists and create one if it doesnt
        /// </summary>
        public static void CheckOrCreateDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static string EncryptOrDecrypt(string text, int encryptionKey)
        {
            StringBuilder inputStringBuild = new StringBuilder(text);
            StringBuilder outStringBuild = new StringBuilder(text.Length);
            char currentChar;

            for (int i = 0; i < text.Length; i++)
            {
                currentChar = inputStringBuild[i];
                currentChar = (char)(currentChar ^ encryptionKey);
                outStringBuild.Append(currentChar);
            }

            return outStringBuild.ToString();
        }

        private static void SerializeJsonUtility(object objectToSave, string directory, string fileName)
        {
            CheckOrCreateDirectory(directory);

            string json = JsonUtility.ToJson(objectToSave);
            string path = Path.Combine(directory, fileName) + ".json";

            if (EncryptionKey != 0)
            {
                json = EncryptOrDecrypt(json, EncryptionKey);
            }

            File.WriteAllText(path, json);
        }

        private static T DeserializeJsonUtility<T>(string directory, string fileName)
        {
            string path = Path.Combine(directory, fileName) + ".json";

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);

                if (EncryptionKey != 0)
                {
                    json = EncryptOrDecrypt(json, EncryptionKey);
                }

                T obj = default;

                try
                {
                    obj = JsonUtility.FromJson<T>(json);
                }
                catch (System.ArgumentException)
                {
                    Debug.LogError("Decryption of json save failed");
                }

                return obj;
            }

            return default;
        }

        private static void SerializeNewtonsoft(object objectToSave, string directory, string fileName)
        {
#if NEWTONSOFT
            CheckOrCreateDirectory(directory);

            string json = JsonConvert.SerializeObject(objectToSave);
            string path = Path.Combine(directory, fileName) + ".json";

            if (EncryptionKey != 0)
            {
                json = EncryptOrDecrypt(json, EncryptionKey);
            }

            File.WriteAllText(path, json);
#else
            LogNewtonsoftError();
#endif
        }

        private static T DeserializeNewtonsoft<T>(string directory, string fileName)
        {
#if NEWTONSOFT
            string path = Path.Combine(directory, fileName) + ".json";

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);

                if (EncryptionKey != 0)
                {
                    json = EncryptOrDecrypt(json, EncryptionKey);
                }

                T obj = default;

                try
                {
                    obj = JsonConvert.DeserializeObject<T>(json);
                }
                catch (JsonReaderException)
                {
                    Debug.LogError("Decryption of json save failed");
                }

                return obj;
            }

            return default;
#else
            LogNewtonsoftError();
            return default;
#endif
        }

        private static void LogNewtonsoftError()
        {
            Debug.LogError("Package com.unity.nuget.newtonsoft-json is not present in project " +
                "and Newtonsoft.Json can not be used.");
        }
    }
}
