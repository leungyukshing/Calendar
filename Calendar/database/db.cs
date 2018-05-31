using Calendar.Models;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//要使用该类则使用Db.GetInstance()获得实例进行操作
//获取name的所有日程
//ObservableCollection<Models.TodoItem> GetAll(string name)
//插入日程
//Boolean Insert(string name, string id, string title, string content, DateTimeOffset date, string imageString)
//删除日程
//Boolean Remove(string name, string id)
//更新日程
//Boolean Update(string name, string id, string title, string content, DateTimeOffset date, string imageString)
//改变日程完成程度
//Boolean Complete(string name, string id, bool finish)
//搜索日程，返回搜索结果字符串
//string Search(string name, string str)
//尝试注册
//Boolean Register(string name,string pwd,long root)
//获取登陆用信息，不存在返回空
//UserItem Login(string name)

namespace Calendar.database
{
    class Db
    {
        static Db dbInstance;
        private SQLiteConnection conn;

        Db()
        {
            LoadDatabase();
        }

        static public Db GetInstance()
        {
            if (dbInstance==null)
            {
                dbInstance = new Db();
            }
            return dbInstance;
        }

        private void LoadDatabase()
        {     // Get a reference to the SQLite database    
            conn = new SQLiteConnection("Todo.db");
            string sql = @"CREATE TABLE IF NOT EXISTS user (
                    name   VARCHAR( 20  ) PRIMARY KEY NOT NULL,
                    pwd    VARCHAR(20),
                    root    INTEGER
                    );";
            using (var statement = conn.Prepare(sql))
            { statement.Step(); }
            sql = @"CREATE TABLE IF NOT EXISTS item (
                    id      VARCHAR( 140 ) NOT NULL,
                    name     VARCHAR( 20  ) NOT NULL,
                    title   VARCHAR( 140 ),
                    content VARCHAR( 140 ),
                    date    VARCHAR( 140 ),
                    image   VARCHAR( 140 ),
                    finish  INTEGER,
                    PRIMARY KEY(name, id),
                    FOREIGN KEY(name) REFERENCES user(name)
                    ON DELETE CASCADE
                    );";
            using (var statement = conn.Prepare(sql))
            { statement.Step(); }
        }

        public ObservableCollection<Models.TodoItem> GetAll(string name)
        {
            var db = this.conn;
            var view = new ObservableCollection<Models.TodoItem>();
            try
            {
                using (var statement = db.Prepare("SELECT * FROM item where name = ?"))
                {
                    statement.Bind(1, name);
                    Debug.WriteLine("before while, name = " + name);
                    while (statement.Step() == SQLiteResult.ROW)
                    {
                        Debug.WriteLine("while loop...");
                        var temp = ((string)statement[4]).Split('/');
                        int year = int.Parse(temp[2]);
                        int month = int.Parse(temp[0]);
                        int day = int.Parse(temp[1]);
                        int hour = int.Parse(temp[3]);
                        int minute = int.Parse(temp[4]);
                        var date1 = new DateTimeOffset(year, month, day,hour, minute,0,TimeSpan.Zero);
                        var date = date1;
                        string tid = (string)statement[0];
                        string ttitle = (string)statement[2];
                        string tdes = (string)statement[3];
                        string timage = (string)statement[5];
                        var t = ((Int64)statement[6]) == Int64.Parse("1") ? true : false;
                        Debug.WriteLine("t==" + t);
                        var  titem = new TodoItem(ttitle, tdes, date, timage, tid, t);

                        view.Add(titem);
                       // view.Add(null);
                        Debug.WriteLine("hello");
                    }
                    Debug.WriteLine("after while..");
                }
                return view;
            }
            catch (Exception ex)
            {
                // TODO: Handle error
                return view;
            }
        }

        public Boolean Insert(string name, string id, string title, string content, DateTimeOffset date, string imageString)
        {
            // SqlConnection was opened in App.xaml.cs and exposed through property conn
            var db = this.conn;
            try{
                using (
                    var sql = db.Prepare("INSERT INTO item (name, id, title, content, date, image, finish) VALUES (?, ?, ?, ?, ?, ?, ?)")){
                    sql.Bind(1, name);//uid
                    sql.Bind(2, id);
                    sql.Bind(3, title);
                    sql.Bind(4, content);
                    sql.Bind(5, date.Month.ToString() + '/' + date.Day.ToString() + '/' + date.Year.ToString() + '/' + date.Hour.ToString() + '/' + date.Minute.ToString());
                    sql.Bind(6, imageString);
                    sql.Bind(7, -1);
                    sql.Step();
                }
                Background.BackgroundTask.getInstance().AddClock(id, title, content, imageString, date);
                return true;
            }catch (Exception ex){
                // TODO: Handle error
                return false;
            }
        }

        public Boolean Remove(string name, string id)
        {
            // SqlConnection was opened in App.xaml.cs and exposed through property conn
            var db = this.conn;
            try
            {
                using (var statement = db.Prepare("DELETE FROM item WHERE id = ? AND name = ?;"))
                {
                    statement.Bind(1, id);
                    statement.Bind(2, name);
                    statement.Step();
                }
                Background.BackgroundTask.getInstance().DeleteClock(id);
                return true;
            }
            catch (Exception ex)
            {
                // TODO: Handle error
                return false;
            }
        }

        public Boolean Update(string name, string id, string title, string content, DateTimeOffset date, string imageString)
        {
            var db = this.conn;
            try
            {
                using (var statement = db.Prepare("UPDATE item SET title = ?, content = ?, date = ?, image = ? WHERE id = ? AND name = ?"))
                {
                    statement.Bind(1, title);
                    statement.Bind(2, content);
                    statement.Bind(3, date.Month.ToString() + '/' + date.Day.ToString() + '/' + date.Year.ToString() + '/' + date.Hour.ToString() + '/' + date.Minute.ToString());
                    statement.Bind(4, imageString);
                    statement.Bind(5, id);
                    statement.Bind(6, name);
                    statement.Step();
                    return true;
                }
            }
            catch (Exception ex)
            {
                // TODO: Handle error
                return false;
            }
        }

        public Boolean Complete(string name, string id, bool finish)
        {
            var db = this.conn;
            try
            {
                using (var statement = db.Prepare("UPDATE item SET finish = ? WHERE id = ? AND name = ?"))
                {
                    statement.Bind(1, finish?1:0);
                    statement.Bind(2, id);
                    statement.Bind(3, name);
                    statement.Step();
                }
                return true;
            }
            catch (Exception ex)
            {
                // TODO: Handle error
                return false;
            }
        }

        public string Search(string name, string str)
        {
            var db = this.conn;
            StringBuilder msg = new StringBuilder();

            try
            {
                int count = 0;
                using (var statement = db.Prepare("SELECT * FROM item WHERE name = ? AND (title LIKE ? OR content LIKE ? OR date LIKE ?)"))
                {
                    statement.Bind(1, name);
                    statement.Bind(2, "%"+str+"%");
                    statement.Bind(3, "%" + str + "%");
                    statement.Bind(4, "%" + str + "%");
                    while (statement.Step() == SQLiteResult.ROW)
                    {
                        count++;
                        msg.Append("第" + count + "项：\ntitle: " + (string)statement[2] + "\ncontent: " + (string)statement[3] + 
                            "\ndate: " + (string)statement[4] + "\n");
                    }
                    msg.Insert(0,"共" + count + "项：\n");
                }
            }
            catch (Exception ex)
            {
                // TODO: Handle error
                msg.Append("无");
            }
            return msg.ToString();
        }

        public Boolean Register(string name,string pwd,long root)
        {
            var db = this.conn;
            try
            {
                using (
                var sql = db.Prepare("INSERT INTO user (name, pwd,root) VALUES (?, ?, ?)"))
                {
                    sql.Bind(1, name);
                    sql.Bind(2, pwd);
                    sql.Bind(3, root);
                    sql.Step();
                }
                return true;
            }
            catch (Exception ex)
            {
                // TODO: Handle error
                return false;
            }
        }

        public UserItem Login(string name)
        {
            var db = this.conn;

            try
            {
                using (var statement = db.Prepare("SELECT * FROM user WHERE name = ?"))
                {
                    statement.Bind(1, name);
                    if (statement.Step() == SQLiteResult.ROW)
                    {
                        return new UserItem((string)statement[0],(string)statement[1],(long)statement[2]);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: Handle error
                return null;
            }
        }


    }
}
