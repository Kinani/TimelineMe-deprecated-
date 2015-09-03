using SQLite.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimelineMe.Models;

namespace TimelineMe.ViewModels
{
    public class MediasViewModel : MediaViewModel
    {
        private ObservableCollection<MediaViewModel> items;

        public ObservableCollection<MediaViewModel> Items
        {
            get { return items; }
            set
            {
                items = value;
                RaisePropertyChanged("Items");
            }
        }


        public ObservableCollection<MediaViewModel> GetItems()
        {
            items = new ObservableCollection<MediaViewModel>();
            using (var db = new SQLiteConnection(App.SQLITE_PLATFORM, App.DB_PATH))
            {
                var query = db.Table<Media>().OrderBy(c => c.Id);
                foreach (var _item in query)
                {
                    var item = new MediaViewModel()
                    {
                        Id = _item.Id,
                        Name = _item.Name,
                        CreationDate = _item.CreationDate,
                        VidOrPic = _item.VidOrPic
                    };
                    items.Add(item);
                }
            }
            return items;
        }

    }
}
