using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;

namespace Calendar.Background
{
    //（已经附加到db的增加中）日常单例，用户退出登陆状态应该调用deleteAllCurrent()，删除所有的闹钟记录，只是关闭应用的话，不用。
    //（已经附加到db的增加中）新增一个需要有闹钟的todoitem时，要用AddClock增加闹钟，id将使用todoitem的id。因为闹钟在退出的登录的时候会删掉，所以没问题。
    //还有就是新用户登陆，加载事件时，应该要对每个有闹钟需求(未完成)的item调用AddClock来增加闹钟
    //（已经附加到db的删除中）用户删除带闹钟的日程或者完成了某个日程应该先调用一次DeleteClock
    class BackgroundTask
    {

        static BackgroundTask Ins=null;

        private BackgroundTask()
        {

        }

        static public BackgroundTask getInstance()
        {
            if(Ins == null)
            {
                Ins = new BackgroundTask();
            }

            return Ins;
        }

        public Boolean AddClock(String id, String title, String content, String ImagePath, DateTimeOffset date,String name)//param:
        {
            if (id == null|| id == "" || DateTimeOffset.Now.CompareTo(date) <= 0 )
            {
                return false;
            }

            try
            {
                ToastTemplateType toastTemplate = ToastTemplateType.ToastImageAndText02;
                XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);

                XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
                toastTextElements[0].AppendChild(toastXml.CreateTextNode(title));
                toastTextElements[1].AppendChild(toastXml.CreateTextNode(content));

                XmlNodeList toastImageAttributes = toastXml.GetElementsByTagName("image");
                ((XmlElement)toastImageAttributes[0]).SetAttribute("src", "ms-appx:///assets/sun.scale-400.png");//ImagePath
                ((XmlElement)toastImageAttributes[0]).SetAttribute("alt", "testImage");

                IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
                //((XmlElement)toastNode).SetAttribute("duration", "long");

                //change music
                //IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
                //XmlElement audio = toastXml.CreateElement("audio");

                ((XmlElement)toastNode).SetAttribute("launch", "{\"type\":\"toast\",\"id\":\""+ id + "\"},\"name\":\"" + name + "\"}");

                ToastNotification toast = new ToastNotification(toastXml);

                ScheduledToastNotification scheduledToast = new ScheduledToastNotification(toastXml, date);
                scheduledToast.Id = id;

                ToastNotificationManager.CreateToastNotifier().AddToSchedule(scheduledToast);
                
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public Boolean DeleteClock(string toast_id)
        {
            try
            {
                var notifier = ToastNotificationManager.CreateToastNotifier();
                var scheduled = notifier.GetScheduledToastNotifications();

                for (int i = 0, len = scheduled.Count; i < len; i++)
                {
                    if (scheduled[i].Id == toast_id)
                    {
                        notifier.RemoveFromSchedule(scheduled[i]);
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public void  DeleteAllCurrent()
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();
            var scheduled = notifier.GetScheduledToastNotifications();
            for (int i = 0, len = scheduled.Count; i < len; i++)
            {
                try
                {
                    notifier.RemoveFromSchedule(scheduled[i]);
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }
        

    }
}
