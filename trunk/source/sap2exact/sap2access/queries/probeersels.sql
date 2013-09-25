USEFULL:
	http://www.stechno.net/sap-tables.html?view=saptable&id=STKO !!


                STAS                            
                STKO                            
                STPN                            
                STPO                            
                STPU                            
                STST                            
                STXH                            

/*
http://wiki.sdn.sap.com/wiki/display/Snippets/Determine+Base+Quantity+using+STKO+Valid-From+Date
TABLES: 
	AFKO,    " Order header data PP orders
    AFPO,    " Order item
	AFVC,    " Operation within an order
	AUFK,    " Order master data
	CRHD,    " Work Center Header
	MAKT,    " Material Descriptions
    MARA,    " Material Master: General Data
    ZMARA,   " Material Master: General Data
    MARC,    " Material Master: Plant Segment
    MAST,    " Material to BOM link
    RESB,    " Reservation/dependent reqmnts.
    STAS,    " BOMs - Item Selection
    STKO,    " BOM Header
    STPO,    " BOM item
    STZU.    " Permanent BOM data 
*/	
-- de actieve recepturen

SELECT
	MATNR,
	STLNR
FROM MAST
WHERE 
	MANDT = 100
AND
	AEDAT = 00000000
AND
	MATNR = '33024X99'
-- het receptuur nr
SELECT
	MATNR,
	STLNR
FROM MAST
WHERE 
	MANDT = 100
AND
	AEDAT = 00000000
AND
	MATNR = '33024X99'
-- actieve receptuur regels
SELECT
	POSNR,
	IDNRK,
	MENGE
FROM STPO
WHERE 
	MANDT = 100
AND
	STLNR = 00000206
AND	
	AEDAT = 00000000



-- more info at: http://scn.sap.com/thread/584486
-- and "FN CSSF_STPOB_READ" to create STPOB

-- info over BOM:
-- http://gbt00acs.benxbrain.com/en/index.do?onInputProcessing(brai_object_thread)&001_threadid=1944122&001_boardtype=01&sysid=WD5&pgmid=R3TR&object=TABL&obj_name=STKO&child_param=

-- MAST>STKO>STAS>STPO
---------------------------------------------
Wijzigingsdatum 
Technische naam: MASTB-AEDAT 
Mandant 
Technische naam: MASTB-MANDT 
Artikelnummer 
Technische naam: MASTB-MATNR 
Stuklijstalternatief 
Technische naam: MASTB-STLAL 
Stuklijstgebruik 
Technische naam: MASTB-STLAN 
Stuklijst 
Technische naam: MASTB-STLNR 
Vestiging 
Technische naam: MASTB-WERKS 
----------------------------
Laatste wijzigingsnummer 
Technische naam: STZUB-AENRL 
Stuklijstgroep 
Technische naam: STZUB-EXSTL 
Mandant 
Technische naam: STZUB-MANDT 
Stuklijstgebruik 
Technische naam: STZUB-STLAN 
Stuklijst 
Technische naam: STZUB-STLNR 
Stuklijsttype 
Technische naam: STZUB-STLTY 
Stuklijsttekst 
Technische naam: STZUB-ZTEXT 
Bevoegdheidsgroep stuklijsten 
Technische naam: STZUB-STLBE 
-----------------------------
Wijzigingsnummer 
Technische naam: STKOB-AENNR 
Stuklijst-basiseenheid 
Technische naam: STKOB-BMEIN 
Basishoeveelheid 
Technische naam: STKOB-BMENG 
Datum geldig van 
Technische naam: STKOB-DATUV 
Mandant 
Technische naam: STKOB-MANDT 
Interne teller 
Technische naam: STKOB-STKOZ 
Stuklijstalternatief 
Technische naam: STKOB-STLAL 
Lijstveld: 
Stuklijst 
Technische naam: STKOB-STLNR 
Stuklijsttype 
Technische naam: STKOB-STLTY 
Stuklijststatus 
Technische naam: STKOB-STLST 
-----------------------------
Componentuitval in procenten 
Technische naam: STPOB-AUSCH 
Operatieuitval 
Technische naam: STPOB-AVOAU 
Documentsoort 
Technische naam: STPOB-DOKAR 
Documentnummer 
Technische naam: STPOB-DOKNR 
Deeldocument 
Technische naam: STPOB-DOKTL 
Documentversie 
Technische naam: STPOB-DOKVR 
Vaste hoeveelheid 
Technische naam: STPOB-FMENG 
Stuklijstcomponent 
Technische naam: STPOB-IDNRK 
Mandant 
Technische naam: STPOB-MANDT 
Componenteenheid 
Technische naam: STPOB-MEINS 
Componenteenheid 
Technische naam: STPOB-MEINS 
Componenthoeveelheid 
Technische naam: STPOB-MENGE 
Objecttype (stuklijstpositie) 
Technische naam: STPOB-OBJTY 
Nummer van stuklijstpositie 
Technische naam: STPOB-POSNR 
Positietype (stuklijst) 
Technische naam: STPOB-POSTP 
Stuklijstpositietekst (regel 1) 
Technische naam: STPOB-POTX1 
Stuklijstpositietekst (regel 2) 
Technische naam: STPOB-POTX2 
Afgiftevest. 
Technische naam: STPOB-PSWRK 
Sorteerbegrip 
Technische naam: STPOB-SORTF 
Knoopnummer van stuklijstpositie 
Technische naam: STPOB-STLKN 
Stuklijst 
Technische naam: STPOB-STLNR 
Stuklijsttype 
Technische naam: STPOB-STLTY 
Interne teller 
Technische naam: STPOB-STPOZ 
Niveau (voor multiniveau stuklijstexplosies) 
Technische naam: STPOB-STUFE1
-----------------------------
Artikelomschrijving 
Technische naam: MAKT-MAKTX 






-- de actieve recepturen
SELECT
	MATNR,
	STLNR
FROM MAST
WHERE 
	MANDT = 100
AND
	AEDAT = 00000000
AND
	MATNR = '33024X99'
-- het receptuur nr
SELECT
	MATNR,
	STLNR
FROM MAST
WHERE 
	MANDT = 100
AND
	AEDAT = 00000000
AND
	MATNR = '33024X99'
-- actieve receptuur regels
SELECT
	POSNR,
	IDNRK,
	MENGE
FROM STPO
WHERE 
	MANDT = 100
AND
	STLNR = 00000206
AND	
	AEDAT = 00000000








	SELECT
    MAST.MATNR,
    MAST.STLNR,
    STKO.*
FROM MAST
LEFT OUTER JOIN STKO
    ON STKO.MANDT = MAST.MANDT
    AND STKO.STLNR= MAST.STLNR
WHERE 
    MAST.MATNR = '33024X99'
    
    
    SELECT DISTINCT 
    werks,
    matnr,
    b.stlal, 
    idnrk, 
    postp, 
    d.aennr,
   sortf, 
   potx1, 
   c.lkenz, 
   b.stlty, 
   stlst
   FROM mast AS a INNER JOIN stko AS b
     ON  a.stlnr = b.stlnr
     AND a.stlal = b.stlal
     INNER JOIN stas AS c
     ON  b.stlnr = c.stlnr
     AND b.stlal = c.stlal
     AND b.stlty = c.stlty
     INNER JOIN stpo AS d
     ON  c.stlnr = d.stlnr
     AND c.stlkn = d.stlkn
     AND c.stlty = d.stlty
  WHERE a.werks = '0001'
/*  
  AND ( ( d.andat <  20130814
        OR d.aedat <  20130814
        OR a.andat <  20130814
        OR a.aedat <  20130814
        OR b.andat <  20130814
        OR b.aedat  <  20130814
        OR c.andat <  20130814
        OR c.aedat <  20130814
        ) AND d.datuv >= lv_effdt )
*/        
     AND a.stlan = '1'
     AND matnr =  '33024X99'