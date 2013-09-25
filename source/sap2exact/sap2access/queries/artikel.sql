SELECT
	MARAV.MATNR AS "Artikelnummer",
	MARAV.BRGEW AS "Brutogewicht",
	MARAV.GEWEI AS "Gewichtseenheid",
	MARAV.MAKTX AS "Artikelomschrijving",
	MARAV.MATKL AS "Goederengroep",
	MARAV.MEINS AS "Basishoev#eenheid",
	MARAV.MHDHB AS "Totale houdbaarheid in dagen",
	MARAV.MTART AS "Artikelsoort",
	MARAV.NTGEW AS "Nettogewicht",
	MARAV.SPRAS AS "Taalcode ",
	MARAV.VPSTA AS "Verzorgingsstatus complete artk",
	MARAV.XCHPF AS "Teken voor chargeverplichting",

	MARM.MATNR AS MARM_MATNR,
	MARM.MEINH AS "Verplakking",
	MARM.BRGEW AS "Brutogewicht",
	MARM.EAN11 AS "Europees artikelnummer",
	MARM.GEWEI AS "Gewichtseenheid",
	MARM.UMREN AS "Noemer voor omrekening",
	MARM.UMREZ AS "Teller voor omrekening",

	MVKE.MATNR AS MVKE_MATNR,
	MVKE.KONDM AS "Artikelgroep",
	35019090 AS "statistieknummer"
FROM MARAV
LEFT OUTER JOIN MARM
    ON MARM.MANDT = MARAV.MANDT
    AND MARM.MATNR= MARAV.MATNR
LEFT OUTER JOIN MVKE
    ON MVKE.MANDT = MARAV.MANDT
    AND MVKE.MATNR= MARAV.MATNR
WHERE NOT MARAV.MATKL = 'VERVALLEN' 
ORDER BY MARAV.MATNR