using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace MultiBranchTexter.Model
{
    /// <summary> 文本节点类 </summary>
    public class TextNode
    {
        // 文件名
        public string Name = "";

        // 后继条件
        public EndCondition EndCondition;

        //文字内容
        public List<TextFragment> Fragments = new List<TextFragment>();
        
        public TextNode() 
        {
            EndCondition = new EndCondition();
        }

        public TextNode(string name)
        {
            Name = name;
            Fragments.Add(new TextFragment());
            EndCondition = new EndCondition();
        }

        #region 添加与删除前后节点 静态方法
     
        /// <summary>
        /// 连接，注意不能在对键值的遍历中搞这个
        /// /// </summary>
        public static void Link(TextNode pre, TextNode post, string answer)
        {
            if (pre.EndCondition.Answers.ContainsKey(answer))
            {
                pre.EndCondition.Answers[answer] = post.Name;
            }
            else
            { pre.EndCondition.Answers.Add(answer, post.Name); }
        }

        /// <summary>
        /// 断开，注意不能在对键值的遍历中搞这个
        /// </summary>
        public static void UnLink(TextNode pre, TextNode post, string answer)
        {
            if (pre.EndCondition.Answers.ContainsKey(answer))
            {
                if (pre.EndCondition.Answers[answer] == post.Name)
                { pre.EndCondition.Answers.Remove(answer); }
            }
        }
        #endregion

        [Obsolete]
        public static List<TextNode> DeserializeFromFile(string file)
        {
            string txt = File.ReadAllText(file);
            return JsonConvert.DeserializeObject<List<TextNode>>(txt);
        }

        [Obsolete]
        public static string SerializeToFile(List<TextNode> nodes, string file)
        {
            string s = JsonConvert.SerializeObject(nodes);
            File.WriteAllText(file, s);
            return s;
        }
    }


    /// <summary>
    /// 带左上角坐标的文本节点，用于读取和保存数据
    /// </summary>
    public class TextNodeWithLeftTop
    {
        public TextNode Node;
        public double Left;
        public double Top;
        public TextNodeWithLeftTop(TextNode node, double left, double top)
        {
            Node = node;
            Left = left;
            Top = top;
        }
    }

    /// <summary> 旧的文本节点类 </summary>
    [Obsolete]
    public class OldTextNode
    {
        // 文件名
        public string Name = "";

        // 后继条件
        public EndCondition endCondition;

        //文字内容
        public List<OldTextFragment> Fragments = new List<OldTextFragment>();

        public OldTextNode()
        {
            endCondition = new EndCondition();
        }

        public TextNode ToTextNode()
        {
            TextNode textNode = new TextNode
            {
                Name = Name,
                EndCondition = endCondition
            };
            for (int i = 0;i< Fragments.Count;i++)
            {
                textNode.Fragments.Add(Fragments[i].ToTextFragment());
            }
            return textNode;
        }

        public static List<TextNode> ToTextNodeList(List<OldTextNode> oldNodes)
        {
            List<TextNode> nodes = new List<TextNode>();
            for (int i =0;i<oldNodes.Count;i++)
            {
                nodes.Add(oldNodes[i].ToTextNode());
            }
            return nodes;
        }
    }
}
