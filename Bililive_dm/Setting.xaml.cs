using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using XiguaDanmakuHelper;

namespace Bililive_dm
{
    /// <summary>
    /// Setting.xaml 的交互逻辑
    /// </summary>
    public delegate ConfigData getConfig();
    public delegate void setConfig(ConfigData configData);

    public partial class Setting : Window
    {
        public static event getConfig GetConfig;
        public static event setConfig SetConfig;

        public ConfigData ConfigData; 

        public Setting()
        {
            InitializeComponent();
        }

        bool textboxHasText = false;

        private void Textbox_Enter(object sender, EventArgs e)
        {
            if (textboxHasText == false) Textbox.Text = "";
        }        //textbox失去焦点        
        private void Textbox_Leave(object sender, EventArgs e)
        {
            if (Textbox.Text == "")
            {
                Textbox.Text = "需要屏蔽的内容，以|为分割";
                textboxHasText = false;
            }
            else
                textboxHasText = true;
        }
    }
}
