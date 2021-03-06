﻿using System;
using System.IO;
using UnityEngine;
using IniParser;
using IniParser.Parser;

namespace UnityExplorer.Config
{
    public class ModConfig
    {
        public static ModConfig Instance;

        internal static readonly IniDataParser _parser = new IniDataParser();
        internal static readonly string INI_PATH = Path.Combine(ExplorerCore.EXPLORER_FOLDER, "config.ini");

        static ModConfig()
        {
            _parser.Configuration.CommentString = "#";
        }

        // Actual configs
        public KeyCode  Main_Menu_Toggle    = KeyCode.F7;
        public bool     Force_Unlock_Mouse  = true;
        public int      Default_Page_Limit  = 25;
        public string   Default_Output_Path = ExplorerCore.EXPLORER_FOLDER + @"\Output";
        public bool     Log_Unity_Debug     = false;
        public bool     Hide_On_Startup     = false;
        //public bool     Save_Logs_To_Disk   = true;

        public static event Action OnConfigChanged;

        internal static void InvokeConfigChanged()
        {
            OnConfigChanged?.Invoke();
        }

        public static void OnLoad()
        {
            Instance = new ModConfig();

            if (LoadSettings())
                return;

            SaveSettings();
        }

        public static bool LoadSettings()
        {
            if (!File.Exists(INI_PATH))
                return false;

            string ini = File.ReadAllText(INI_PATH);

            var data = _parser.Parse(ini);

            foreach (var config in data.Sections["Config"])
            {
                switch (config.KeyName)
                {
                    case nameof(Main_Menu_Toggle):
                        Instance.Main_Menu_Toggle = (KeyCode)Enum.Parse(typeof(KeyCode), config.Value);
                        break;
                    case nameof(Force_Unlock_Mouse):
                        Instance.Force_Unlock_Mouse = bool.Parse(config.Value);
                        break;
                    case nameof(Default_Page_Limit):
                        Instance.Default_Page_Limit = int.Parse(config.Value);
                        break;
                    case nameof(Log_Unity_Debug):
                        Instance.Log_Unity_Debug = bool.Parse(config.Value);
                        break;
                    case nameof(Default_Output_Path):
                        Instance.Default_Output_Path = config.Value;
                        break;
                    case nameof(Hide_On_Startup):
                        Instance.Hide_On_Startup = bool.Parse(config.Value);
                        break;
                    //case nameof(Save_Logs_To_Disk):
                    //    Instance.Save_Logs_To_Disk = bool.Parse(config.Value);
                    //    break;
                }
            }

            return true;
        }

        public static void SaveSettings()
        {
            var data = new IniParser.Model.IniData();

            data.Sections.AddSection("Config");

            var sec = data.Sections["Config"];
            sec.AddKey(nameof(Main_Menu_Toggle),    Instance.Main_Menu_Toggle.ToString());
            sec.AddKey(nameof(Force_Unlock_Mouse),  Instance.Force_Unlock_Mouse.ToString());
            sec.AddKey(nameof(Default_Page_Limit),  Instance.Default_Page_Limit.ToString());
            sec.AddKey(nameof(Log_Unity_Debug),     Instance.Log_Unity_Debug.ToString());
            sec.AddKey(nameof(Default_Output_Path), Instance.Default_Output_Path);
            sec.AddKey(nameof(Hide_On_Startup),     Instance.Hide_On_Startup.ToString());
            //sec.AddKey("Save_Logs_To_Disk",     Instance.Save_Logs_To_Disk.ToString());

            File.WriteAllText(INI_PATH, data.ToString());
        }
    }
}
