﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microbrewit.Model
{
    public class User
    {
        //public int Id { get; set; }
        public string Username { get; set; }        
        public string Email { get; set; }
        public string BreweryName { get; set; }
        public string Settings { get; set; }      
    }
}