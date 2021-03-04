using MultiBranchTexter.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MultiBranchTexter.View
{
    /// <summary>
    /// 节点基类
    /// </summary>
    public class NodeBase : UserControl
    {
        public NodeButton FatherNode { get; set; }

        /// <summary>
        /// 获取fathernode的节点，即是这个控件真正对应的节点
        /// </summary>
        public TextNode FatherTextNode { get { return FatherNode.TextNode; } }

        /// <summary>
        /// 获得左上角坐标
        /// </summary>
        public Point GetCanvasOffset()
        {
            return TransformToAncestor(FatherNode).Transform(new Point(0,0))
                + new Vector(Canvas.GetLeft(FatherNode), Canvas.GetTop(FatherNode));
        }
    }
}