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
        public virtual List<HoeveelheidsEenheid> HoeveelheidsEenheden { get; set; }

        public virtual int ExactGewensteBelastingCategorie { get; set; }
        public virtual double ExactGewensteNettoGewicht { get; set; }

        public virtual double KostPrijs { get; set; }
        //public virtual double VerkoopPrijs { get; set; }


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
            HoeveelheidsEenheden = new List<HoeveelheidsEenheid>();
        }

        public double ConversieFactor(string van, string naar)
        {
            // eerst zoeken, wellicht conversie met afval ofsu
            foreach (HoeveelheidsEenheid eenheid in HoeveelheidsEenheden)
            {
                if (eenheid.vanEenheid == van && eenheid.naarEenheid == naar)
                {
                    if (eenheid.factor != 1)
                    {
                        System.Diagnostics.Debug.WriteLine("conversie factor van: " + van + " naar: " + naar + " factor:" + eenheid.factor);
                    }
                    return eenheid.factor;
                }
            }
            // als we niet gaan converteren, dan maar zo
            if (van == naar)
            {
                System.Diagnostics.Debug.WriteLine("conversie factor van: " + van + " naar: " + naar + " hard gezet op : 1.0");
                return 1;
            }
            throw new NotImplementedException("kon eenheid niet converteren voor artikel:" + MateriaalCode + " van: " + van  + " naar: " + naar);
        }
    }
}