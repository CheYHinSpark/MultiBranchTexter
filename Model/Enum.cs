using System;
using System.Collections.Generic;
using System.Text;

namespace MultiBranchTexter.Model
{
    /// <summary>
    /// 节点状态枚举
    /// </summary>
    public enum NodeState
    {
        Normal = 0,
        Selected = 1,
        Searched = 2,
        TopSearched = 3
    }

    /// <summary>
    /// 后继节点类型
    /// </summary>
    public enum EndType
    {
        Single = 0,
        YesNo = 1,
        MultiAnswers = 2
    }
}
