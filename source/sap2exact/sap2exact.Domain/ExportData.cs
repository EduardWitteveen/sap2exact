using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace access2exact.Domain
{
    public class ExportData        
    {
        public virtual Guid Id { get; set; }


        private Dictionary<string, Domain.BaseArtikel> artikelen = new Dictionary<string, Domain.BaseArtikel>();

        public virtual Dictionary<string, Domain.EindArtikel> EindArtikelen { get; set; }
        public virtual Dictionary<string, Domain.ReceptuurArtikel> ReceptuurArtikelen { get; set; }
        public virtual Dictionary<string, Domain.VerpakkingsArtikel> VerpakkingsArtikelen { get; set; }
        public virtual Dictionary<string, Domain.GrondstofArtikel> GrondstofArtikelen { get; set; }
        public virtual Dictionary<string, Domain.IngredientArtikel> IngredientArtikelen { get; set; }

        public ExportData()
        {
            EindArtikelen = new Dictionary<string, Domain.EindArtikel>();
            ReceptuurArtikelen = new Dictionary<string, Domain.ReceptuurArtikel>();
            VerpakkingsArtikelen = new Dictionary<string, Domain.VerpakkingsArtikel>();
            GrondstofArtikelen = new Dictionary<string, Domain.GrondstofArtikel>();
            IngredientArtikelen = new Dictionary<string, Domain.IngredientArtikel>();
        }

        public Domain.BaseArtikel Retrieve(String artikelcode)
        {
            if (artikelen.ContainsKey(artikelcode))
            {
                return artikelen[artikelcode];
            }
            return null;
        }

        public virtual void Add(BaseArtikel artikel)
        {
            if(artikel.GetType() == typeof(EindArtikel)) {
                if(EindArtikelen.ContainsKey(artikel.Code)) {
                    EindArtikelen.Add(artikel.Code, (EindArtikel)artikel);
                    artikelen.Add(artikel.Code, artikel);
                }
            }
            else if(artikel.GetType() == typeof(ReceptuurArtikel)) {
                if (!ReceptuurArtikelen.ContainsKey(artikel.Code))
                {
                    ReceptuurArtikelen.Add(artikel.Code, (ReceptuurArtikel)artikel);
                    artikelen.Add(artikel.Code, artikel);
                }
            }
            else if(artikel.GetType() == typeof(VerpakkingsArtikel)) {
                if (!VerpakkingsArtikelen.ContainsKey(artikel.Code))
                {
                    VerpakkingsArtikelen.Add(artikel.Code, (VerpakkingsArtikel)artikel);
                    artikelen.Add(artikel.Code, artikel);
                }
            }
            else if(artikel.GetType() == typeof(GrondstofArtikel)) {
                if (!GrondstofArtikelen.ContainsKey(artikel.Code))
                {
                    GrondstofArtikelen.Add(artikel.Code, (GrondstofArtikel)artikel);
                    artikelen.Add(artikel.Code, artikel);
                }
            }
            else if (artikel.GetType() == typeof(IngredientArtikel))
            {
                if (!IngredientArtikelen.ContainsKey(artikel.Code))
                {
                    IngredientArtikelen.Add(artikel.Code, (IngredientArtikel)artikel);
                    artikelen.Add(artikel.Code, artikel);
                }
            }
            else throw new NotImplementedException("unknown type: " + artikel.GetType().FullName);
        }
    }
}
