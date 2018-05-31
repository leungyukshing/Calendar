using Calendar.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calendar.network;
using System.Diagnostics;
using Windows.Data.Xml.Dom;
using Calendar.Services;
using Windows.UI.Notifications;
using Calendar.Tile;

namespace Calendar.ViewModels
{
    public class View : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private static View instance;
        private Group future;
        private Group finished;
        private TodoItem selectedItem;
        private networkConnection net = networkConnection.getConnection();
        private View()
        {
            load();
        }

        async public void load()
        {
            Debug.WriteLine("loading...");
            Future.listName = "future";
            Finished.listName = "finished";
            if (App.loginUser == null)
                return;
            Group all = new Group( database.Db.GetInstance().GetAll(App.loginUser.username));
            Finished.itemList.Clear();
            Finished.all_item = 0;
            Future.itemList.Clear();
            Future.all_item = 0;
            for(int i = 0; i < all.itemList.Count; i++)
            {
                if (all.itemList.ElementAt(i).Completed)
                {
                    Finished.itemList.Add(all.itemList.ElementAt(i));
                }
                else
                {
                    Future.itemList.Add(all.itemList.ElementAt(i));
                }
            }
            Future.all_item = Future.itemList.Count;
        }

        public static View getInstance()
        {
            if(instance == null)
            {
                instance = new View();
            }
            return instance;
        }
        public ObservableCollection<Group> GroupList
        {
            set { }
            get
            {
                ObservableCollection<Group> view = new ObservableCollection<Group>();
                view.Add(Future);
                view.Add(Finished);
                return view;
            }
        }
        public TodoItem SelectedItem {
            set { selectedItem = value; }
            get { return selectedItem; }
        }

        public static View SingleView
        {
            set
            {

            }
            get
            {
                if(instance == null)
                {
                    instance = new View();

                }
                return instance;
            }
        }
        public Group Future
        {
            set { future = value; }
            get
            {
                if(future == null)
                {
                    future = new Group(new System.Collections.ObjectModel.ObservableCollection<TodoItem>());
                }
                return future;
            }
        }
        public Group Finished
        {
            set { finished = value; }
            get
            {
                if(finished == null)
                {
                    finished = new Group(new System.Collections.ObjectModel.ObservableCollection<TodoItem>());
                }
                return finished;
            }
        }

        public void OnItemStatusChanged(TodoItem todo)
        {
            if (Future.itemList.Contains(todo))
            {
                Future.itemList.Remove(todo);
                Finished.itemList.Add(todo);
            }
            else if (Finished.itemList.Contains(todo))
            {
                Finished.itemList.Remove(todo);
                Future.itemList.Add(todo);
            }
            else
            {

            }
        }
        public void Remove(TodoItem todo)
        {
            Finished.itemList.Remove(todo);

            if (Future.itemList.Remove(todo))
                Future.all_item--;

            Circulation();
        }

        public void Add(TodoItem todo)
        {
            if(todo.Completed == true)
            {
                Finished.itemList.Add(todo);
            }
            else
            {
                Future.itemList.Add(todo);
                Future.all_item++;
            }
            Circulation();
        }
        public void Update(string title,string detail,DateTimeOffset date,string imgUri = null, bool finished = false)
        {
            if (SelectedItem == null)
                return;
            if (imgUri != null)
                SelectedItem.uriPath = imgUri;
            SelectedItem.Date = date;
            SelectedItem.Title = title;
            SelectedItem.Description = detail;
            SelectedItem.Completed = finished;
            if (finished)
            {
                if(Future.itemList.Remove(SelectedItem))
                    Future.all_item--;
                if (!Finished.itemList.Contains(SelectedItem))
                    Finished.itemList.Add(SelectedItem);
            }
            else
            {
                if (!Future.itemList.Contains(SelectedItem))
                {
                    Future.itemList.Add(SelectedItem);
                    Future.all_item++;
                }
                Finished.itemList.Remove(SelectedItem);
            }
            Circulation();
        }

        private void UpdatePrimaryTile(string title, string detail)
        {
            var xmlDoc = TileService.CreateTiles(new PrimaryTile(title, detail));
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            TileNotification notification = new TileNotification(xmlDoc);
            updater.Update(notification);
        }

        public void Circulation()
        {
            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            for (int i = 0; i < future.itemList.Count; i++)
                UpdatePrimaryTile(future.itemList[i].Title, future.itemList[i].Description);
        }
    }
}
