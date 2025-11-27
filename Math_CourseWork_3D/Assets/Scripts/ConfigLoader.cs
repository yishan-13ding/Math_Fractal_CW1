using System.Globalization;
using System.IO;  // when using txt file
using UnityEngine;

public static class ConfigLoader //static不能使用 new ConfigLoader() 创建实例对象,只能直接调用静态方法
{
    public static FractalConfig Load(string filePath)  
    {
        FractalConfig config = new FractalConfig();

        if (!File.Exists(filePath))
        {
            Debug.LogError("找不到配置文件: " + filePath);
            return null;
        }

        foreach (string line in File.ReadAllLines(filePath))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;   // 跳过空行
            if (line.StartsWith("#")) continue;              // 跳过注释行

            string[] parts = line.Split('=');
            if (parts.Length != 2) continue;

            string key = parts[0].Trim();
            string value = parts[1].Trim();

            switch (key)
            {
                case "type": config.type = value; break;
                case "initialShape": config.initialShape = value; break;
                case "iteration": config.iteration = int.Parse(value); break;
                case "size": config.size = float.Parse(value); break;
                case "centerX": config.centerX = float.Parse(value); break;
                case "centerY": config.centerY = float.Parse(value); break;
                case "offsetAngle": config.offsetAngle = float.Parse(value); break;
                case "drawMode": config.drawMode = value; break;
            }
        }
        return config;
    }
}
