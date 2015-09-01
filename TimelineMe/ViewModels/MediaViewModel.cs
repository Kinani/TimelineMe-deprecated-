using SQLite.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimelineMe.Models;

namespace TimelineMe.ViewModels
{
    public class MediaViewModel : MediaViewModelBase
    {
        #region Properties
        private int id;

        public int Id
        {
            get { return id; }
            set
            {
                if (id == value)
                    return;

                id = value;
                RaisePropertyChanged("Id");
            }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set
            {
                if (name == value)
                    return;
                name = value;
                RaisePropertyChanged("Name");
            }
        }

        private DateTime creationDate;

        public DateTime CreationDate
        {
            get { return creationDate; }
            set
            {
                if (creationDate == value)
                    return;
                creationDate = value;
                RaisePropertyChanged("CreationDate");
            }
        }

        private bool vidOrPic;

        public bool VidOrPic
        {
            get { return vidOrPic; }
            set
            {
                if (vidOrPic == value)
                    return;
                vidOrPic = value;
                RaisePropertyChanged("VidOrPic");
            }
        }
        

        #endregion

        public MediaViewModel GetItem(int itemId)
        {
            var item = new MediaViewModel();
            using (var db = new SQLiteConnection(App.SQLITE_PLATFORM, App.DB_PATH))
            {
                var _item = (db.Table<Media>().Where(
                    c => c.Id == itemId)).Single();
                item.Id = _item.Id;
                item.Name = _item.Name;
                item.CreationDate = _item.CreationDate;
                item.VidOrPic = _item.VidOrPic;
            }
            return item;
        }


        public string SaveItem(MediaViewModel item)
        {
            string result = string.Empty;
            using (var db = new SQLiteConnection(App.SQLITE_PLATFORM, App.DB_PATH))
            {
                try
                {
                    var existingItem = (db.Table<Media>().Where(
                        c => c.Id == item.Id)).SingleOrDefault();

                    if (existingItem != null)
                    {
                        existingItem.Id = item.Id;
                        existingItem.Name = item.Name;
                        existingItem.VidOrPic = item.VidOrPic;

                        int success = db.Update(existingItem);
                    }
                    else
                    {
                        int success = db.Insert(new Media()
                        {
                            Id = item.Id,
                            Name = item.Name,
                            CreationDate = item.CreationDate,
                            VidOrPic = item.VidOrPic
                        });
                    }
                    result = "Success";
                }
                catch
                {
                    result = "This item was not saved.";
                }
            }
            return result;
        }

        public string DeleteItem(int itemId)
        {

            string result = string.Empty;
            using (var dbConn = new SQLiteConnection(App.SQLITE_PLATFORM, App.DB_PATH))
            {
                var existingItem = dbConn.Query<Media>("select * from Media where Id =" + itemId).FirstOrDefault();
                if (existingItem != null)
                {
                    dbConn.RunInTransaction(() =>
                    {
                        dbConn.Delete(existingItem);


                        if (dbConn.Delete(existingItem) > 0)
                        {
                            result = "Success";
                        }
                        else
                        {
                            result = "This item was not removed";
                        }

                    });
                }

                return result;
            }
        }



    }
}
