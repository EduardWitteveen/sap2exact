using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace access2exact.poco
{
    public class BaseArtikel
    {
        public string Code;
        public string Verpakking;
        //public string Statistical = "35019090";
        public string Description;
        public Dictionary<int, string> Descriptions = new Dictionary<int, string>();
        
        //public string VerpakkingType;
        //public string VerpakkingEenheid;
        //public double VerpakkingBruto;
        //public double VerpakkingNetto;

        public string PrijsEenheid;
        public double PrijsKost;
        public double PrijsVerkoop;
        public double PrijsGewichtNetto;

        // uit scherm arikelen per leverancier
        public string VerkoopVerpakking;
        public double VerkoopGewichtNetto;
        public double VerkoopGewichtBruto;
        public string VerkoopGewichtEenheid;
        public string TimeStamp;
    }
}
