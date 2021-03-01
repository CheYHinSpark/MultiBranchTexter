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

        public EndType EndType;

        public bool IsQuestion = true;

        public List<(string, string)> Answers = new List<(string, string)>();

        public EndCondition()
        {
            EndType = EndType.Single;
        }

        public EndCondition(EndType endType)
        {
            EndType = endType;
            if (EndType == EndType.Single)
            { Answers.Add(("", "")); }
            else if (EndType == EndType.YesNo)
            {
                Answers.Add(("yes", ""));
                Answers.Add(("no", ""));
            }
        }
    }

    [Obsolete]
    public class OldEndCondition
    {
        public string Question;

        public EndType EndType;

        public bool IsQuestion = true;

        public Dictionary<string, string> Answers = new Dictionary<string, string>();

        public OldEndCondition()
        {
            EndType = EndType.Single;
        }

        public OldEndCondition(EndType endType)
        {
            EndType = endType;
        }
        public EndCondition ToEndCondition()
        {
            EndCondition ec = new EndCondition();
            ec.Question = Question;
            ec.EndType = EndType;
            ec.IsQuestion = IsQuestion;
            ec.Answers = new List<(string, string)>();
            foreach (string answer in Answers.Keys)
            {
                ec.Answers.Add((answer, Answers[answer]));
            }

            return ec;
        }
    }
}