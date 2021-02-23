using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace MultiBranchTexter.Model
{
    /// <summary>
    /// 文本节点类
    /// </summary>
    public class TextNode
    {
        // 文件名
        public string Name = "";
        // 文字内容
        //public string Text { get; set; }
        // 后继条件
        public EndCondition endCondition;
        // 前节点
        //[JsonIgnore]
        private readonly List<TextNode> preNodes = new List<TextNode>();
        // 后节点
        //[JsonIgnore]
        //public List<TextNode> PostNodes { get; private set; } = new List<TextNode>();

        //文字内容
        public List<TextFragment> Fragments = new List<TextFragment>();
        
        public TextNode() 
        {
            endCondition = new EndCondition();
        }
        
        public TextNode(string name, string text)
        {
            Name = name;
            Fragments.Add(new TextFragment(text));
            endCondition = new EndCondition();
        }

        #region 添加与删除前后节点
        #region 静态方法
     
        /// <summary>
        /// 连接，注意不能在对键值的遍历中搞这个
        /// /// </summary>
        public static void Link(TextNode pre, TextNode post, string answer)
        {
            post.AddPreNode(pre);

            if (pre.endCondition.Answers.ContainsKey(answer))
            {
                pre.endCondition.Answers[answer] = post.Name;
            }
            else
            { pre.endCondition.Answers.Add(answer, post.Name); }
        }

        /// <summary>
        /// 断开，注意不能在对键值的遍历中搞这个
        /// </summary>
        public static void UnLink(TextNode pre, TextNode post, string answer)
        {
            post.DeletePreNode(pre);

            if (pre.endCondition.Answers.ContainsKey(answer))
            {
                if (pre.endCondition.Answers[answer] == post.Name)
                { pre.endCondition.Answers.Remove(answer); }
            }
        }
        #endregion

        public void AddPreNode(TextNode node)
        {
            //TODO 判断是否已经存在
            preNodes.Add(node);
        }

        public void DeletePreNode(TextNode node)
        {
            //TODO 判断是否已经存在
            preNodes.Remove(node);
        }
        #endregion

        [Obsolete]
        public int GetPostNodeIndex(List<TextNodeWithLeftTop> textNodes)
        {
            for (int i = 0; i < textNodes.Count; i++)
            {
                if (endCondition.Answers.ContainsValue(textNodes[i].Node.Name))
                { return i; }
            }
            return -1;//可能没有后继了
        }
    
        public static List<TextNode> DeserializeFromFile(string file)
        {
            string txt = File.ReadAllText(file);
            return JsonConvert.DeserializeObject<List<TextNode>>(txt);
        }

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
}
