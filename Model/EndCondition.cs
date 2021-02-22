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
        private readonly bool isExpression;
        public bool IsExpression { get { return isExpression; } }//如果这个是false，则是yesno
        public Dictionary<string, string> Answers = new Dictionary<string, string>();
        public UniversalEndCondition(bool isexpression) 
        { 
            isExpression = isexpression;
            if (!isexpression)
            {
                Answers.Add("yes","");
                Answers.Add("no","");
            }
        }
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