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

        public List<TextFragment> Fragments = new List<TextFragment>();
        
        public TextNode() 
        {
            endCondition = new SingleEndCondition();
        }
        
        public TextNode(string name, string text)
        {
            Name = name;
            Fragments.Add(new TextFragment(text));
            endCondition = new SingleEndCondition();
        }

        #region 添加与删除前后节点
        #region 静态方法
        /// <summary>
        /// 单后继连接
        /// </summary>
        public static void Link(TextNode pre, TextNode post)
        {
            //pre清除所有post
            //pre.ClearAllPostNode();
            //pre.AddPostNode(post);
            pre.endCondition = new SingleEndCondition() { NextNodeName = post.Name };
            post.AddPreNode(pre);
        }
     
        /// <summary>
        /// 多后继连接
        /// </summary>
        public static void Link(TextNode pre, TextNode post, string answer)
        {
            if (!(pre.endCondition is UniversalEndCondition))
            {
                //删除
                //pre.ClearAllPostNode();
                pre.endCondition = new UniversalEndCondition(true);
            }
            //pre.AddPostNode(post);
            post.AddPreNode(pre);
            //修改后继条件
            UniversalEndCondition uec = pre.endCondition as UniversalEndCondition;
            if (uec.Answers.ContainsKey(answer))
            { uec.Answers[answer] = post.Name; }
            else
            { uec.Answers.Add(answer, post.Name); }
        }

        public static void UnLink(TextNode pre, TextNode post)
        {
            //pre.DeletePostNode(post);
            post.DeletePreNode(pre);
            pre.endCondition = new SingleEndCondition() { NextNodeName = "" };
        }
        

        public static void UnLink(TextNode pre, TextNode post, string answer)
        {
            //pre.DeletePostNode(post);
            post.DeletePreNode(pre);
            //修改后继条件
            if (pre.endCondition is UniversalEndCondition)
            {
                UniversalEndCondition uec = pre.endCondition as UniversalEndCondition;
                if (uec.Answers.ContainsKey(answer))
                {
                    if (uec.Answers[answer] == post.Name)
                    { uec.Answers.Remove(answer); }
                }
            }
        }
        #endregion

        public void AddPreNode(TextNode node)
        {
            //TODO 判断是否已经存在
            preNodes.Add(node);
        }
        //public void AddPostNode(TextNode node)
        //{
        //    //TODO 判断是否已经存在
        //    PostNodes.Add(node);
        //}
        public void DeletePreNode(TextNode node)
        {
            //TODO 判断是否已经存在
            preNodes.Remove(node);
        }
        //public void DeletePostNode(TextNode node)
        //{
        //    //TODO 判断是否已经存在
        //    PostNodes.Remove(node);
        //}
        //public void ClearAllPostNode()
        //{
        //    PostNodes.Clear();
        //}
        #endregion

        public int GetPostNodeIndex(List<TextNodeWithLeftTop> textNodes)
        {
            if (endCondition is SingleEndCondition)
            {
                string name = (endCondition as SingleEndCondition).NextNodeName;
                for (int i = 0; i < textNodes.Count; i++)
                {
                    if (textNodes[i].Node.Name == name)
                    { return i; }
                }
            }
            else if (endCondition is UniversalEndCondition)
            {
                UniversalEndCondition uec = endCondition as UniversalEndCondition;

                for (int i = 0; i < textNodes.Count; i++)
                {
                    if (uec.Answers.ContainsValue(textNodes[i].Node.Name))
                    { return i; }
                }
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
