using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sap2exact.Domain
{
    public class Stuklijst
    {
        public virtual int StuklijstVersion{ get; set; }
        public virtual double StuklijstTotaalAantal{ get; set; }
        public virtual List<StuklijstRegel> StuklijstRegels{ get; set; }
        public virtual string StuklijstNaam { get; set; }
        public virtual DateTime StuklijstDatum { get; set; }

        public Stuklijst()
        {
            StuklijstRegels = new List<StuklijstRegel>();
        }

        public void StuklijstRegelsAdd(StuklijstRegel receptuurregel)
        {
            /*
            for (int i = 0; i < StuklijstRegels.Count; i++)
            {
                if (StuklijstRegels[i].Volgnummer == 0)
                {
                    StuklijstRegels[i].Volgnummer = 1000;
                    Console.Error.WriteLine("SHOULD BE FIXED: replacing stuklijst regel volgnummer met waarde 0");
                }
            }

            // HACK HACK: dubbele artikelcode's
            for (int i = 0; i < StuklijstRegels.Count; i++)
            {
                if (StuklijstRegels[i].Volgnummer == receptuurregel.Volgnummer)
                {
                    // skip if replaced in next loop?
                    if (StuklijstRegels[i].Artikel.MateriaalCode != receptuurregel.Artikel.MateriaalCode)
                    {
                        receptuurregel.Volgnummer += 1;
                        i = 0;
                        Console.Error.WriteLine("SHOULD BE FIXED: replacing stuklijst regel met dezelfde volgnummer!:");
                    }
                }
            }
            for(int i = 0;i < StuklijstRegels.Count; i++) {
                if(StuklijstRegels[i].Artikel.MateriaalCode == receptuurregel.Artikel.MateriaalCode) {
                    StuklijstRegels[i] = receptuurregel;
                    Console.Error.WriteLine("SHOULD BE FIXED: replacing stuklijst regel met dezelfde code!:");
                    return;
                }
            }
            */
            StuklijstRegels.Add(receptuurregel);
        }
    }
}
