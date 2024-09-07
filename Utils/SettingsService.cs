using System.Text.Json;
using System.Xml;

namespace Utils;

public class SettingsService
{
    private static SettingsService? _instance;

    public static SettingsService Instance
    {
        get
        {
            _instance ??= new SettingsService();
            return _instance;
        }
    }

    private Dictionary<string, string?> _dictionary = new();
    private string _path;

    private SettingsService()
    {
        _path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SergeiKrivko",
            "GPT-chat");
        Directory.CreateDirectory(_path);
        _path = Path.Join(_path, "settings.xml");
        LoadSettings();
    }

    private void LoadSettings()
    {
        var document = new XmlDocument();
        try
        {
            document.Load(_path);
            if (document.FirstChild != null)
            {
                var root = document.SelectSingleNode("settings");
                if (root != null)
                {
                    for (var i = 0; i < root.ChildNodes.Count; i++)
                    {
                        var node = root.ChildNodes[i];
                        if (node?.Attributes != null)
                        {
                            // _dictionary[node.Attributes["key"]?.Value ?? ""] = node.Attributes["value"]?.Value;
                            _dictionary[node.Attributes["key"]?.Value ?? ""] = node.InnerText;
                        }
                    }
                }
            }
        }
        catch (FileNotFoundException)
        {
        }
    }

    private void StoreSettings()
    {
        var document = new XmlDocument();
        var xmlDeclaration = document.CreateXmlDeclaration("1.0", "UTF-8", null);
        document.AppendChild(xmlDeclaration);
        
        var root = document.CreateElement("settings");
        document.AppendChild(root);
        
        foreach (var item in _dictionary)
        {
            if (item.Value != null)
            {
                var tag = document.CreateElement("record");
                tag.SetAttribute("key", item.Key);
                tag.InnerText = item.Value;
                // tag.SetAttribute("value", item.Value);
                root.AppendChild(tag);
            }
        }

        document.Save(_path);
    }

    public void Set(string key, string? value)
    {
        _dictionary[key] = value;
        StoreSettings();
    }
    
    public void Set(string key, object? obj)
    {
        Set(key, JsonSerializer.Serialize(obj));
    }

    public void Remove(string key)
    {
        _dictionary.Remove(key);
    }

    public string? Get(string key)
    {
        if (_dictionary.TryGetValue(key, out var res))
            return res;
        return null;
    }

    public string Get(string key, string defaultValue)
    {
        if (_dictionary.TryGetValue(key, out var res))
            return res ?? defaultValue;
        return defaultValue;
    }

    public T Get<T>(string key, T defaultValue)
    {
        var res = Get<T>(key);
        return res ?? defaultValue;
    }

    public T? Get<T>(string key)
    {
        var str = Get(key);
        if (str == null)
            return default;
        try
        {
            return JsonSerializer.Deserialize<T>(str);
        }
        catch (JsonException e)
        {
            return default;
        }
    }
}