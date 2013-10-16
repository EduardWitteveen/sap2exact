using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace access2exact.Domain
{
    public class BaseArtikel
    {
        public virtual string Code { get; set; }
        public virtual string Verpakking { get; set; }
        public virtual string Description { get; set; }
        public virtual Dictionary<int, string> Descriptions { get; set; }
        public virtual string PrijsEenheid { get; set; }
        public virtual double PrijsKost { get; set; }
        public virtual double PrijsVerkoop { get; set; }
        public virtual double PrijsGewichtNetto { get; set; }

        // uit scherm arikelen per leverancier
        public virtual string VerkoopVerpakking { get; set; }
        public virtual double VerkoopAantalNetto { get; set; }
        public virtual double VerkoopAantalBruto { get; set; }
        //public string VerkoopGewichtEenheid;
        public virtual int PrijsBelastingCategorie { get; set; }
        public virtual int HoudbaarheidInDagen { get; set; }
        public virtual DateTime TimeStamp { get; set; }
        public virtual string Intrastat { get; set; }

        public BaseArtikel()
            : base()
        {
            Descriptions = new Dictionary<int, string>();
        }
    }
}