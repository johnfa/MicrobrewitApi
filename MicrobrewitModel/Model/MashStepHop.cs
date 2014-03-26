﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microbrewit.Model
{
    public class MashStepHop
    {
        public int Id { get; set; }
        public int HopId { get; set; }
        public int MashStepId { get; set; }
        public int AAValue { get; set; }
        public int AAAmount { get; set; }
        public int HopFormId { get; set; }

        public HopForm HopForm { get; set; }
        public MashStep MashStep { get; set; }
        public Hop Hop { get; set; }
    }
}