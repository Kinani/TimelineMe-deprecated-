using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimelineMe.Models
{
    public class Media
    {
        [PrimaryKey][AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreationDate { get; set; }
        // True is for Vid :)
        public bool VidOrPic { get; set; }
    }
}
