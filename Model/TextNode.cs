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
    }
}
