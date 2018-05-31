using Calendar.database;
using Calendar.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public sealed partial class SigninPage : Page
    {
        public SigninPage()
        {
            this.InitializeComponent();
        }

        // 处理登录事件
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear the Error
            UsernameError.Text = "";
            PasswordError.Text = "";

            string username = InputUsername.Text;
            string password = InputPassword.Password;

            // check format
            if (!checkFormat(username, password)) {
                return;
            }

            // check whether the user exist
            var flag =  Db.GetInstance().Login(username,password);
            // user is not found
            if (flag == false)
            {
              //   new MessageDialog("用户不存在").ShowAsync();
                UsernameError.Text = "用户不存在或密码错误";
                return;
            }
            // check whether the password is correct

            App.loginUser = new UserItem(username, password, 0);
            // set the state to login
            App.isLogin = true;

            // jump to mainpage
            Frame.Navigate(typeof(MainPage));
        }

        // 检查用户信息格式
        private bool checkFormat(string username, string password)
        {
            bool isPass = true;
            if (username == "")
            {
                UsernameError.Text = "请输入用户名";
                isPass = false;
            }
            if (password == "")
            {
                PasswordError.Text = "请输入密码";
                isPass = false;
            }

            return isPass;
        }

        // 跳转到注册页面
        private void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SignupPage));
        }
    }
}
