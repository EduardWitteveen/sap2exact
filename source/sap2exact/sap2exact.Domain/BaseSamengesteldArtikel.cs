using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sap2exact.Domain
{
    public class BaseSamengesteldArtikel: BaseArtikel
    {
        public virtual  List<Stuklijst> Stuklijsten{ get; set; }

        public BaseSamengesteldArtikel()
            : base()
        {
            Stuklijsten= new List<Stuklijst>();
        }
    }
}
