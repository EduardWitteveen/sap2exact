using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sap2exact.Domain
{ 
    public class TekstArtikel: BaseArtikel
    {
        public string Tekst;
        public TekstArtikel(string tekstregel1, string tekstregel2)
        {
            this.MateriaalCode = "INSTRUCTIE";
            this.ArtikelOmschrijving = "Instuctie productieorder";
            Tekst = tekstregel1 + " " + tekstregel2;
        }
    }
}
