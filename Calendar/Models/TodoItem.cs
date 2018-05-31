using System;
using System.ComponentModel;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Calendar.Models
{
    public class TodoItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string id;
        public Visibility Vis
        {
            get
            {
                if (Completed)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
            set
            {
                if ((Visibility)value == Visibility.Visible)
                    Completed = true;
                else
                    Completed = false;

            }
        }
        public string FormatTime
        {
            private set
            {

            }
            get
            {
                return Services.TimeService.getFormatTime(Date);
            }
        }
        private string title;
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                this.title = value;
                if(PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Title"));
            }
        }
        private ImageSource imageSource { get; set; }
        public string Description { get; set; }
        private bool completed;
        public bool Completed
        {
            get
            {
                return completed;
            }
            set
            {
                this.completed = value;

                if (PropertyChanged != null)
                {
                    Debug.WriteLine("property changed");
                    Debug.WriteLine("Visibility==" + Vis);
                    PropertyChanged(this, new PropertyChangedEventArgs("Vis"));
                    PropertyChanged(this, new PropertyChangedEventArgs("Completed"));
                }
            }
        }
        private DateTimeOffset date;
        public DateTimeOffset Date
        {
            get
            {
                return date;
            }
            set
            {
                date = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Date"));
                    PropertyChanged(this, new PropertyChangedEventArgs("FormatTime"));
                }
            }
        }
        public ImageSource ImageSource_
        {
            get
            {
                return imageSource;
            }
            set
            {
                imageSource = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("ImageSource_"));
                }
                
            }
        }
        public string uriPath { get; set; } = "ms-appx:///Assets/init.png";
        public TodoItem(string title, string description, DateTimeOffset date,string uri,string id_,bool finished = false)
        {
            

            if (uri == null || uri == "")
            {
                //do nothind
                uriPath = "ms-appx:///Assets/init.png";
            }

            else
            {
                uriPath = uri;
                
            }
            ImageSource_ = new BitmapImage(new Uri(uriPath));

            //
            if (id_ == null||id.Length < 10) {
                id = Guid.NewGuid().ToString("N");
            }

            else
            {
                id = id_;
            }
               
            this.Title = title;
            this.Description = description;
            
            Completed = finished;

            this.Date = date;
        }

        public string getId()
        {
            return id;
        }
        public void setId(string id)
        {
            this.id = id;
        }
    }
}
