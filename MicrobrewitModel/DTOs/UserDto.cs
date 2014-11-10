﻿using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microbrewit.Model.DTOs
{
    [ElasticType(Name = "user")]
    public class UserDto
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Gravatar { get; set; }
        public IList<BreweryDto> Breweries { get; set; }
        public IList<BeerSimpleDto> Beers { get; set; }
        public string Settings { get; set; }
        public string DataType { get { return "user"; } }
    }
}