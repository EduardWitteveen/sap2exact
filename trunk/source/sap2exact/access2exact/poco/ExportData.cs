using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace access2exact.poco
{
    public class ExportData
    {
        public Dictionary<string, poco.EindArtikel> eindartikelen = new Dictionary<string, poco.EindArtikel>();
        public Dictionary<string, poco.ReceptuurArtikel> receptuurartikelen = new Dictionary<string, poco.ReceptuurArtikel>();
        public Dictionary<string, poco.VerpakkingsArtikel> verpakkingsartikelen = new Dictionary<string, poco.VerpakkingsArtikel>();
        public Dictionary<string, poco.GrondstofArtikel> grondstofartikelen = new Dictionary<string, poco.GrondstofArtikel>();
        public Dictionary<string, poco.IngredientArtikel> ingredientartikelen = new Dictionary<string, poco.IngredientArtikel>();

        public void Add(BaseArtikel artikel)
        {
            if(artikel.GetType() == typeof(EindArtikel)) {
                //if(eindartikelen.ContainsKey(artikel.Code)) eindartikelen.Add(artikel.Code, (EindArtikel)artikel);
                eindartikelen.Add(artikel.Code, (EindArtikel)artikel);
            }
            else if(artikel.GetType() == typeof(ReceptuurArtikel)) {
                if (!receptuurartikelen.ContainsKey(artikel.Code)) receptuurartikelen.Add(artikel.Code, (ReceptuurArtikel)artikel);
            }
            else if(artikel.GetType() == typeof(VerpakkingsArtikel)) {
                if (!verpakkingsartikelen.ContainsKey(artikel.Code)) verpakkingsartikelen.Add(artikel.Code, (VerpakkingsArtikel)artikel);
            }
            else if(artikel.GetType() == typeof(GrondstofArtikel)) {
                if (!grondstofartikelen.ContainsKey(artikel.Code)) grondstofartikelen.Add(artikel.Code, (GrondstofArtikel)artikel);
            }
            else if (artikel.GetType() == typeof(IngredientArtikel))
            {
                if (!ingredientartikelen.ContainsKey(artikel.Code)) ingredientartikelen.Add(artikel.Code, (IngredientArtikel)artikel);
            }
            else throw new NotImplementedException("unknown type: " + artikel.GetType().FullName);
        }
    }
}
