using Calendar.Background;
﻿using Calendar.database;
using Calendar.Models;
using Calendar.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
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
using Calendar.network;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Text;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;


// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Calendar
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    /// 

    public sealed partial class MainPage : Page
    {
        public View SingleView;
        bool Update_flag = false;
        private Db database = Db.GetInstance();
        public string weather;
        public networkConnection net = networkConnection.getConnection();

        public MainPage()
        {
            initList();
            this.InitializeComponent();
            initializePage();
            SingleView.Circulation();
            userName.Text = App.loginUser.username;

            var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = Colors.LightYellow;
            titleBar.ForegroundColor = Colors.White;
            titleBar.ButtonHoverBackgroundColor = Colors.LightBlue;
            titleBar.ButtonBackgroundColor = Colors.LightYellow;
        }

        public async Task<string> getConnectToGetWeatherAsync(string queryString)
        {
            //Create an HTTP client 
            Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient();
            //Add a user-agent header to the GET request. 
            var headers = httpClient.DefaultRequestHeaders;
            //The safe way to add a header value is to use the TryParseAdd method and verify the return value is true,
            //especially if the header value is coming from user input.
            string header = "ie";
            if (!headers.UserAgent.TryParseAdd(header))
            {
                throw new Exception("Invalid header value: " + header);
            }
            header = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
            if (!headers.UserAgent.TryParseAdd(header))
            {
                throw new Exception("Invalid header value: " + header);
            }
            //Uri requestUri = new Uri("http://v.juhe.cn/weather/index?format=2&cityname="+queryString+"&key=c8f3ebd5bd91ab7aa0a9be9cc717e5dd");
            Uri requestUri = new Uri("http://v.juhe.cn/weather/index?cityname=" + queryString + "&dtype=xml&format=2&key=c8f3ebd5bd91ab7aa0a9be9cc717e5dd");
            //Send the GET request asynchronously and retrieve the response as a string.
            Windows.Web.Http.HttpResponseMessage httpResponse = new Windows.Web.Http.HttpResponseMessage();
            string httpResponseBody = "";
            try
            {
                //Send the GET request    
                httpResponse = await httpClient.GetAsync(requestUri);
                httpResponse.EnsureSuccessStatusCode();
                httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
                weather_information.Text =  getStringFromWeatherXMLString(httpResponseBody);
                return getStringFromWeatherXMLString(httpResponseBody);
            }
            catch (Exception ex)
            {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
                return "Error,please retry";
            }
        }

        private string getStringFromWeatherXMLString(string xmlStr)
        {//200
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlStr);
            IXmlNode root = doc.SelectSingleNode("root");
            XmlNodeList nodeList = root.ChildNodes;
            if (int.Parse(nodeList[0].InnerText) != 200)
            {
                return "city is not exist";
            }
            nodeList = nodeList[2].ChildNodes;
            nodeList = nodeList[1].ChildNodes;
            //遍历所有子节点
            string temperature = "";
            foreach (IXmlNode xn in nodeList)
            {
                XmlElement xe = (XmlElement)xn;
                if (xe.TagName == "temperature")
                {
                    temperature = xe.InnerText;
                }
                else if (xe.TagName == "weather")
                {
                    weather = xe.InnerText;
                    if (weather.Contains("雨"))
                    {
                        Uri uri = new Uri("ms-appx:///Assets/rain.png");
                        BitmapImage bt = new BitmapImage(uri);
                        image.Source = bt;
                    }
                    else if (weather.Contains("云"))
                    {
                        Uri uri = new Uri("ms-appx:///Assets/cloud.png");
                        BitmapImage bt = new BitmapImage(uri);
                        image.Source = bt;
                    }
                    else
                    {
                        Uri uri = new Uri("ms-appx:///Assets/sun.png");
                        BitmapImage bt = new BitmapImage(uri);
                        image.Source = bt;
                    }
                }
            }
            return temperature;
        }

        public async void initializePage()
        {
            title.Text = "";
            detail.Text = "";
            date.Date = DateTime.Now;
            time.Time = new TimeSpan(DateTimeOffset.Now.Hour,DateTimeOffset.Now.Minute,DateTimeOffset.Now.Second);
            Update_flag = false;
            complete.Content = "future";
            //add.Content = "create";
            await getConnectToGetWeatherAsync("广州");
        }

        public void initList()
        {
            SingleView = View.SingleView;
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var itemlist = (((sender as AppBarButton).DataContext) as Group);

            itemlist.Vis = itemlist.Vis == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ListViewItem_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //cannot find place function worked
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // not neccessary
        }

        private void Delete_Event(object sender, RoutedEventArgs e)
        {
            //delete
            if (SingleView.SelectedItem == null)
            {
                new MessageDialog("no item is selected!\ndelete failed!").ShowAsync();
                return;
            }
            View.SingleView.Remove(SingleView.SelectedItem);
            Update_flag = false;
            initializePage();


            database.Remove(App.loginUser.username, View.SingleView.SelectedItem.getId());


        }

        private async void OnUserItemAdding()
        {
            string ttl = title.Text;
            string des = detail.Text;
            DateTimeOffset date_ = date.Date;
            date_ = new DateTimeOffset(date_.Year, date_.Month, date_.Day, time.Time.Hours, time.Time.Minutes,time.Time.Seconds,new TimeSpan(8,0,0));

            bool finished = false;
            if ((string)complete.Content == "future")
                finished = false;
            else if ((string)complete.Content == "finished")
                finished = true;
            string error_text = "";
            string error_title = "";
            string error_detail = "";
            string error_date = "";
            int flag = 0;
            if (title.Text == "")
            {
                error_title = "未填写Title\n";
                flag = 1;
            }
            else if (detail.Text == "")
            {
                error_detail = "未填写Detail\n";
                flag = 1;
            }
            else if (!finished)
            {
                if (date.Date.Year < DateTime.Now.Year)
                {
                    error_date = "日期过期\n";
                    flag = 1;
                }
                else if (date.Date.Year > DateTime.Now.Year)
                {
                    flag = 0;
                }
                else if (date.Date.Month < DateTime.Now.Month)
                {
                    error_date = "日期过期\n";
                    flag = 1;
                }
                else if(date.Date.Month > DateTime.Now.Month)
                {
                    flag = 0;
                }
                else if (date.Date.Day < DateTime.Now.Day)
                {
                    error_date = "日期过期\n";
                    flag = 1;
                }
                else if(date.Date.Day >= DateTime.Now.Day)
                {
                    flag = 0;
                }
            }
            if (flag == 0)
            {
                TodoItem temp = new TodoItem(ttl, des, date_, null, null, finished);
                View.SingleView.Add(temp);
                initializePage();
                database.Insert(App.loginUser.username, temp.getId(),temp.Title, temp.Description, temp.Date, temp.uriPath);
            }
            else
            {
                error_text = error_title + error_detail + error_date;
                MessageDialog err = new MessageDialog(error_text, "提示");
                err.Commands.Add(new UICommand("返回", cmd => { }, commandId: 0));
                err.DefaultCommandIndex = 0;
                await err.ShowAsync();
            }
            }
        private async void OnUserItemUpdate()

        {
            string ttl = title.Text;
            string des = detail.Text;
            DateTimeOffset date_ = date.Date;
            date_ = new DateTimeOffset(date_.Year, date_.Month, date_.Day, time.Time.Hours, time.Time.Minutes, time.Time.Seconds, new TimeSpan(8,0,0));

            string imgPath = View.SingleView.SelectedItem.uriPath;//modified
            bool finished = false;
            string error_text = "";
            string error_title = "";
            string error_detail = "";
            string error_date = "";
            int flag = 0;
            if ((string)complete.Content == "future")
                finished = false;
            else if ((string)complete.Content == "finished")
                finished = true;

            if (title.Text == "")
            {
                error_title = "未填写Title\n";
                flag = 1;
            }
            if (detail.Text == "")
            {
                error_detail = "未填写Detail\n";
                flag = 1;
            }
            if (!finished)
            {
                if (date.Date.Year < DateTime.Now.Year)
                {
                    error_date = "日期过期\n";
                    flag = 1;
                }
                else if(date.Date.Year > DateTimeOffset.Now.Year)
                {
                    flag = 0;
                }
                else if (date.Date.Month < DateTime.Now.Month)
                {
                    error_date = "日期过期\n";
                    flag = 1;
                }
                else if(date.Date.Month > DateTimeOffset.Now.Month)
                {
                    flag = 0;
                }
                else if (date.Date.Day < DateTime.Now.Day)
                {
                    error_date = "日期过期\n";
                    flag = 1;
                }
                else if (date.Date.Day > DateTimeOffset.Now.Day)
                {
                    flag = 0;

                }
                
            }
            if (flag == 0)
            {
                View.SingleView.Update(ttl, des, date_, imgPath, finished);
                initializePage();
                add.Content = "Create";
                database.Update(App.loginUser.username, View.SingleView.SelectedItem.getId(),
                                ttl, des, date_, imgPath);
            }
            else
            {
                error_text = error_title + error_detail + error_date;
                MessageDialog err = new MessageDialog(error_text, "提示");
                err.Commands.Add(new UICommand("返回", cmd => { }, commandId: 0));
                err.DefaultCommandIndex = 0;
                await err.ShowAsync();
            }
            


        }
        private void Share_Event(object sender, RoutedEventArgs e)
        {
            //share
            DataTransferManager.ShowShareUI();
        }

        private void Show_Detail(object sender, ItemClickEventArgs e)
        {
            var one = e.ClickedItem as TodoItem;
            title.Text = one.Title;
            date.Date = one.Date;
            time.Time = new TimeSpan(date.Date.Hour, date.Date.Minute, date.Date.Second);
            detail.Text = one.Description;
            Update_flag = true;
            add.Content = "Update";
            if (one.Completed)
                complete.Content = "finished";
            else
                complete.Content = "future";
            //
            SingleView.SelectedItem = e.ClickedItem as TodoItem;

        }

        private void Search_Event(object sender, RoutedEventArgs e)
        {
            //search

            if (App.isLogin == false)
            {
                new MessageDialog("not login!").ShowAsync();
                return;
            }
            string name = App.loginUser.username;
            string str = searchBox.Text;
            string info = database.Search(name, str);

            info += "view.count" + View.SingleView.Finished.itemList.Count;
            new MessageDialog(info).ShowAsync();


        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            (Window.Current.Content as Frame).Navigate(typeof(SigninPage));
        }

        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            (Window.Current.Content as Frame).Navigate(typeof(SignupPage));
            App.isLogin = false;
            App.loginUser = null;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            View.getInstance().load();

            name.Text = App.loginUser.username;

            DataTransferManager.GetForCurrentView().DataRequested += OnShareDataRequested;
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            DataTransferManager.GetForCurrentView().DataRequested -= OnShareDataRequested;
        }
        private void OnShareDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var request = args.Request;
            request.Data.Properties.Title = "Calender";
            request.Data.SetText(SingleView.SelectedItem == null ? "nothind selected" : SingleView.SelectedItem.Title);
        }

        async private void Add_Or_Delete(object sender, RoutedEventArgs e)
        {
            var itemlist = (((sender as AppBarButton).DataContext) as Group);
            bool date_flag = false;
            if (date.Date.Year == DateTime.Now.Year && date.Date.Month == DateTime.Now.Month && date.Date.Day == DateTime.Now.Day)
                date_flag = true;
            if ((string)itemlist.listName == "future")
            {
                if (Update_flag || title.Text != "" || detail.Text != "" || !date_flag || (string)complete.Content != "future")
                {
                    MessageDialog warning = new MessageDialog("确认离开当前界面？", "提示");

                    warning.Commands.Add(new UICommand("确认", cmd =>
                    {
                        initializePage();
                    }, commandId: 0));
                    warning.Commands.Add(new UICommand("返回", cmd => { return; }, commandId: 1));
                    warning.DefaultCommandIndex = 0;
                    warning.CancelCommandIndex = 1;
                    await warning.ShowAsync();
                }
            }
            else if ((string)itemlist.listName == "finished")
            {
                MessageDialog warning = new MessageDialog("确认清除？", "提示");

                warning.Commands.Add(new UICommand("确认", cmd =>
                {
                    itemlist.itemList.Clear();
                }, commandId: 0));
                warning.Commands.Add(new UICommand("返回", cmd => { return; }, commandId: 1));
                warning.DefaultCommandIndex = 0;
                warning.CancelCommandIndex = 1;
                await warning.ShowAsync();
            }
        }

        private void Add_Or_Update(object sender, RoutedEventArgs e)
        {
            var appbarbutton = sender as AppBarButton;
            if ((string)appbarbutton.Content == "Create")
                OnUserItemAdding();
            else if ((string)appbarbutton.Content == "Update")
                OnUserItemUpdate();
        }

        private void Complete_Click(object sender, RoutedEventArgs e)
        {
            var appbarbutton = sender as AppBarButton;
            if ((string)appbarbutton.Content == "future")
                appbarbutton.Content = "finished";
            else if ((string)appbarbutton.Content == "finished")
                appbarbutton.Content = "future";
        }

        private async void Import_Txt(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add(".txt");
            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                using (Stream stream = await file.OpenStreamForReadAsync())
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    var encoding = Encoding.GetEncoding(0);
                    var dialog = new MessageDialog("txt文件的编码是？");
                    dialog.Commands.Add(new UICommand("ANSI(默认)", cmd =>
                    {
                        encoding = Encoding.GetEncoding(0);
                    }, commandId: 0));
                    dialog.Commands.Add(new UICommand("UTF-8", cmd =>
                    {
                        encoding = Encoding.UTF8;
                    }, commandId: 1));
                    //设置默认按钮，不设置的话默认的确认按钮是第一个按钮
                    dialog.DefaultCommandIndex = 0;
                    dialog.CancelCommandIndex = 1;
                    //获取返回值
                    await dialog.ShowAsync();
                    using (StreamReader reader = new StreamReader(stream, encoding, false))
                    {
                        detail.Text = reader.ReadToEnd();
                    }
                }
            }
        }
    }
}
