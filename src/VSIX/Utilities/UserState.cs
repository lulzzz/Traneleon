﻿using System;
using System.IO;
using System.Xml.Serialization;

namespace Acklann.Traneleon.Utilities
{
    [XmlRoot("state")]
    public sealed class UserState : IDisposable
    {
        [XmlElement("watch")]
        public bool WatcherEnabled { get; set; }

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

        public void Save(string filePath = null)
        {
            if (filePath == null) filePath = _path;

            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            try
            {
                using (Stream file = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    var serializer = new XmlSerializer(typeof(UserState));
                    serializer.Serialize(file, this);
                }
            }
            catch (IOException) { }
        }

        public void Dispose()
        {
            Save();
        }

        #region Private Members

        private static readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(Traneleon), "userstate.xml");

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