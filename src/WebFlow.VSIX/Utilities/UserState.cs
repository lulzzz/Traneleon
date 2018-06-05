using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Acklann.WebFlow.Utilities
{
    public sealed class UserState : IDisposable
    {
        private UserState()
        {
            WatchList = new List<string>();
        }

        public List<string> WatchList { get; set; }

        public static UserState Load()
        {
            if (File.Exists(_path))
            {
                using (Stream file = File.OpenRead(_path))
                {
                    return (UserState)new XmlSerializer(typeof(UserState)).Deserialize(file);
                }
            }
            else
            {
                return new UserState();
            }
        }

        public void Save()
        {
            string dir = Path.GetDirectoryName(_path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var serializer = new XmlSerializer(typeof(UserState));
            using (Stream file = File.OpenWrite(_path))
            {
                serializer.Serialize(file, this);
            }
        }

        public bool CheckIfOnWatchList(string fullPath)
        {
            if (WatchList != null)
                foreach (string path in WatchList)
                    if (path.Equals(fullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

            return false;
        }

        public void Dispose()
        {
            Save();
        }

        #region Private Members

        private static readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(WebFlow), "userstate.xml");

        #endregion Private Members

        #region Singleton

        public static UserState Instance
        {
            get { return Nested._instance; }
        }

        private class Nested
        {
            static Nested()
            {
            }

            internal static readonly UserState _instance = Load();
        }

        #endregion Singleton
    }
}