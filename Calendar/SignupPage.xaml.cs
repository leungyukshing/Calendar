using Calendar.database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Calendar
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SignupPage : Page
    {
        public SignupPage()
        {
            this.InitializeComponent();
        }

        // 处理注册的用户信息
        private void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear Error
            UsernameError.Text = "";
            PasswordError.Text = "";
            PasswordRepeatError.Text = "";

            // Get Message
            string username = InputUsername.Text;
            string password = InputPassword.Password;
            string password_repeat = InputPasswordRepeat.Password;
            int root;
            if (comboBox.SelectionBoxItem == null)
            {
                return;
            }
            if (comboBox.SelectionBoxItem.ToString() == "Normal")
            {
                root = 0;
            }
            else
            {
                root = 1;
            }

            // Check format
            if (!checkFormat(username, password, password_repeat))
            {
                return;
            }

            // insert to database
            // Fail
            if (!Db.GetInstance().Register(username, password))
            {
                var m = new MessageDialog("用户名已存在").ShowAsync();
            }

            // jump to SigninPage
            Frame.Navigate(typeof(SigninPage), "");
        }
        
        private bool checkFormat(string username, string password, string password_repeat)
        {
            bool isPass = true;

            if (username == "")
            {
                UsernameError.Text = "用户名不能为空";
                isPass = false;
            }
            else if (username.Length > 20)
            {
                UsernameError.Text = "用户名长度不能超过20个字符";
                isPass = false;
            }

            if (password == "")
            {
                PasswordError.Text = "密码不能为空";
                isPass = false;
            }
            else if (password.Length > 20)
            {
                PasswordError.Text = "密码长度不能超过20个字符";
                isPass = false;
            }

            if (password != password_repeat)
            {
                PasswordRepeatError.Text = "两次输入密码不一样";
                isPass = false;
            }

            return isPass;
            
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SigninPage), "");
        }
    }
}
