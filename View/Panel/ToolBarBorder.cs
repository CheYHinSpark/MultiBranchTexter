using System.Windows;
using System.Windows.Controls;

namespace MultiBranchTexter.View
{
    public class ToolBarBorder : Border
    {
        public static DependencyProperty IsFullScreenProperty =
         DependencyProperty.Register("IsFullScreen", //属性名称
             typeof(bool), //属性类型
             typeof(ToolBarBorder), //该属性所有者，即将该属性注册到那个类上
             new PropertyMetadata(false)//属性默认值
             );

        public bool IsFullScreen
        {
            get { return (bool)GetValue(IsFullScreenProperty); }
            set { SetValue(IsFullScreenProperty, value); }
        }
        public ToolBarBorder()
        { }
    }
}
