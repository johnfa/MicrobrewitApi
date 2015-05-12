﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microbrewit.Model
{
    public class Flavour
    {
        public int FlavourId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<HopFlavour> Hops { get; set; }
    }
}
