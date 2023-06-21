using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

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
            // 检测设置文件是否存在
            if (!File.Exists(settingsPath))
            {
                // 创建新的设置文件，并写入初始设置
                var initialSettings = new RPEManagerSettings(); // 自定义设置对象
                initialSettings.ResourcePath = "F:\\Program Files (x86)\\RPEv1.2\\Resources";
                string json = JsonSerializer.Serialize(initialSettings);
                File.WriteAllText(settingsPath, json);
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
