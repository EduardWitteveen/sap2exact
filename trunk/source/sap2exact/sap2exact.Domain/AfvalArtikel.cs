using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sap2exact.Domain
{
    public class AfvalArtikel: GrondstofArtikel
    {
        public AfvalArtikel(GrondstofArtikel ga)
        {
            // CAUTION: we keep the reference to the objects of the other object!!
            this.ArtikelOmschrijving = ga.ArtikelOmschrijving;
            this.ArtikelOmschrijvingen = ga.ArtikelOmschrijvingen;
            this.BasishoeveelheidEenheid = ga.BasishoeveelheidEenheid;
            this.BruttoGewicht = ga.BruttoGewicht;
            this.ExactGewensteBelastingCategorie = ga.ExactGewensteBelastingCategorie;
            this.ExactGewensteNettoGewicht = ga.ExactGewensteNettoGewicht;
            this.Gewichtseenheid = ga.Gewichtseenheid;
            this.HoeveelheidsEenheden = ga.HoeveelheidsEenheden;
            this.HoudbaarheidInDagen = ga.HoudbaarheidInDagen;
            this.Intrastat = ga.Intrastat;
            this.KostPrijs = ga.KostPrijs;
            this.MateriaalCode = ga.MateriaalCode;
            this.NettoGewicht = ga.NettoGewicht;
            this.Stuklijsten = ga.Stuklijsten;
            this.TimeStamp = ga.TimeStamp;
            this.VerkoopPrijs = ga.VerkoopPrijs;
        }

        public static StuklijstRegel CreateStuklijstRegel(StuklijstRegel receptuurregel, double factor)
        {
            StuklijstRegel afvalregel = receptuurregel.Clone();
            afvalregel.Volgnummer += 1;
            afvalregel.ReceptuurRegelAantal = (afvalregel.ReceptuurRegelAantal * factor) - afvalregel.ReceptuurRegelAantal;
            afvalregel.Artikel = new AfvalArtikel((GrondstofArtikel)receptuurregel.Artikel);
            return afvalregel;
        }
    }
}
