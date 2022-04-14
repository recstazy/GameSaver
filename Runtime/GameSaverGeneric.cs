using UnityEngine;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Recstazy.GameSaver
{
    public class GameSaverGeneric<TSave, TProfile> : GameSaverBase where TSave : new() where TProfile : new()
    {
        #region Fields

        protected static event Action OnBeforeSavingSave;
        protected static event Action OnSavedSave;
        protected static event Action OnBeforeLoadSave;
        protected static event Action OnLoadedSave;
        
        protected static event Action<string> OnBeforeLoadProfile;
        protected static event Action<string> OnLoadedProfile;
        protected static event Action<string> OnBeforeSavingProfile;
        protected static event Action<string> OnSavedProfile;
        
        protected static TSave s_save;
        protected static Dictionary<string, TProfile> s_profiles = new Dictionary<string, TProfile>();

        #endregion

        #region Properties

        public static TSave Save
        {
            get
            {
                if (s_save == null)
                {
                    LoadSaveFromFile();
                }

                return s_save;
            }
        }

        #endregion

        static GameSaverGeneric()
        {
            OnSavesDeleted += SavesDeleted;
        }

        public static void SaveChanged()
        {
            OnBeforeSavingSave?.Invoke();
            SaveToDisc();
            OnSavedSave?.Invoke();
        }

        public static TProfile GetProfile(string name)
        {
            return LoadProfileFromFile(name);
        }

        public static void SaveProfile(string name)
        {
            OnBeforeSavingProfile?.Invoke(name);
            SaveProfileToDisc(name);
            OnSavedProfile?.Invoke(name);
        }

        private static void SavesDeleted()
        {
            s_save = default;

            var keys = s_profiles.Keys.ToArray();

            for (int i = 0; i < keys.Length; i++)
            {
                s_profiles[keys[i]] = default;
                SaveProfile(keys[i]);
            }

            SaveChanged();
        }

        private static int GetEncryptionKeyWithCurrentSettings()
        {
            if (Debug.isDebugBuild)
            {
                if (!Settings.EncryptInDevBuilds)
                {
                    return 0;
                }
            }
            else
            {
                if (!Settings.EncryptInRelease)
                {
                    return 0;
                }
            }

            return Settings.EncryptionKey;
        }

        private static void LoadSaveFromFile()
        {
            OnBeforeLoadSave?.Invoke();
            LoadSaveFromDisc();
            
            if (s_save == null)
            {
                s_save = new TSave();
                SaveToDisc();
            }

            OnLoadedSave?.Invoke();
        }

        private static TProfile LoadProfileFromFile(string name)
        {
            if (s_profiles.TryGetValue(name, out TProfile profile))
            {
                return profile;
            }

            OnBeforeLoadProfile?.Invoke(name);
            profile = LoadProfileFromDisc(name);
            
            if (profile == null)
            {
                profile = new TProfile();
                s_profiles[name] = profile;
                SaveProfileToDisc(name);
            }

            OnLoadedProfile?.Invoke(name);
            return profile;
        }

        private static void SaveToDisc()
        {
            if (s_save is ISaveLoadReciever saveLoadReciever)
            {
                saveLoadReciever.OnBeforeSerialize();
            }

            if (Application.isEditor && Settings.SaveOverride != null)
            {
                (Settings.SaveOverride as ScriptableSaveDataGeneric<TSave>).SaveObject = s_save;
                Settings.SaveOverride.SetDirty();
            }
            else
            {
                SaveSerializer.EncryptionKey = GetEncryptionKeyWithCurrentSettings();
                SaveSerializer.Serializator = Settings.SerializatorType;
                SaveSerializer.Serialize(s_save, Settings.SavesFullDirectory, Settings.SaveName);
            }
        }

        private static void LoadSaveFromDisc()
        {
            if (Application.isEditor && Settings.SaveOverride != null)
            {
                s_save = (Settings.SaveOverride as ScriptableSaveDataGeneric<TSave>).SaveObject;
            }
            else
            {
                SaveSerializer.EncryptionKey = GetEncryptionKeyWithCurrentSettings();
                SaveSerializer.Serializator = Settings.SerializatorType;
                s_save = SaveSerializer.Deserialize<TSave>(Settings.SavesFullDirectory, Settings.SaveName);
            }

            if (s_save is ISaveLoadReciever saveLoadReciever)
            {
                saveLoadReciever.OnAfterDeserialize();
            }
        }

        private static void SaveProfileToDisc(string name)
        {
            var profileObj = s_profiles[name];

            if (profileObj is ISaveLoadReciever saveLoadReciever)
            {
                saveLoadReciever.OnBeforeSerialize();
            }

            if (Application.isEditor && Settings.TryGetProfileOverride(name, out ScriptableSaveDataGeneric<TProfile> profile))
            {
                profile.SaveObject = profileObj;
                profile.SetDirty();
            }
            else
            {
                SaveSerializer.EncryptionKey = GetEncryptionKeyWithCurrentSettings();
                SaveSerializer.Serializator = Settings.SerializatorType;
                SaveSerializer.Serialize(profileObj, Settings.SavesFullDirectory, GetProfileFileName(name));
            }
        }

        private static TProfile LoadProfileFromDisc(string name)
        {
            TProfile profileObj;

            if (Application.isEditor && Settings.TryGetProfileOverride(name, out ScriptableSaveDataGeneric<TProfile> profile))
            {
                profileObj = profile.SaveObject;
            }
            else
            {
                SaveSerializer.EncryptionKey = GetEncryptionKeyWithCurrentSettings();
                SaveSerializer.Serializator = Settings.SerializatorType;
                profileObj = SaveSerializer.Deserialize<TProfile>(Settings.SavesFullDirectory, GetProfileFileName(name));
            }

            if (profileObj is ISaveLoadReciever saveLoadReciever)
            {
                saveLoadReciever.OnAfterDeserialize();
            }

            if (s_profiles.ContainsKey(name))
            {
                s_profiles[name] = profileObj;
            }
            else
            {
                s_profiles.Add(name, profileObj);
            }

            return profileObj;
        }

        #region Texture Save-Load

        public static void SaveTexture(Texture2D texture, string name)
        {
            string directory = Settings.SavesFullDirectory;
            SaveSerializer.CheckOrCreateDirectory(directory);

            var bytes = texture.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(directory, name) + ".png", bytes);
        }

        public static Texture2D LoadTexture(string name)
        {
            string path = Path.Combine(Settings.SavesFullDirectory, name) + ".png";

            if (File.Exists(path))
            {
                var bytes = File.ReadAllBytes(path);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(bytes);
                return tex;
            }

            return null;
        }

        #endregion
    }
}
