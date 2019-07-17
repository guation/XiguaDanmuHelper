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
        private bool textboxHasText = false;
        public ConfigData configData; 
        private bool isInt=false;

        public Setting()
        {
            InitializeComponent();
            configData = GetConfig?.Invoke();
            slider1.Value = configData.spd;
            slider2.Value = configData.pit;
            slider3.Value = configData.vol;
            switch (configData.per)
            {
                case 0:
                    RadioButton1.IsChecked = true;
                    TextBox.Text = RadioButton1.Content.ToString();
                    break;
                case 1:
                    RadioButton2.IsChecked = true;
                    TextBox.Text = RadioButton2.Content.ToString();
                    break;
                case 3:
                    RadioButton3.IsChecked = true;
                    TextBox.Text = RadioButton3.Content.ToString();
                    break;
                case 4:
                    RadioButton4.IsChecked = true;
                    TextBox.Text = RadioButton4.Content.ToString();
                    break;
            }
            if (configData.BlackList == "")
            {
                textboxHasText = false;
            }
            else
            {
                textboxHasText = true;
                Textbox.Text = configData.BlackList;
            }
            isInt = true;
        }


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
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("    欢迎使用西瓜直非官方助手。\n    本软件（当前分支）目前由挂神个人维护。\n    软件在使用过程出现问题可与挂神联系，如果您觉得此软件对您有帮助也可以考虑赞助本软件的开发。\n    本软件完全开源，项目地址：https://github.com/guation/XiguaDanmuHelper \n    您可以在不违反协议的情况下自行进行二次开发。" , "关于本软件");
        }

        private void Slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(isInt) configData.spd = (int)slider1.Value;
        }

        private void Slider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isInt) configData.pit = (int)slider2.Value;
        }

        private void Slider3_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isInt) configData.vol = (int)slider3.Value;
        }

        private void RadioButton1_Checked(object sender, RoutedEventArgs e)
        {
            if (isInt) configData.per = 0;
        }

        private void RadioButton2_Checked(object sender, RoutedEventArgs e)
        {
            if (isInt) configData.per = 1;
        }

        private void RadioButton3_Checked(object sender, RoutedEventArgs e)
        {
            if (isInt) configData.per = 3;
        }

        private void RadioButton4_Checked(object sender, RoutedEventArgs e)
        {
            if (isInt) configData.per = 4;
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textboxHasText) configData.BlackList = Textbox.Text.ToLower();
        }
    }
}
