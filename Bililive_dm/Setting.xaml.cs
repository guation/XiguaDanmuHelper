﻿using GT_XiguaAPI;
using Microsoft.VisualBasic;
using NAudio.Wave;
using System;
using System.Management;
using System.Windows;
using System.Windows.Controls;
//using XiguaDanmakuHelper;


namespace Bililive_dm
{
    /// <summary>
    /// Setting.xaml 的交互逻辑
    /// </summary>
    public delegate ConfigData GetConfig();
    public delegate YuYin GetYuyin();

    public partial class Setting : Window
    {
        public static event GetConfig GetConfig;
        public static event GetYuyin GetYuyin;
        public ConfigData configData;
        private bool isInt = false;


        public Setting()
        {
            InitializeComponent();
            configData = GetConfig?.Invoke();
            slider1.Value = configData.spd;
            slider2.Value = configData.pit;
            slider3.Value = configData.vol;
            slider4.Value = configData.maxSize;
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
            audiolist.Items.Add("Windows默认输出设备");
            audiolist.SelectedIndex = 0;

            for (int deviceid = 0; deviceid < WaveOut.DeviceCount; deviceid++)
            {
                var capabilities = WaveOut.GetCapabilities(deviceid);
                audiolist.Items.Add(capabilities.ProductName);
                if (configData.AudioDeviceName == capabilities.ProductName)
                    audiolist.SelectedIndex = deviceid + 1;
                //ProductName即是声卡名称
            }
            isInt = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("    欢迎使用西瓜直非官方助手。\n    本软件原作者 q792602257 ，当前分支由 挂神 维护。\n    软件在使用过程出现问题可通过“用户反馈”或以下方式联系挂神：哔哩哔哩@I挂神I、西瓜视频@sy挂神、GitHub@guation。\n   如果您觉得此软件对您有帮助也可以考虑赞助本软件的开发，挂神的QQ群：291283968，挂神的QQ：1853306918。\n    本软件完全开源，严禁倒卖，项目地址：https://github.com/guation/XiguaDanmuHelper \n    您可以在不违反协议的情况下进行二次开发。", "关于本软件");
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            string msg = Interaction.InputBox("输入您的反馈内容，点击确定提交，点击取消离开。我们会将您的反馈内容转发到挂神QQ以便及时处理，如果您无法通过一句话描述问题请使用QQ与挂神取得联系。注意：我们将会收集您的机器码一并提交，机器码仅做识别用户依据不包含您的隐私信息。", "用户反馈", null, -1, -1);
            string data;
            if (msg == "") return;
            try
            {
                string cpuInfo = " ";
                using (ManagementClass cimobject = new ManagementClass("Win32_Processor"))
                {
                    ManagementObjectCollection moc = cimobject.GetInstances();

                    foreach (ManagementObject mo in moc)
                    {
                        cpuInfo = mo.Properties["ProcessorId"].Value.ToString();
                        mo.Dispose();
                    }
                }
                if((bool)GetYuyin?.Invoke()?.Feedback(msg, cpuInfo))
                    data = "反馈内容已提交,您的ID：" + cpuInfo;
                else
                    data = "服务器未连接";
            }
            catch (Exception err)
            {
                data = "用户反馈发生了错误" + err.ToString();
            }
            MessageBox.Show(data, "用户反馈");
        }
        private void Slider0_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isInt) configData.maxCapacity = (int)slider0.Value;
        }

        private void Slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isInt) {
                configData.spd = (int)slider1.Value;
            }
        }

        private void Slider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isInt)
            {
                configData.pit = (int)slider2.Value;
            }
        }

        private void Slider3_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isInt)
            {
                configData.vol = (int)slider3.Value;
            }
        }

        private void Slider4_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isInt) configData.maxSize = (int)slider4.Value;
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

        private void audiolist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInt)
            {
                configData.AudioDeviceName = audiolist.Text;
            }
        }

        private void Clossing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            configData.BlackList = Textbox.Text;
            //User.targetBrand = configData.Brand;
            configData.AudioDeviceName = audiolist.Text;
        }
    }
}
