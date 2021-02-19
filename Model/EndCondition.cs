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

    public class SingleEndCondition : EndCondition
    {
        public string NextNodeName;
    }

    public class UniversalEndCondition : EndCondition
    {
        public bool IsExpression;
        public Dictionary<string, string> Answers = new Dictionary<string, string>();
    }

    [Obsolete]
    public class YesNoCondition : EndCondition
    {
        public TextNode YesNode;
        public TextNode NoNode;
    }

    [Obsolete]
    public class MultiAnswerCondition : EndCondition
    {
        public List<AnswerToNode> AnswerToNodes = new List<AnswerToNode>();
    }

    [Obsolete]
    public class AnswerToNode
    {
        public string Answer;
        public TextNode PostNode;
    }
}