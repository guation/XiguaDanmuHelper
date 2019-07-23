using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualBasic;
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
                    break;
                case 1:
                    RadioButton2.IsChecked = true;
                    break;
                case 3:
                    RadioButton3.IsChecked = true;
                    break;
                case 4:
                    RadioButton4.IsChecked = true;
                    break;
            }
            Textbox.Text = configData.BlackList;
            slider0.Value = configData.maxCapacity;
            isInt = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("    欢迎使用西瓜直非官方助手。\n    本软件原作者 q792602257 ，当前分支由 挂神 维护。\n    软件在使用过程出现问题可与挂神联系（哔哩哔哩@I挂神I 西瓜视频@sy挂神 GitHub@guation），如果您觉得此软件对您有帮助也可以考虑赞助本软件的开发，挂神的QQ群：291283968，挂神的QQ：1853306918。\n    本软件完全开源，项目地址：https://github.com/guation/XiguaDanmuHelper \n    您可以在不违反协议的情况下自行进行二次开发。", "关于本软件");
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            string str = Interaction.InputBox("用户反馈暂未开放", "用户反馈", null, -1, -1);
        }
        private void Slider0_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isInt) configData.maxCapacity = (int)slider0.Value;
        }

        private void Slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isInt) configData.spd = (int)slider1.Value;
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
            configData.per = 0;
            TextBox.Text = RadioButton1.Content.ToString();
        }

        private void RadioButton2_Checked(object sender, RoutedEventArgs e)
        {
            configData.per = 1;
            TextBox.Text = RadioButton2.Content.ToString();
        }

        private void RadioButton3_Checked(object sender, RoutedEventArgs e)
        {
            configData.per = 3;
            TextBox.Text = RadioButton3.Content.ToString();
        }

        private void RadioButton4_Checked(object sender, RoutedEventArgs e)
        {
            configData.per = 4;
            TextBox.Text = RadioButton4.Content.ToString();
        }

        private void Clossing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            configData.BlackList = Textbox.Text;
        }
    }
}
