using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace access2exact.Domain
{
    public class StuklijstRegel
    {
        public virtual int Volgnummer { get; set; }
        public virtual double ReceptuurRegelAantal { get; set; }
        public virtual BaseArtikel Artikel { get; set; }
    }
}