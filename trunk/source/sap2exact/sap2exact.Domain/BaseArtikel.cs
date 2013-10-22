using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sap2exact.Domain;

namespace sap2exact.Domain
{
    public class BaseArtikel
    {
        public virtual string MateriaalCode { get; set; }
        public virtual string ArtikelOmschrijving { get; set; }
        public virtual Dictionary<int, string> ArtikelOmschrijvingen { get; set; }
        public virtual string BasishoeveelheidEenheid { get; set; }
        public virtual Dictionary<string, HoeveelheidsEenheid> HoeveelheidsEenheden { get; set; }

        public virtual int ExactGewensteBelastingCategorie { get; set; }
        public virtual double ExactGewensteNettoGewicht { get; set; }

        public virtual double KostPrijs { get; set; }
        public virtual double VerkoopPrijs { get; set; }


        // uit scherm arikelen per leverancier
        public virtual double NettoGewicht { get; set; }
        public virtual double BruttoGewicht { get; set; }
        public virtual string Gewichtseenheid { get; set; }

        public virtual int HoudbaarheidInDagen { get; set; }        
        public virtual DateTime TimeStamp { get; set; }
        public virtual string Intrastat { get; set; }

        public BaseArtikel()
            : base()
        {
            ArtikelOmschrijvingen = new Dictionary<int, string>();
            HoeveelheidsEenheden = new Dictionary<string, HoeveelheidsEenheid>();
        }
    }
}