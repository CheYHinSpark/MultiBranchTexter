using System.Windows;
using System.Windows.Controls;

namespace MultiBranchTexter.Controls
{
    /// <summary>
    /// HintTextBlock.xaml 的交互逻辑
    /// </summary>
    public partial class HintTextBlock : UserControl
    {
        public static DependencyProperty IsAwakeProperty =
            DependencyProperty.Register("IsAwake", //属性名称
                typeof(bool), //属性类型
                typeof(HintTextBlock), //该属性所有者，即将该属性注册到那个类上
                new PropertyMetadata(false)//属性默认值
                );

        public bool IsAwake
        {
            get { return (bool)GetValue(IsAwakeProperty); }
            set { SetValue(IsAwakeProperty, value); }
        }

        public static DependencyProperty InTextProperty =
           DependencyProperty.Register("InText", //属性名称
               typeof(string), //属性类型
               typeof(HintTextBlock), //该属性所有者，即将该属性注册到那个类上
               new PropertyMetadata("")//属性默认值
               );

        public string InText
        {
            get { return (string)GetValue(InTextProperty); }
            set { SetValue(InTextProperty, value); }
        }

         public static DependencyProperty OutTextProperty =
           DependencyProperty.Register("OutText", //属性名称
               typeof(string), //属性类型
               typeof(HintTextBlock), //该属性所有者，即将该属性注册到那个类上
               new PropertyMetadata("")//属性默认值
               );

        public string OutText
        {
            get { return (string)GetValue(OutTextProperty); }
            set { SetValue(OutTextProperty, value); }
        }

        public HintTextBlock()
        {
            InitializeComponent();
        }
    }
}
