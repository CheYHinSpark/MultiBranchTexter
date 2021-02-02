using System;
using System.Collections.Generic;
using System.Text;

namespace MultiBranchTexter.Model
{
    /// <summary>
    /// textNode的结尾条件
    /// </summary>
    public class EndCondition
    {
        public string Question;
    }

    public class YesNoCondition : EndCondition
    {
        public TextNode YesNode;
        public TextNode NoNode;
    }

    public class MultiAnswerCondition : EndCondition
    {
        public List<AnswerToNode> AnswerToNodes = new List<AnswerToNode>();
    }

    public struct AnswerToNode
    {
        public string Answer;
        public TextNode PostNode;
    }
}