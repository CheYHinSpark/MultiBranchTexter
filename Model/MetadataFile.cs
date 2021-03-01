using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using MultiBranchTexter.ViewModel;
using System.Windows;

namespace MultiBranchTexter.Model
{
    class MetadataFile
    {
        public Dictionary<string, (double left, double top)> coordinates = new Dictionary<string, (double left, double top)>();
        public string nodeFilePath;

        private List<TextNodeWithLeftTop> SetNodeCoordinate(List<TextNode> nodes)
        {
            return nodes.Select(n =>
            {
                string name = n.Name;
                if (coordinates.TryGetValue(name, out (double left, double top) c))
                {
                    return new TextNodeWithLeftTop(n, c.left, c.top);
                }
                else
                {
                    Debug.WriteLine($"未找到节点{n.Name}坐标");
                    ViewModelFactory.Main.IsModified = true;//这时相当于已经有修改了
                    return new TextNodeWithLeftTop(n, 0, 0);
                }
            }).ToList();
        }

        public static List<TextNodeWithLeftTop> ReadTextNodes(string metaUrl)
        {
            return JsonConvert.DeserializeObject<List<TextNodeWithLeftTop>>(File.ReadAllText(metaUrl));
        }

        [Obsolete]
        public static List<TextNodeWithLeftTop> ReadVeryNodes(string metaUrl)
        {
            var meta = JsonConvert.DeserializeObject<MetadataFile>(File.ReadAllText(metaUrl)) ?? new MetadataFile();
            if (!File.Exists(meta.nodeFilePath))
            {
                //如果记载的没有找到，查找本地
                string tryUrl = new Regex(@"\.mbjson$").Replace(metaUrl, ".json");
                if (!File.Exists(tryUrl))
                {
                    MessageBox.Show("无法找到对应的json文件！\n" +
                        "请用文本文档打开该mbjson文件编辑末尾的\"nodeFilePath\"项。");
                    ViewModelFactory.Main.IsWorkGridVisible = false;
                    throw new FormatException("未找到对应脚本文件");
                }
                meta.nodeFilePath = tryUrl;
            }
            var list = JsonConvert.DeserializeObject<List<OldTextNode>>(File.ReadAllText(meta.nodeFilePath));
            return meta.SetNodeCoordinate(OldTextNode.ToTextNodeList(list));
        }

        [Obsolete]
        public static List<TextNodeWithLeftTop> ReadOldNodes(string metaUrl)
        {
            List<OldTextNodeWithLeftTop> data = JsonConvert.DeserializeObject<List<OldTextNodeWithLeftTop>>(File.ReadAllText(metaUrl));
            List<TextNodeWithLeftTop> newd = new List<TextNodeWithLeftTop>();
            for (int i = 0; i < data.Count; i++)
            {
                newd.Add(data[i].ToNew());
            }
            return newd;
        }

        public static void WriteTextNodes(string metaUrl, List<TextNodeWithLeftTop> nodes)
        {
            File.WriteAllText(metaUrl, JsonConvert.SerializeObject(nodes, Formatting.Indented));
        }

        [Obsolete]//以后导出可以用用看
        public static void WriteNodes(string metaUrl, List<TextNodeWithLeftTop> nodes)
        {
            var nodeUrl = new Regex(@"\.mbjson$", RegexOptions.IgnoreCase).Replace(metaUrl, ".json");
            if (nodeUrl.ToLowerInvariant() == metaUrl.ToLowerInvariant())
            {
                nodeUrl = new Regex(@"\.json$", RegexOptions.IgnoreCase).Replace(nodeUrl, ".node.json");
            }
            if (nodeUrl.ToLowerInvariant() == metaUrl.ToLowerInvariant())
            {
                throw new Exception("wtf?");
            }
            var nodeToSave = nodes.Select(n => n.Node).ToList();

            var m = new MetadataFile();
            foreach (var n in nodes)
            {
                m.coordinates[n.Node.Name] = (n.Left, n.Top);
            }
            m.nodeFilePath = nodeUrl;

            File.WriteAllText(nodeUrl, JsonConvert.SerializeObject(nodeToSave, Formatting.Indented));
            File.WriteAllText(metaUrl, JsonConvert.SerializeObject(m, Formatting.Indented));
        }
    }
}
