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
        public List<TextNode> preNodes;
        // 后节点
        public List<TextNode> postNodes;



        public TextNode() { }
        // TODO
        public TextNode(string name) 
        {

        }
    }
}
