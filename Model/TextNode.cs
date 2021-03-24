using System.Collections.Generic;

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
            for (int i = 0; i < pre.EndCondition.Answers.Count; i++)
            {
                if (pre.EndCondition.Answers[i].Item1 == answer)
                {
                    pre.EndCondition.Answers[i] = (answer, post.Name, pre.EndCondition.Answers[i].Item3);
                    return;
                }
            }
            pre.EndCondition.Answers.Add((answer, post.Name, ""));
        }

        /// <summary>
        /// 断开，注意不能在对键值的遍历中搞这个
        /// </summary>
        public static void UnLink(TextNode pre, string answer)
        {
            for (int i = 0; i < pre.EndCondition.Answers.Count; i++)
            {
                if (pre.EndCondition.Answers[i].Item1 == answer)
                {
                    pre.EndCondition.Answers[i] = (answer, "", pre.EndCondition.Answers[i].Item3);
                    return;
                }
            }
        }
        #endregion

        /// <summary>
        /// JSON保存，游戏开发用
        /// </summary>
        public OperationTextNode ToOperationTextNode()
        {
            List<OperationTextFragment> newFragments = new List<OperationTextFragment>();
            for (int i =0;i<Fragments.Count;i++)
            { newFragments.Add(Fragments[i].ToOperation()); }
            return new OperationTextNode() { Name = Name, Fragments = newFragments, EndCondition = EndCondition };
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

    /// <summary>
    /// 转换为operation的文本节点类 
    /// JSON保存，游戏开发用
    /// </summary>
    public class OperationTextNode
    {
        // 文件名
        public string Name = "";

        // 后继条件
        public EndCondition EndCondition;

        //文字内容
        public List<OperationTextFragment> Fragments = new List<OperationTextFragment>();

        public OperationTextNode()
        {
            EndCondition = new EndCondition();
        }

        public TextNode ToTextNode()
        {
            TextNode textNode = new TextNode
            {
                Name = Name,
                EndCondition = EndCondition
            };
            for (int i = 0;i< Fragments.Count;i++)
            {
                textNode.Fragments.Add(Fragments[i].ToTextFragment());
            }
            return textNode;
        }

        public static List<TextNode> ToTextNodeList(List<OperationTextNode> oldNodes)
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
