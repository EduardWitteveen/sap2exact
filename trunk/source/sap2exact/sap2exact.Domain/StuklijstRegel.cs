using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sap2exact.Domain
{
    public class StuklijstRegel
    {
        public virtual int Volgnummer { get; set; }
        public virtual double ReceptuurRegelAantal { get; set; }
        public virtual double ReceptuurEenheidFactor { get; set; }
        public virtual string ReceptuurEenheid { get; set; }
        public virtual string ReceptuurEenheidConversie { get; set; }
        public virtual string ReceptuurSortBegrip { get; set; }

        public virtual BaseArtikel Artikel { get; set; }

        internal StuklijstRegel Clone()
        {
            return (StuklijstRegel)this.MemberwiseClone();
        }
    }
}