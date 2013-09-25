SELECT
    MAST.MATNR AS "Artikelnummer",
    MAST.STLNR AS "Bill of material",
    MAST.STLAL AS "Alternative BOM" ,
    
    STKO.STLTY AS "BOM category",
    STKO.STLNR AS STKO_STLNR,
    STKO.STLAL AS STKO_STLAL,

    
    STAS.STLTY AS STAS_STLTY,
    STAS.STLNR AS STAS_STLNR,
    STAS.STLKN AS "BOM item node number",
    
    STPO.IDNRK AS "BOM component",
    STPO.POSNR AS "BOM Item Number",
    STPO.MENGE AS "Component quantity"
FROM MAST
INNER JOIN STKO
	ON STKO.LKENZ = ''
	AND STKO.LOEKZ = ''
	AND STKO.STLNR = MAST.STLNR
	AND STKO.STLAL = MAST.STLAL
	AND STKO.STKOZ NOT IN 
	(
		/* ALLEEN DE LAATSTE! */
		SELECT vorige_stko.VGKZL
		FROM STKO vorige_stko
		WHERE vorige_stko.STLNR = MAST.STLNR
		AND vorige_stko.STLAL = MAST.STLAL                    
	)
INNER JOIN STAS
	ON STAS.LKENZ = ''
	AND STAS.STLTY = STKO.STLTY
	AND STAS.STLNR = STKO.STLNR
                 /* STASZ = */
INNER JOIN STPO
	ON  STPO.LKENZ = ''
	AND STPO.STLTY = STAS.STLTY
	AND STPO.STLNR = STAS.STLNR
	AND STPO.STLKN = STAS.STLKN
	AND STPO.STPOZ NOT IN 
	(
		/* ALLEEN DE LAATSTE! */
		SELECT vorige_stpo.VGPZL
		FROM STPO vorige_stpo
		WHERE vorige_stpo.STLTY = STAS.STLTY
		AND vorige_stpo.STLNR = STAS.STLNR
	)  
WHERE MAST.MATNR = :artikelnummer
--ORDER BY STPO.POSNR