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
    }
}
