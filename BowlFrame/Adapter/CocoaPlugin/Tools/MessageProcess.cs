using BowlFrame.Config;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BowlFrame.Adapter.CocoaPlugin.Tools
{
    public class MessageProcess
    {
        private readonly string _filePath;

        private readonly IPlatform _platform;

        private readonly string _lang;

        private readonly Dictionary<string, string> _placeholders;

        public MessageOption MessageOption { get; set; }

        private readonly Dictionary<string, List<string>> _messages = [];

        public MessageProcess(string filePath, string fileName, IPlatform platform, string lang = "zh_cn", bool noDefaultMsg = false)
        {
            filePath = Path.GetFullPath(filePath);

            _filePath = Path.Combine(filePath, fileName + "." + lang);

            if (!File.Exists(_filePath))
                throw new FileNotFoundException("未找到语言文件", _filePath);

            _platform = platform;
            _lang = lang;

            if (!noDefaultMsg)
                if (File.Exists(Path.Combine(PathConfig.ConfigPath, "Messages", $"Global.{lang}")))
                    LoadMsgFromJson(JObject.Parse(Path.Combine(PathConfig.ConfigPath, "Messages", $"Global.{lang}")));

            _placeholders = new()       //内置占位符
            {
                { "TimeAsk", DateTime.UtcNow.AddHours(MessageOption.TimeZone).Hour switch { int i when i < 5 => "{Early_Morning}", int i when (i < 8) => "{Morning}", int i when (i < 11) => "{A.M.}", int i when (i < 13) => "{Noon}", int i when (i < 17) => "{P.M.}", int i when (i < 18) => "{Evening}", int i when (i < 24) => "{Night}", _ => "", } },
                { "WeekDay", ((int)DateTime.Now.DayOfWeek).ToString() }
            };
        }

        public void AddPlaceholder(string key, string value) => _placeholders.Add(key, value);

        public void AddPlaceholders(IDictionary<string, string> keyValues) => _ = keyValues.Select(x => { _placeholders.Add(x.Key, x.Value); return true; });

        public void RemovePlaceholder(string key) => _placeholders.Remove(key);

        public void RemovePlaceholders(IList<string> keys) => _ = keys.Select(_placeholders.Remove);

        public void LoadMsgFromJson(JObject json)
        {
            MessageOption? messageOption = null;

            //寻找是否存在Setting字段
            if (json.TryGetValue("Setting", out JToken? value))
                messageOption = value.ToObject<MessageOption>();

            foreach (KeyValuePair<string, JToken?> keyValue in messageOption is null ? json : (JObject?)json["Messages"] ?? [])
            {
                if (!_messages.ContainsKey(keyValue.Key))
                    _messages.Add(keyValue.Key, []);

                _messages.TryGetValue(keyValue.Key, out List<string>? list);

                if (list is null)
                    throw new NullReferenceException();

                if (keyValue.Value is not null)
                {
                    JToken token = keyValue.Value;

                    if (token.Type == JTokenType.Object)
                    {
                        JObject property = (JObject)token;

                        foreach (KeyValuePair<string, JToken?> keyValue_2 in property)
                        {
                            if (keyValue_2.Key != _platform.ID) continue;

                            token = keyValue_2.Value ?? new JArray();
                            break;
                        }

                        if (token == keyValue.Value)
                            token = token["Global"] ?? new JArray();
                    }

                    if (token.Type == JTokenType.Array)
                        foreach (JToken item in (JArray)token)
                            list.Add((string?)item ?? "Null");
                    else
                        list.Add((string?)token ?? "Null");
                }
            }
        }
    }

    public struct MessageOption
    {
        public short TimeZone { get; set; }

        public short TextDirection { get; set; }
    }
}