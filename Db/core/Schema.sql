CREATE SCHEMA [core]
    AUTHORIZATION [dbo];
GO

EXECUTE sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Core shared components for unified dashboard system', 
    @level0type = N'SCHEMA',
    @level0name = N'core';
GO