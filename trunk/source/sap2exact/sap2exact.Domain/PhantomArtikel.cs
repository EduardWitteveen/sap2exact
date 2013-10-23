using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sap2exact.Domain
{
    public class PhantomArtikel: ReceptuurArtikel
    {
        public PhantomArtikel(BaseArtikel childartikel, double factor)
        {
            this.MateriaalCode = "HF" + childartikel.MateriaalCode;
            this.ArtikelOmschrijving = "Geweekte " + childartikel.ArtikelOmschrijving;
            this.ExactGewensteBelastingCategorie = childartikel.ExactGewensteBelastingCategorie;
            this.ExactGewensteNettoGewicht = childartikel.ExactGewensteNettoGewicht;
            this.BasishoeveelheidEenheid = childartikel.BasishoeveelheidEenheid;
            this.Gewichtseenheid = childartikel.Gewichtseenheid;
            this.NettoGewicht = childartikel.NettoGewicht;

            const int AANTAL_IN_RECEPTUUR = 100;
            var stuklijstmateriaal = new Domain.StuklijstRegel();
            stuklijstmateriaal.Volgnummer = 10;
            stuklijstmateriaal.Artikel = childartikel;
            stuklijstmateriaal.ReceptuurEenheid = "KN";
            stuklijstmateriaal.ReceptuurRegelAantal = AANTAL_IN_RECEPTUUR * factor;
            stuklijstmateriaal.ReceptuurEenheidFactor = 1;

            var stuklijstwater = new Domain.StuklijstRegel();
            stuklijstwater.Volgnummer = 20;
            stuklijstwater.Artikel = new Domain.WeekWater();
            stuklijstwater.ReceptuurEenheid = "KN";
            stuklijstwater.ReceptuurRegelAantal = AANTAL_IN_RECEPTUUR - stuklijstmateriaal.ReceptuurRegelAantal;
            stuklijstwater.ReceptuurEenheidFactor = 1;

            var stuklijst = new Domain.Stuklijst();
            stuklijst.StuklijstTotaalAantal = AANTAL_IN_RECEPTUUR;
            stuklijst.StuklijstNaam = "SAP2EXACT: KG->KN";
            stuklijst.StuklijstVersion = 1;
            stuklijst.StuklijstRegelsAdd(stuklijstmateriaal);
            stuklijst.StuklijstRegelsAdd(stuklijstwater);
            this.Stuklijsten.Add(stuklijst);
        }
    }
}
