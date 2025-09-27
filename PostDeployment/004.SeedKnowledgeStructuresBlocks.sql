-- ==========================================================================================
-- Post-Deployment Script for Seeding default Blocks for the Diagnostics Module
-- ==========================================================================================

; -- Sepparator semicolon before WITH statement

MERGE INTO [diagnostics].[Blocks] AS target
USING (VALUES
           ('Sobre la empresaria'),
           ('Información de género'),
           ('Sobre la empresa'),
           ('Corporativo'),
           ('Mercado'),
           ('Imagen y Comunicación'),
           ('Formalización'),
           ('Contable / Financiero'),
           ('Financiamiento'),
           ('Digitalización')
) AS source (Name)
ON target.Name = source.Name
WHEN NOT MATCHED THEN
    INSERT (Name)
    VALUES (source.Name);