using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MultiBranchTexter.View
{
    /// <summary>
    /// 拥有内发光的板子
    /// /// </summary>
    public partial class InnerGlowGrid : UserControl
    {
        public static DependencyProperty GlowLengthProperty =
            DependencyProperty.Register("GlowLength", //属性名称
                typeof(double), //属性类型
                typeof(InnerGlowGrid), //该属性所有者，即将该属性注册到那个类上
                new PropertyMetadata(60.0)//属性默认值
                );

        /// <summary> 内发光距离 </summary>
        public double GlowLength
        {
            get { return (double)GetValue(GlowLengthProperty); }
            set { SetValue(GlowLengthProperty, value); }
        }

        public static DependencyProperty GlowColorProperty =
           DependencyProperty.Register("GlowColor", //属性名称
               typeof(Brush), //属性类型
               typeof(InnerGlowGrid), //该属性所有者，即将该属性注册到那个类上
               new PropertyMetadata(Brushes.Transparent)//属性默认值
               );

        /// <summary> 内发光刷子 </summary>
        public Brush GlowColor
        {
            get { return (Brush)GetValue(GlowColorProperty); }
            set { SetValue(GlowColorProperty, value); }
        }

        public InnerGlowGrid()
        {
            InitializeComponent();
        }
    }
}
