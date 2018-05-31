using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendar.Tile
{
    public class PrimaryTile
    {
        public string time { get; set; } = "";
        public string message { get; set; } = "";
        public string branding { get; set; } = "name";
        public string appName { get; set; } = "Calendar";

        public PrimaryTile(string title = "", string detail = "")
        {
            time = title;
            message = detail;
        }
    }
}
