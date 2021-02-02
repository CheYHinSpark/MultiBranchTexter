using System.Collections.Generic;

namespace MultiBranchTexter.Model
{
    public class TextNode
    {
        // 文件名
        public string Name = "";
        // 文字内容
        public string Text = "";
        // 后继条件
        public EndCondition endCondition;
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

        #region 添加与删除前后节点
        #region 静态方法
        public static void Link(TextNode pre, TextNode post)
        {
            //pre清除所有post
            pre.ClearAllPostNode();
            pre.AddPostNode(post);
            post.AddPreNode(pre);
        }
        public static void Link(TextNode pre, TextNode post, bool yesNo)
        {
            if (!(pre.endCondition is YesNoCondition))
            {
                //TODO:删除
                pre.ClearAllPostNode();
                pre.endCondition = new YesNoCondition();
            }
            pre.AddPostNode(post);
            post.AddPreNode(pre);
            //修改后继条件
            if (yesNo)
            { (pre.endCondition as YesNoCondition).YesNode = post; }
            else
            { (pre.endCondition as YesNoCondition).NoNode = post; }
        }
        public static void Link(TextNode pre, TextNode post, string answer)
        {
            pre.AddPostNode(post);
            post.AddPreNode(pre);
            //TODO:修改后继条件
        }

        public static void UnLink(TextNode pre, TextNode post)
        {
            pre.DeletePostNode(post);
            post.DeletePreNode(pre);
        }
        
        public static void UnLink(TextNode pre, TextNode post, bool yesNo)
        {
            pre.DeletePostNode(post);
            post.DeletePreNode(pre);
            //修改后继条件
            if (pre.endCondition is YesNoCondition)
            {
                if (yesNo)
                { (pre.endCondition as YesNoCondition).YesNode = null; }
                else
                { (pre.endCondition as YesNoCondition).NoNode = null; }
            }
        }

        public static void UnLink(TextNode pre, TextNode post, string answer)
        {
            pre.DeletePostNode(post);
            post.DeletePreNode(pre);
            //TODO:修改后继条件
            if (pre.endCondition is MultiAnswerCondition)
            {

            }
        }
        #endregion

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
        public void DeletePreNode(TextNode node)
        {
            //TODO 判断是否已经存在
            preNodes.Remove(node);
        }
        public void DeletePostNode(TextNode node)
        {
            //TODO 判断是否已经存在
            postNodes.Remove(node);
        }
        public void ClearAllPostNode()
        {
            postNodes.Clear();
        }
        #endregion

        /// <summary>
        /// 得到postNodes在参数List中的指标，目前是根据内存地址判断
        /// </summary>
        /// <param name="textNodes"></param>
        /// <returns></returns>
        public List<int> GetPostNodeIndex(List<TextNode> textNodes)
        {
            List<int> vs = new List<int>();
            for (int i = 0; i < textNodes.Count; i++)
            {
                if (postNodes.Contains(textNodes[i]))
                {
                    vs.Add(i);
                }
            }
            return vs;
        }
    }
}
