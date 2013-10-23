using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace sap2exact.Domain
{
    public class ExportData        
    {
        public virtual Guid Id { get; set; }


        public virtual Dictionary<string, Domain.BaseArtikel> AlleArtikelen { get; set; }

        public virtual Dictionary<string, Domain.EindArtikel> EindArtikelen { get; set; }
        public virtual Dictionary<string, Domain.ReceptuurArtikel> ReceptuurArtikelen { get; set; }
        public virtual Dictionary<string, Domain.VerpakkingsArtikel> VerpakkingsArtikelen { get; set; }
        public virtual Dictionary<string, Domain.GrondstofArtikel> GrondstofArtikelen { get; set; }
        public virtual Dictionary<string, Domain.IngredientArtikel> IngredientArtikelen { get; set; }

        public ExportData()
        {
            Id = Guid.NewGuid();

            AlleArtikelen = new Dictionary<string, Domain.BaseArtikel>();
            EindArtikelen = new Dictionary<string, Domain.EindArtikel>();
            ReceptuurArtikelen = new Dictionary<string, Domain.ReceptuurArtikel>();
            VerpakkingsArtikelen = new Dictionary<string, Domain.VerpakkingsArtikel>();
            GrondstofArtikelen = new Dictionary<string, Domain.GrondstofArtikel>();
            IngredientArtikelen = new Dictionary<string, Domain.IngredientArtikel>();
        }

        public virtual Domain.BaseArtikel Retrieve(String artikelcode)
        {
            if (AlleArtikelen.ContainsKey(artikelcode))
            {
                return AlleArtikelen[artikelcode];
            }
            return null;
        }

        public virtual void Add(BaseArtikel artikel)
        {
            if(artikel.GetType() == typeof(EindArtikel)) {
                EindArtikelen.Add(artikel.MateriaalCode, (EindArtikel)artikel);
            }
            else if (artikel.GetType() == typeof(ReceptuurArtikel) || artikel.GetType() == typeof(PhantomArtikel))
            {
                ReceptuurArtikelen.Add(artikel.MateriaalCode, (ReceptuurArtikel)artikel);
            }
            else if(artikel.GetType() == typeof(VerpakkingsArtikel)) {
                VerpakkingsArtikelen.Add(artikel.MateriaalCode, (VerpakkingsArtikel)artikel);
            }
            else if(artikel.GetType() == typeof(GrondstofArtikel)) {
                GrondstofArtikelen.Add(artikel.MateriaalCode, (GrondstofArtikel)artikel);
            }
            else if (artikel.GetType() == typeof(IngredientArtikel))
            {
                IngredientArtikelen.Add(artikel.MateriaalCode, (IngredientArtikel)artikel);

            }
            else throw new NotImplementedException("unknown type: " + artikel.GetType().FullName);
            AlleArtikelen.Add(artikel.MateriaalCode, artikel);
        }
    }
}