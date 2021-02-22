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
        Normal,
        Searched,
        Selected
    }

    /// <summary>
    /// 后继节点类型
    /// </summary>
    public enum EndType
    {
        Single,
        YesNo,
        MultiAnswers
    }
}
