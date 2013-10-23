using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sap2exact.Domain
{
    public class HoeveelheidsEenheid
    {
        public string vanEenheid { get; set; }
        public string naarEenheid { get; set; }

        public double factor { get; set; }
    }
}
