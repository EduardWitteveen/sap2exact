using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace sap2exact
{
    public class Domain2Xml
    {
        System.IO.FileInfo exportfile;
        XmlDocument xmldocument;
        XmlElement items;

        const string ARTIKEL_CODE_OPVULLING = "T9_______";
        const int ARTIKEL_CODE_LENGTE = 12;
        //const string OPVULLING = "VL_______";
        Conversie conversie;

        static string CreateSapCode(Domain.BaseArtikel artikel)
        {
            if(artikel.GetType() == typeof(Domain.WeekWater)) {
                return artikel.MateriaalCode;
            }
            string code = artikel.MateriaalCode.Trim();
            int len = code.Length;
            code = ARTIKEL_CODE_OPVULLING.Substring(0, ARTIKEL_CODE_LENGTE - len) + code;
            System.Diagnostics.Debug.Assert(code.Length == ARTIKEL_CODE_LENGTE);

            return code;
        }



        public Domain2Xml()
        {
            // conversie gegevens 
            conversie = new Conversie();

            // export location
            exportfile = new System.IO.FileInfo(Properties.Settings.Default.export_xml);

            // exact information object
            xmldocument = new XmlDocument();
            var xmldecl = xmldocument.CreateXmlDeclaration("1.0", "utf-8", null);

            //var eExact = xmldocument.CreateElement("", "eExact", "");
            var eExact = xmldocument.CreateElement("eExact");
            eExact.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            eExact.SetAttribute("xmlns:xsd", "http://www.w3.org/2001/XMLSchema");
            var schema = xmldocument.CreateAttribute("xsi", "noNamespaceSchemaLocation", "http://www.w3.org/2001/XMLSchema-instance");
            schema.Value = "eExact-Schema.xsd";
            eExact.SetAttributeNode(schema);
            xmldocument.AppendChild(eExact);
            xmldocument.InsertBefore(xmldecl, eExact);
            items = xmldocument.CreateElement("Items");
            eExact.AppendChild(items);
        }

        XmlElement CreateXmlRootArtikelElement(Domain.BaseArtikel artikel, Conversie.ArtikelGroepInformatie artikelgroepinformatie)
        {
            System.Diagnostics.Debug.WriteLine("artikel:" + artikel.MateriaalCode + " (" + artikel.GetType().Name + ")");

            var item = xmldocument.CreateElement("Item");
            item.SetAttribute("code", CreateSapCode(artikel));
            var description = xmldocument.CreateElement("Description");
            description.AppendChild(xmldocument.CreateTextNode(artikel.ArtikelOmschrijving));
            item.AppendChild(description);

            var multidescriptions = xmldocument.CreateElement("MultiDescriptions");
            for(int regel = 0; regel < 5; regel++) {
                var multidescription = xmldocument.CreateElement("MultiDescription");
                multidescription.SetAttribute("number", Convert.ToString(regel));
                string md = artikel.ArtikelOmschrijving;
                if (artikel.ArtikelOmschrijvingen.ContainsKey(regel))
                {
                    md = artikel.ArtikelOmschrijvingen[regel];
                }
                multidescription.AppendChild(xmldocument.CreateTextNode(md));
                multidescriptions.AppendChild(multidescription);
            }
            item.AppendChild(multidescriptions);
            ////////////////////////////////////////////////////////
            var assortment = xmldocument.CreateElement("Assortment");
            assortment.SetAttribute("number", artikelgroepinformatie.FinancieleArtikelgroep);
            var asdescription = xmldocument.CreateElement("Description");
            asdescription.AppendChild(xmldocument.CreateTextNode("Artikelgroep"));
            assortment.AppendChild(asdescription);
            AddGlStuff(assortment, artikelgroepinformatie);
            item.AppendChild(assortment);                
            ////////////////////////////////////////////////////////            
            var availability = xmldocument.CreateElement("Availability");
            var datestart = xmldocument.CreateElement("DateStart");
            datestart.AppendChild(xmldocument.CreateTextNode(
                        String.Format("{0:yyyy-MM-dd}", artikel.TimeStamp)
                        ));
            availability.AppendChild(datestart);
            item.AppendChild(availability);
            
            var issalesitem = xmldocument.CreateElement("IsSalesItem");
            // 1 if type = eindartikel
            issalesitem.AppendChild(xmldocument.CreateTextNode(artikel.GetType() == typeof(Domain.EindArtikel) ? "1" : "0"));
            //issalesitem.AppendChild(xmldocument.CreateTextNode("0"));
            item.AppendChild(issalesitem);

            var ispurchaseitem = xmldocument.CreateElement("IsPurchaseItem");
            ispurchaseitem.AppendChild(xmldocument.CreateTextNode(artikel.GetType() == typeof(Domain.GrondstofArtikel) || artikel.GetType() == typeof(Domain.VerpakkingsArtikel)  ? "1" : "0"));
            item.AppendChild(ispurchaseitem);

            var isserialnumberitem = xmldocument.CreateElement("IsSerialNumberItem");
            isserialnumberitem.AppendChild(xmldocument.CreateTextNode("0"));
            item.AppendChild(isserialnumberitem);

            var isbatchitem = xmldocument.CreateElement("IsBatchItem");
            isbatchitem.AppendChild(xmldocument.CreateTextNode("0"));
            item.AppendChild(isbatchitem);

            var issubassemblyitem = xmldocument.CreateElement("IsSubAssemblyItem");
            string issubassembly = artikel.GetType() != typeof(Domain.EindArtikel) ? "1" : "0";
            issubassemblyitem.AppendChild(xmldocument.CreateTextNode(issubassembly));
            item.AppendChild(issubassemblyitem);

            var isassembleditem = xmldocument.CreateElement("IsAssembledItem");
            // 1 if type = samengesteld
            //isassembleditem.AppendChild(xmldocument.CreateTextNode(typeof(Domain.BaseSamengesteldArtikel).IsAssignableFrom(artikel.GetType()) ? "1" : "0"));
            string isAssembled = (artikel.GetType() == typeof(Domain.EindArtikel) 
                || artikel.GetType() == typeof(Domain.ReceptuurArtikel) 
                || artikel.GetType() == typeof(Domain.PhantomArtikel)) ? "1" : "0";
            isassembleditem.AppendChild(xmldocument.CreateTextNode(isAssembled));
            item.AppendChild(isassembleditem);

            var isstockitem = xmldocument.CreateElement("IsStockItem");
            isstockitem.AppendChild(xmldocument.CreateTextNode("0"));
            item.AppendChild(isstockitem);

            var isfractionalloweditem = xmldocument.CreateElement("IsFractionAllowedItem");
            isfractionalloweditem.AppendChild(xmldocument.CreateTextNode("1"));
            item.AppendChild(isfractionalloweditem);
            /*
            var isoutsourceditem = xmldocument.CreateElement("IsOutsourcedItem");
            isoutsourceditem.AppendChild(xmldocument.CreateTextNode("0"));
            item.AppendChild(isoutsourceditem);
             */
            //////////////////////////////////////////////////////////
            AddGlStuff(item, artikelgroepinformatie);
            //////////////////////////////////////////////////////////
            // sales feest
            item.AppendChild(CreateXmlSalesElement(artikel));
            /////////////////////////////////////////////////////////
            var costs = xmldocument.CreateElement("Costs");
            var price = xmldocument.CreateElement("Price");
            var currency = xmldocument.CreateElement("Currency");
            currency.SetAttribute("code", "EUR");
            price.AppendChild(currency);
            var value= xmldocument.CreateElement("Value");
            value.AppendChild(xmldocument.CreateTextNode(Convert.ToString(artikel.KostPrijs)));
            price.AppendChild(value);
            costs.AppendChild(price);
            item.AppendChild(costs);


            var dimension = xmldocument.CreateElement("Dimension");
            var weightnet = xmldocument.CreateElement("WeightNet");
            weightnet.AppendChild(xmldocument.CreateTextNode(Convert.ToString(artikel.ExactGewensteNettoGewicht)));
            dimension.AppendChild(weightnet);
            item.AppendChild(dimension);
            return item;
        }

        private void AddGlStuff(XmlElement item, Conversie.ArtikelGroepInformatie artikelgroepinformatie)
        {
            var glrevenue = xmldocument.CreateElement("GLRevenue");
            glrevenue.SetAttribute("code", artikelgroepinformatie.FinancieleOmzet);
            var gldescription = xmldocument.CreateElement("Description");
            gldescription.AppendChild(xmldocument.CreateTextNode("SAP-IMPORT"));
            glrevenue.AppendChild(gldescription);
            item.AppendChild(glrevenue);

            var glcosts = xmldocument.CreateElement("GLCosts");
            glcosts.SetAttribute("code", artikelgroepinformatie.FinancieleKostprijsverkopen);
            gldescription = xmldocument.CreateElement("Description");
            gldescription.AppendChild(xmldocument.CreateTextNode("SAP-IMPORT"));
            glcosts.AppendChild(gldescription);
            item.AppendChild(glcosts);

            var glpurchase = xmldocument.CreateElement("GLPurchase");
            //glpurchase.SetAttribute("code", "     30620");
            glpurchase.SetAttribute("code", artikelgroepinformatie.FinancieleVoorraad);
            gldescription = xmldocument.CreateElement("Description");
            gldescription.AppendChild(xmldocument.CreateTextNode("SAP-IMPORT"));
            glpurchase.AppendChild(gldescription);
            item.AppendChild(glpurchase);

            var glaccountdiscount = xmldocument.CreateElement("GLAccountDiscount");
            //glaccountdiscount.SetAttribute("code", "     81020");
            glaccountdiscount.SetAttribute("code", artikelgroepinformatie.FinancieleKortingsrekening);
            gldescription = xmldocument.CreateElement("Description");
            gldescription.AppendChild(xmldocument.CreateTextNode("SAP-IMPORT"));
            glaccountdiscount.AppendChild(gldescription);
            item.AppendChild(glaccountdiscount);
        }

        private XmlNode CreateXmlSalesElement(Domain.BaseArtikel artikel)
        {
            var sales = xmldocument.CreateElement("Sales");
//            if (artikel.GetType() == typeof(Domain.EindArtikel))
//            {
                var price = xmldocument.CreateElement("Price");
                var currency = xmldocument.CreateElement("Currency");
                currency.SetAttribute("code", "EUR");
                price.AppendChild(currency);
                var value = xmldocument.CreateElement("Value");
                value.AppendChild(xmldocument.CreateTextNode("0.00"));
                price.AppendChild(value);
                var vat = xmldocument.CreateElement("VAT");
                vat.SetAttribute("code", Convert.ToString(artikel.ExactGewensteBelastingCategorie));
                vat.SetAttribute("type", "B");
                vat.SetAttribute("vattype", "E");
                //vat.SetAttribute("taxtype", "V");
                price.AppendChild(vat);
                sales.AppendChild(price);
//            }            
            var unit = xmldocument.CreateElement("Unit");
            //unit.SetAttribute("unit", artikel.Gewichtseenheid);
            //unit.SetAttribute("unit", artikel.BasishoeveelheidEenheid);
            unit.SetAttribute("unit", Convert2VerpakkingsType(artikel));
            unit.SetAttribute("type", "W");
            unit.SetAttribute("active", "1");
            sales.AppendChild(unit);
            return sales;
        }

        XmlElement AddXmlSamengesteldArtikel(XmlElement item, Domain.BaseSamengesteldArtikel artikel)
        {
            if (typeof(Domain.EindArtikel) == artikel.GetType())
            {
                var statistical = xmldocument.CreateElement("Statistical");
                var number = xmldocument.CreateElement("Number");
                number.AppendChild(xmldocument.CreateTextNode(((Domain.EindArtikel)artikel).Intrastat));
                statistical.AppendChild(number);
                var units = xmldocument.CreateElement("Units");
                units.AppendChild(xmldocument.CreateTextNode("0"));
                statistical.AppendChild(units);
                item.AppendChild(statistical);
            }

            if(artikel.Stuklijsten.Count > 0) {
                // boms
                var boms = xmldocument.CreateElement("BOMs");

                foreach (Domain.Stuklijst stuklijst in artikel.Stuklijsten)
                {
                    var bom = xmldocument.CreateElement("BOM");
                    bom.SetAttribute("code", CreateSapCode(artikel));
                    bom.SetAttribute("versionnumber", Convert.ToString(stuklijst.StuklijstVersion));

                    var description = xmldocument.CreateElement("Description");
                    var stuklijstnaam = stuklijst.StuklijstNaam;
                    if (stuklijstnaam.Length == 0)
                    {
                        stuklijstnaam = "Standaard";
                    }
                    else if (stuklijstnaam.Length > 30)
                    {
                        stuklijstnaam = stuklijstnaam.Substring(0, 30);
                    }
                    description.AppendChild(xmldocument.CreateTextNode(stuklijstnaam));
                    bom.AppendChild(description);

                    var effectivedate = xmldocument.CreateElement("EffectiveDate");
                    effectivedate.AppendChild(xmldocument.CreateTextNode(
                        String.Format("{0:yyyy-MM-dd}", stuklijst.StuklijstDatum)
                        ));
                    bom.AppendChild(effectivedate);

                    var quantity = xmldocument.CreateElement("Quantity");
                    quantity.AppendChild(xmldocument.CreateTextNode(stuklijst.StuklijstTotaalAantal.ToString()));
                    bom.AppendChild(quantity);

                    foreach (Domain.StuklijstRegel receptuurregel in stuklijst.StuklijstRegels)
                    {
                        var bomline = xmldocument.CreateElement("BOMLine");
                        bomline.SetAttribute("type", "I");
                        bomline.SetAttribute("sequencenumber", receptuurregel.Volgnummer.ToString());
                        var bomitem = xmldocument.CreateElement("Item");
                        bomitem.SetAttribute("code", CreateSapCode(receptuurregel.Artikel));
                        var bomdescription = xmldocument.CreateElement("Description");

                        bomdescription.AppendChild(xmldocument.CreateTextNode(receptuurregel.Artikel.ArtikelOmschrijving));
                        bomitem.AppendChild(bomdescription);
                        bomline.AppendChild(bomitem);

                        var condition = xmldocument.CreateElement("Condition");
                        string conditiontext = "N";
                        if(receptuurregel.Artikel.GetType() == typeof(Domain.PhantomArtikel) ) {
                            conditiontext = "F";
                        }
                        else if(receptuurregel.Artikel.GetType() == typeof(Domain.AfvalArtikel) ) {
                            conditiontext = "W";
                        }
                        condition.AppendChild(xmldocument.CreateTextNode( conditiontext ));
                        bomline.AppendChild(condition);

                        var backflush = xmldocument.CreateElement("BackFlush");
                        backflush.AppendChild(xmldocument.CreateTextNode("0"));
                        bomline.AppendChild(backflush);

                        var bomquantity = xmldocument.CreateElement("Quantity");
                        bomquantity.AppendChild(xmldocument.CreateTextNode(receptuurregel.ReceptuurRegelAantal.ToString()));
                        bomline.AppendChild(bomquantity);

                        bom.AppendChild(bomline);
                    }

                    boms.AppendChild(bom);
                    item.AppendChild(boms);
                }
            }
            return item;
        }


        XmlElement AddXmlRootArtikelFooter(XmlElement item, Domain.BaseArtikel artikel)
        {
            // leverancier info
            var itemaccounts = xmldocument.CreateElement("ItemAccounts");
            var itemaccount = xmldocument.CreateElement("ItemAccount");
            var account = xmldocument.CreateElement("Account");

            string leverancierscode = "999990";
            string leverancierstekst = "Samengesteld:" + artikel.MateriaalCode;
            if (artikel.GetType() == typeof(Domain.EindArtikel))
            {
                leverancierscode = "040012";
                leverancierstekst = "Eindartikel:" + artikel.MateriaalCode;
            }
            else if (artikel.GetType() == typeof(Domain.GrondstofArtikel))
            {
                leverancierscode = "999980";
                leverancierstekst = "Grondstof:" + artikel.MateriaalCode;
            }
            else if (artikel.GetType() == typeof(Domain.VerpakkingsArtikel))
            {
                leverancierscode = "999980";
                leverancierstekst = "Verpakking:" + artikel.MateriaalCode;
            }
            account.SetAttribute("code", leverancierscode);
            itemaccount.AppendChild(account);
            var itemcode = xmldocument.CreateElement("ItemCode");
            itemcode.AppendChild(xmldocument.CreateTextNode(leverancierstekst));
            itemaccount.AppendChild(itemcode);

            var purchase = xmldocument.CreateElement("Purchase");
            var price = xmldocument.CreateElement("Price");
            var currency = xmldocument.CreateElement("Currency");
            currency.SetAttribute("code", "EUR");
            price.AppendChild(currency);
            purchase.AppendChild(price);
            var value = xmldocument.CreateElement("Value");
            value.AppendChild(xmldocument.CreateTextNode("0"));
            price.AppendChild(value);

            var unit = xmldocument.CreateElement("Unit");
            //unit.SetAttribute("unit", artikel.BasishoeveelheidEenheid);
            unit.SetAttribute("unit", Convert2VerpakkingsType(artikel, true));            
            unit.SetAttribute("type", "O");
            unit.SetAttribute("active", "1");
            purchase.AppendChild(unit);

            var salesunit = xmldocument.CreateElement("SalesUnits");
            salesunit.AppendChild(xmldocument.CreateTextNode(Convert.ToString(artikel.NettoGewicht)));
            purchase.AppendChild(salesunit);
            itemaccount.AppendChild(purchase);

            itemaccounts.AppendChild(itemaccount);
            item.AppendChild(itemaccounts);

            // magazijn info
            var itemwarehouses = xmldocument.CreateElement("ItemWarehouses");
            var itemwarehouse = xmldocument.CreateElement("ItemWarehouse");
            itemwarehouse.SetAttribute("default", "1");
            var warehouse = xmldocument.CreateElement("Warehouse");
            warehouse.SetAttribute("code", "300");
            itemwarehouse.AppendChild(warehouse);
            itemwarehouses.AppendChild(itemwarehouse);
            item.AppendChild(itemwarehouses);

            // houdbaarheid in dagen
            var shelflife = xmldocument.CreateElement("ShelfLife");
            //shelflife.AppendChild(xmldocument.CreateTextNode(Convert.ToString(artikel.HoudbaarheidInDagen)));
            shelflife.AppendChild(xmldocument.CreateTextNode("0"));
            item.AppendChild(shelflife);            

            // classificatie icp = goederen
            var classification = xmldocument.CreateElement("TaxItemClassification");
            classification.AppendChild(xmldocument.CreateTextNode("10"));
            item.AppendChild(classification);

            return item;
        }

        public string Convert2VerpakkingsType(Domain.BaseArtikel artikel, bool salesunit = false)
        {
            string verpakkingstype;
            System.Diagnostics.Debug.Assert(!salesunit);
            if (salesunit)
            {
                // TODO: gaat fout!!
                // SAP2EXACT: aantalconversie
                if (artikel.GetType() == typeof(Domain.VerpakkingsArtikel))
                {
                    verpakkingstype = artikel.BasishoeveelheidEenheid;
                }
                else
                {
                    verpakkingstype = artikel.Gewichtseenheid;
                }
                ////////////////////////////////////////////////////
            }
            else
            {
                // SAP2EXACT: aantalconversie
                if (artikel.GetType() == typeof(Domain.VerpakkingsArtikel))
                {
                    verpakkingstype = artikel.BasishoeveelheidEenheid;
                }
                else
                {
                    verpakkingstype = artikel.Gewichtseenheid;
                }
                ////////////////////////////////////////////////////
            }
            if (verpakkingstype == "DS")
            {
                verpakkingstype = "doos";
            }
            else if (verpakkingstype == "KG")
            {
                verpakkingstype = "kg";
            }
            else if (verpakkingstype == "ST")
            {
                verpakkingstype = "stuk";
            }
            else if (verpakkingstype == "ZAK")
            {
                verpakkingstype = "zak";
            }
            else
            {
                Output.Error("unknown eenheid:" + verpakkingstype);
                throw new NotImplementedException("no support voor verpakkingstype: " + verpakkingstype);
            }
            return verpakkingstype;
        }


        void ExportArtikelen(Domain.ExportData data)
        {
            // WE NEGEREN HIER DE INGREDIENTEN!!!

            foreach (Domain.GrondstofArtikel grondstofartikel in data.GrondstofArtikelen.Values)
            {
                items.AppendChild(xmldocument.CreateComment("GrondStof:" + grondstofartikel.MateriaalCode));
                Conversie.ArtikelGroepInformatie agi = conversie.GetArtikelGroepInformatie(grondstofartikel);
                var item = CreateXmlRootArtikelElement(grondstofartikel, agi);
                item = AddXmlVrijeveldenArtikel(item, grondstofartikel, agi);
                item = AddXmlRootArtikelFooter(item, grondstofartikel);
                items.AppendChild(item);
            }
            foreach (Domain.VerpakkingsArtikel verpakkingartikel in data.VerpakkingsArtikelen.Values)
            {
                items.AppendChild(xmldocument.CreateComment("Verpakking:" + verpakkingartikel.MateriaalCode));
                Conversie.ArtikelGroepInformatie agi = conversie.GetArtikelGroepInformatie(verpakkingartikel);
                var item = CreateXmlRootArtikelElement(verpakkingartikel,agi);
                item = AddXmlVrijeveldenArtikel(item, verpakkingartikel, agi);
                item = AddXmlRootArtikelFooter(item, verpakkingartikel);
                items.AppendChild(item);
            }
            foreach (Domain.ReceptuurArtikel receptuurartikel in data.ReceptuurArtikelen.Values)
            {
                items.AppendChild(xmldocument.CreateComment("Receptuur:" + receptuurartikel.MateriaalCode));
                Conversie.ArtikelGroepInformatie agi = conversie.GetArtikelGroepInformatie(receptuurartikel);
                var item = CreateXmlRootArtikelElement(receptuurartikel, agi);
                item = AddXmlSamengesteldArtikel(item, receptuurartikel);
                item = AddXmlVrijeveldenArtikel(item, receptuurartikel, agi);
                item = AddXmlRootArtikelFooter(item, receptuurartikel);
                items.AppendChild(item);
            }
            foreach (Domain.EindArtikel eindartikel in data.EindArtikelen.Values)
            {
                items.AppendChild(xmldocument.CreateComment("Eindartikel:" + eindartikel.MateriaalCode));
                Conversie.ArtikelGroepInformatie agi = conversie.GetArtikelGroepInformatie(eindartikel);
                var item = CreateXmlRootArtikelElement(eindartikel, agi);
                item = AddXmlSamengesteldArtikel(item, eindartikel);
                item = AddXmlVrijeveldenArtikel(item, eindartikel, agi);
                item = AddXmlRootArtikelFooter(item, eindartikel);
                items.AppendChild(item);
            }
        }

        private XmlElement AddXmlVrijeveldenArtikel(XmlElement item, Domain.BaseArtikel artikel, Conversie.ArtikelGroepInformatie artikelgroepinformatie)
        {

            var resource = xmldocument.CreateElement("Resource");
            resource.SetAttribute("code", "witteveen-automatisering.nl");
            var lastname = xmldocument.CreateElement("LastName");
            lastname.AppendChild(xmldocument.CreateTextNode("Automatisering"));
            resource.AppendChild(lastname);
            var firstname = xmldocument.CreateElement("FirstName");
            firstname.AppendChild(xmldocument.CreateTextNode("Witteveen"));
            resource.AppendChild(firstname);
            resource.AppendChild(firstname);
            item.AppendChild(resource);

            var freefields = xmldocument.CreateElement("FreeFields");
            var freetexts = xmldocument.CreateElement("FreeTexts");
            freefields.AppendChild(freetexts);

            var freedates = xmldocument.CreateElement("FreeDates");
            freefields.AppendChild(freedates);

            var freenumbers = xmldocument.CreateElement("FreeNumbers");
            // tht
            var freenumer2 = xmldocument.CreateElement("FreeNumber");
            freenumer2.SetAttribute("number", "2");
            freenumer2.AppendChild(xmldocument.CreateTextNode(Convert.ToString(artikel.HoudbaarheidInDagen)));
            freenumbers.AppendChild(freenumer2);
            // aantal in verpakking
            var freenumer7 = xmldocument.CreateElement("FreeNumber");
            freenumer7.SetAttribute("number", "7");
            freenumer7.AppendChild(xmldocument.CreateTextNode(Convert.ToString(artikel.NettoGewicht)));
            freenumbers.AppendChild(freenumer7);
            
            freefields.AppendChild(freenumbers);

            var freeyesnos = xmldocument.CreateElement("FreeYesNos");
            freefields.AppendChild(freeyesnos);
            item.AppendChild(freefields);

            // class 1: basis artikelgroep
            var itemcategory = xmldocument.CreateElement("ItemCategory");
            itemcategory.SetAttribute("number", "1");
            itemcategory.SetAttribute("code", artikelgroepinformatie.BasisArtikelgroep);
            var description = xmldocument.CreateElement("Description");
            description.AppendChild(xmldocument.CreateTextNode(artikelgroepinformatie.BasisArtikelgroep));
            itemcategory.AppendChild(description);
            item.AppendChild(itemcategory);

            // class 2: basis productgroep
            itemcategory = xmldocument.CreateElement("ItemCategory");
            itemcategory.SetAttribute("number", "2");
            itemcategory.SetAttribute("code", artikelgroepinformatie.BasisProductgroep);
            description = xmldocument.CreateElement("Description");
            description.AppendChild(xmldocument.CreateTextNode(artikelgroepinformatie.BasisProductgroep));
            itemcategory.AppendChild(description);
            item.AppendChild(itemcategory);

            // class 3
            string iccode = "30";
            string icdescription = "Halffabrikaten";
            if (artikel.GetType() == typeof(Domain.GrondstofArtikel))
            {
                iccode = "10";
                icdescription = "Grondstoffen";
            }
            else if (artikel.GetType() == typeof(Domain.VerpakkingsArtikel))
            {
                iccode = "20";
                icdescription = "Verpakkingen";
            }
            else if (artikel.GetType() == typeof(Domain.ReceptuurArtikel))
            {
                iccode = "30";
                icdescription = "Halffabrikaten";
            }
            else if (artikel.GetType() == typeof(Domain.PhantomArtikel))
            {
                iccode = "30";
                icdescription = "Halffabrikaten";
            }
            else if (artikel.GetType() == typeof(Domain.EindArtikel))
            {
                iccode = "40";
                icdescription = "Gereed Product";
            }
            else throw new NotImplementedException("unsupported article type");

            itemcategory = xmldocument.CreateElement("ItemCategory");
            itemcategory.SetAttribute("number", "3");
            itemcategory.SetAttribute("code", iccode);
            description = xmldocument.CreateElement("Description");
            description.AppendChild(xmldocument.CreateTextNode(icdescription));
            itemcategory.AppendChild(description);
            item.AppendChild(itemcategory);
            if (artikel.GetType() == typeof(Domain.EindArtikel))
            {
                // class 6
                itemcategory = xmldocument.CreateElement("ItemCategory");
                itemcategory.SetAttribute("number", "6");
                itemcategory.SetAttribute("code", "30");
                description = xmldocument.CreateElement("Description");
                description.AppendChild(xmldocument.CreateTextNode("Productie-artikel Vlaardingen"));
                itemcategory.AppendChild(description);
                item.AppendChild(itemcategory);
                // class 7
                itemcategory = xmldocument.CreateElement("ItemCategory");
                itemcategory.SetAttribute("number", "7");
                itemcategory.SetAttribute("code", "BULK");
                description = xmldocument.CreateElement("Description");
                description.AppendChild(xmldocument.CreateTextNode("Bulkproduct"));
                itemcategory.AppendChild(description);
                item.AppendChild(itemcategory);
            }
            return item;
        }

        public void WriteData(Domain.ExportData data)
        {
            ExportArtikelen(data);

            if (exportfile.Exists) exportfile.Delete();
            xmldocument.Save(exportfile.FullName);
        }
    }
}
