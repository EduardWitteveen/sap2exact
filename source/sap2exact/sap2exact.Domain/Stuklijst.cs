using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace access2exact.Domain
{
    public class Stuklijst
    {
        public int StuklijstVersion;
        public double StuklijstTotaalAantal;
        public List<StuklijstRegel> StuklijstRegels = new List<StuklijstRegel>();
        public string StuklijstNaam;
        public DateTime StuklijstDatum;

        public void StuklijstRegelsAdd(StuklijstRegel receptuurregel)
        {
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
                    if (StuklijstRegels[i].Artikel.Code != receptuurregel.Artikel.Code)
                    {
                        receptuurregel.Volgnummer += 1;
                        i = 0;
                        Console.Error.WriteLine("SHOULD BE FIXED: replacing stuklijst regel met dezelfde volgnummer!:");
                    }
                }
            }
            // HACK HACK: dubbele artikelcode's
            for(int i = 0;i < StuklijstRegels.Count; i++) {
                if(StuklijstRegels[i].Artikel.Code == receptuurregel.Artikel.Code) {
                    StuklijstRegels[i] = receptuurregel;
                    Console.Error.WriteLine("SHOULD BE FIXED: replacing stuklijst regel met dezelfde code!:");
                    return;
                }
            }
            StuklijstRegels.Add(receptuurregel);
        }
    }
}
