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
            StuklijstRegels.Add(receptuurregel);
        }
    }
}
