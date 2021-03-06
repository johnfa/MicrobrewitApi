﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace Microbrewit.Model
{
    public class User
    {
        //public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Settings { get; set; }
        public string Gravatar { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string HeaderImage { get; set; }
        public string Avatar { get; set; }
        public IList<string> Roles { get; set; }

        public ICollection<UserSocial> Socials { get; set; }
        public ICollection<BreweryMember> Breweries { get; set; }
        public ICollection<UserBeer> Beers { get; set; }
    }
}
