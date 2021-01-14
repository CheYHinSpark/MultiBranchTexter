using System;
using System.Collections.Generic;
using System.Text;

namespace MultiBranchTexter.Model
{
    public class TextNode
    {
        // 文件名
        public string Name;
        // 文字内容
        public string Text;
        // 前节点
        private List<TextNode> preNodes = new List<TextNode>();
        // 后节点
        private List<TextNode> postNodes = new List<TextNode>();
        // 后节点快速索引
        public List<int> postNodeIndexes = new List<int>();


        public TextNode() { }
        // TODO
        public TextNode(string name, string text)
        {
            Name = name;
            Text = text;
        }

        public void AddPreNode(TextNode node)
        {
            //TODO 判断是否已经存在
            preNodes.Add(node);
        }
        public void AddPostNode(TextNode node)
        {
            //TODO 判断是否已经存在
            postNodes.Add(node);
        }
        /// <summary>
        /// 得到postNodes在参数List中的指标，仅仅根据name判断Node是否相等
        /// </summary>
        /// <param name="textNodes"></param>
        /// <returns></returns>
        public List<int> GetPostNodeIndex(List<TextNode> textNodes)
        {
            List<int> vs = new List<int>();
            for (int i = 0; i < textNodes.Count; i++)
            {
                for (int j = 0; j < postNodes.Count; j++)
                {
                    if (textNodes[i].Name == postNodes[j].Name)
                    { vs.Add(i); }
                }
            }
            return vs;
        }
    }
}
