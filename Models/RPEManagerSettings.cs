using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Microsoft.Win32;
using System.Windows;

namespace RPEFluentManager.Models
{
    public class RPEManagerSettings
    {
        [JsonPropertyName("ResourcePath")]
        public string ResourcePath {get; set;}

    }

    public class SettingsHandler
    {

        static string settingsPath = "Settings.json";

        public static RPEManagerSettings GetSettings()
        {
            if (!File.Exists(settingsPath))
            {

                OpenFileDialog openFileDialog = new OpenFileDialog() { FileName = "PhiEdit.exe", Title = "第一次打开时请先选择要管理的RPE可执行文件" };
                if (openFileDialog.ShowDialog() == true)
                {
                    var initialSettings = new RPEManagerSettings() { ResourcePath = Path.Combine(Path.GetDirectoryName(openFileDialog.FileName), "Resources") };
                    string json = JsonSerializer.Serialize(initialSettings);
                    File.WriteAllText(settingsPath, json);
                }
                else
                {
                    Application.Current.Shutdown();
                }
            }
            return JsonSerializer.Deserialize<RPEManagerSettings>(File.ReadAllText(settingsPath));
        }

        public static void SaveSettings(RPEManagerSettings settings)
        {
            string json = JsonSerializer.Serialize(settings);
            File.WriteAllText(settingsPath, json);
        }

    }
}
