using System;
using System.Collections.Specialized;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Voto.Consolidate
{

    [XmlRoot()]
    public class Settings
    {
        private static Settings instance = null;
        private static readonly string AppTmpFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\voto.consolidate";
        private static readonly string SettingsFilePath = AppTmpFolder + "\\settings.xml";

        private Settings() { }

        /// <summary>
        /// Access the Singleton instance
        /// </summary>
        [XmlElement]
        public static Settings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Settings();
                    Settings.Deserialize();
                }
                return instance;
            }
        }
        [XmlAttribute]
        public bool CopySetting { get; set; } = true;

        [XmlAttribute]
        public bool MoveSetting { get; set; } = false;

        [XmlAttribute]
        public string DestinationRootSetting { get; set; }

        [XmlAttribute]
        public StringCollection SourceDirectorySetting { get; set; }

        [XmlAttribute]
        public bool DeleteDuplicateFiles { get; set; } = false;

        [XmlAttribute]
        public bool DeleteEmptyDirectories { get; set; } = false;

        [XmlAttribute]
        public int DaysOldSetting { get; set; } = 0;

        [XmlAttribute]
        public bool ConsolidationSelectionAllSetting { get; set; } = true;

        [XmlAttribute]
        public bool ConsolidationSelectionOldSetting { get; set; } = false;

        [XmlAttribute]
        public bool SubfolderLastWriteDateSetting { get; set; } = true;

        [XmlAttribute]
        public bool SubfolderConsolidationDateSetting { get; set; } = false;

        [XmlAttribute]
        public bool OverwriteSetting { get; set; } = false;

        [XmlAttribute]
        public bool PicBmpSetting { get; set; } = true;

        [XmlAttribute]
        public bool PicGifSetting { get; set; } = true;

        [XmlAttribute]
        public bool PicJpegSetting { get; set; } = true;

        [XmlAttribute]
        public bool PicJpgSetting { get; set; } = true;

        [XmlAttribute]
        public bool PicPngSetting { get; set; } = true;

        [XmlAttribute]
        public bool PicPsdSetting { get; set; } = true;

        [XmlAttribute]
        public bool PicRawSetting { get; set; } = true;

        [XmlAttribute]
        public bool PicTiffSetting { get; set; } = true;

        [XmlAttribute]
        public bool VidAviSetting { get; set; } = true;

        [XmlAttribute]
        public bool VidFlvSetting { get; set; } = true;

        [XmlAttribute]
        public bool VidMovSetting { get; set; } = true;

        [XmlAttribute]
        public bool VidMp4Setting { get; set; } = true;

        [XmlAttribute]
        public bool VidMpgSetting { get; set; } = true;

        [XmlAttribute]
        public bool VidWmvSetting { get; set; } = true;

        [XmlAttribute]
        public bool VidMtsSetting { get; set; } = true;

        [XmlAttribute]
        public StringCollection GoogleAlbumsSetting { get; set; }


        public static void SetDefaults()
        {
            Serialize();
        }

        /// <summary>
        /// Save setting into file
        /// </summary>
        public static void Serialize()
        {
            // Create the directory
            if (!Directory.Exists(AppTmpFolder))
            {
                Directory.CreateDirectory(AppTmpFolder);
            }

            using TextWriter writer = new StreamWriter(SettingsFilePath);
            XmlSerializer serializer = new(typeof(Settings));
            serializer.Serialize(writer, Settings.Instance);
        }

        /// <summary>
        /// Load setting from file
        /// </summary>
        public static void Deserialize()
        {
            if (!System.IO.File.Exists(SettingsFilePath))
            {
                // Can't find saved settings, using default vales
                SetDefaults();
                return;
            }
            else
            {
                try
                {
                    using XmlReader reader = XmlReader.Create(SettingsFilePath);
                    XmlSerializer serializer = new(typeof(Settings));
                    if (serializer.CanDeserialize(reader))
                    {
                        Settings.instance = serializer.Deserialize(reader) as Settings;
                    }
                }
                catch (System.Exception)
                {
                    // Failed to load some data, leave the settings to default
                    SetDefaults();
                }
            }
        }
    }
}