﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace access2exact.poco
{
    public class BaseSamengesteldArtikel: BaseArtikel
    {
        public List<Stuklijst> Stuklijsten = new List<Stuklijst>();
    }
}
