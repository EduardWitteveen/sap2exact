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
        public TekstArtikel(string tekst)
        {
            this.MateriaalCode = "INSTRUCTIE";
            this.ArtikelOmschrijving = "Instuctie productieorder";
            Tekst = tekst;
        }
    }
}
