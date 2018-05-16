﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendar.Models
{
    public class UserItem
    {
        public string username { get; set; }

        public string password { get; set; }

        public long root { get; set; }

        public UserItem(string username, string password, long root)
        {
            this.username = username;
            this.password = password;
            this.root = root;
        }
    }
}
