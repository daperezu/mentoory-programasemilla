// Shared form constants module
// Single source of truth for all specialized form inputs

(function(window) {
    'use strict';

    // Constants definitions
    const FormConstants = {
        // Nationality list with ISO codes
        nationalities: [
            // Americas
            { value: 'AR', text: 'Argentina' },
            { value: 'BO', text: 'Bolivia' },
            { value: 'BR', text: 'Brasil' },
            { value: 'CA', text: 'Canadá' },
            { value: 'CL', text: 'Chile' },
            { value: 'CO', text: 'Colombia' },
            { value: 'CR', text: 'Costa Rica' },
            { value: 'CU', text: 'Cuba' },
            { value: 'DO', text: 'República Dominicana' },
            { value: 'EC', text: 'Ecuador' },
            { value: 'SV', text: 'El Salvador' },
            { value: 'US', text: 'Estados Unidos' },
            { value: 'GT', text: 'Guatemala' },
            { value: 'HT', text: 'Haití' },
            { value: 'HN', text: 'Honduras' },
            { value: 'JM', text: 'Jamaica' },
            { value: 'MX', text: 'México' },
            { value: 'NI', text: 'Nicaragua' },
            { value: 'PA', text: 'Panamá' },
            { value: 'PY', text: 'Paraguay' },
            { value: 'PE', text: 'Perú' },
            { value: 'PR', text: 'Puerto Rico' },
            { value: 'UY', text: 'Uruguay' },
            { value: 'VE', text: 'Venezuela' },
            
            // Europe
            { value: 'AL', text: 'Albania' },
            { value: 'DE', text: 'Alemania' },
            { value: 'AD', text: 'Andorra' },
            { value: 'AT', text: 'Austria' },
            { value: 'BE', text: 'Bélgica' },
            { value: 'BY', text: 'Bielorrusia' },
            { value: 'BA', text: 'Bosnia y Herzegovina' },
            { value: 'BG', text: 'Bulgaria' },
            { value: 'HR', text: 'Croacia' },
            { value: 'DK', text: 'Dinamarca' },
            { value: 'SK', text: 'Eslovaquia' },
            { value: 'SI', text: 'Eslovenia' },
            { value: 'ES', text: 'España' },
            { value: 'EE', text: 'Estonia' },
            { value: 'FI', text: 'Finlandia' },
            { value: 'FR', text: 'Francia' },
            { value: 'GR', text: 'Grecia' },
            { value: 'HU', text: 'Hungría' },
            { value: 'IE', text: 'Irlanda' },
            { value: 'IS', text: 'Islandia' },
            { value: 'IT', text: 'Italia' },
            { value: 'LV', text: 'Letonia' },
            { value: 'LI', text: 'Liechtenstein' },
            { value: 'LT', text: 'Lituania' },
            { value: 'LU', text: 'Luxemburgo' },
            { value: 'MK', text: 'Macedonia del Norte' },
            { value: 'MT', text: 'Malta' },
            { value: 'MD', text: 'Moldavia' },
            { value: 'MC', text: 'Mónaco' },
            { value: 'ME', text: 'Montenegro' },
            { value: 'NO', text: 'Noruega' },
            { value: 'NL', text: 'Países Bajos' },
            { value: 'PL', text: 'Polonia' },
            { value: 'PT', text: 'Portugal' },
            { value: 'GB', text: 'Reino Unido' },
            { value: 'CZ', text: 'República Checa' },
            { value: 'RO', text: 'Rumania' },
            { value: 'RU', text: 'Rusia' },
            { value: 'SM', text: 'San Marino' },
            { value: 'RS', text: 'Serbia' },
            { value: 'SE', text: 'Suecia' },
            { value: 'CH', text: 'Suiza' },
            { value: 'UA', text: 'Ucrania' },
            { value: 'VA', text: 'Vaticano' },
            
            // Asia
            { value: 'AF', text: 'Afganistán' },
            { value: 'SA', text: 'Arabia Saudita' },
            { value: 'AM', text: 'Armenia' },
            { value: 'AZ', text: 'Azerbaiyán' },
            { value: 'BH', text: 'Baréin' },
            { value: 'BD', text: 'Bangladesh' },
            { value: 'BT', text: 'Bután' },
            { value: 'BN', text: 'Brunéi' },
            { value: 'KH', text: 'Camboya' },
            { value: 'CN', text: 'China' },
            { value: 'CY', text: 'Chipre' },
            { value: 'KP', text: 'Corea del Norte' },
            { value: 'KR', text: 'Corea del Sur' },
            { value: 'AE', text: 'Emiratos Árabes Unidos' },
            { value: 'PH', text: 'Filipinas' },
            { value: 'GE', text: 'Georgia' },
            { value: 'IN', text: 'India' },
            { value: 'ID', text: 'Indonesia' },
            { value: 'IQ', text: 'Irak' },
            { value: 'IR', text: 'Irán' },
            { value: 'IL', text: 'Israel' },
            { value: 'JP', text: 'Japón' },
            { value: 'JO', text: 'Jordania' },
            { value: 'KZ', text: 'Kazajistán' },
            { value: 'KW', text: 'Kuwait' },
            { value: 'KG', text: 'Kirguistán' },
            { value: 'LA', text: 'Laos' },
            { value: 'LB', text: 'Líbano' },
            { value: 'MY', text: 'Malasia' },
            { value: 'MV', text: 'Maldivas' },
            { value: 'MN', text: 'Mongolia' },
            { value: 'MM', text: 'Myanmar' },
            { value: 'NP', text: 'Nepal' },
            { value: 'OM', text: 'Omán' },
            { value: 'PK', text: 'Pakistán' },
            { value: 'PS', text: 'Palestina' },
            { value: 'QA', text: 'Qatar' },
            { value: 'SG', text: 'Singapur' },
            { value: 'SY', text: 'Siria' },
            { value: 'LK', text: 'Sri Lanka' },
            { value: 'TJ', text: 'Tayikistán' },
            { value: 'TH', text: 'Tailandia' },
            { value: 'TL', text: 'Timor Oriental' },
            { value: 'TM', text: 'Turkmenistán' },
            { value: 'TR', text: 'Turquía' },
            { value: 'UZ', text: 'Uzbekistán' },
            { value: 'VN', text: 'Vietnam' },
            { value: 'YE', text: 'Yemen' },
            
            // Africa
            { value: 'DZ', text: 'Argelia' },
            { value: 'AO', text: 'Angola' },
            { value: 'BJ', text: 'Benín' },
            { value: 'BW', text: 'Botsuana' },
            { value: 'BF', text: 'Burkina Faso' },
            { value: 'BI', text: 'Burundi' },
            { value: 'CV', text: 'Cabo Verde' },
            { value: 'CM', text: 'Camerún' },
            { value: 'TD', text: 'Chad' },
            { value: 'KM', text: 'Comoras' },
            { value: 'CG', text: 'Congo' },
            { value: 'CD', text: 'República Democrática del Congo' },
            { value: 'CI', text: 'Costa de Marfil' },
            { value: 'DJ', text: 'Yibuti' },
            { value: 'EG', text: 'Egipto' },
            { value: 'GQ', text: 'Guinea Ecuatorial' },
            { value: 'ER', text: 'Eritrea' },
            { value: 'SZ', text: 'Esuatini' },
            { value: 'ET', text: 'Etiopía' },
            { value: 'GA', text: 'Gabón' },
            { value: 'GM', text: 'Gambia' },
            { value: 'GH', text: 'Ghana' },
            { value: 'GN', text: 'Guinea' },
            { value: 'GW', text: 'Guinea-Bisáu' },
            { value: 'KE', text: 'Kenia' },
            { value: 'LS', text: 'Lesoto' },
            { value: 'LR', text: 'Liberia' },
            { value: 'LY', text: 'Libia' },
            { value: 'MG', text: 'Madagascar' },
            { value: 'MW', text: 'Malaui' },
            { value: 'ML', text: 'Malí' },
            { value: 'MR', text: 'Mauritania' },
            { value: 'MU', text: 'Mauricio' },
            { value: 'MA', text: 'Marruecos' },
            { value: 'MZ', text: 'Mozambique' },
            { value: 'NA', text: 'Namibia' },
            { value: 'NE', text: 'Níger' },
            { value: 'NG', text: 'Nigeria' },
            { value: 'CF', text: 'República Centroafricana' },
            { value: 'RW', text: 'Ruanda' },
            { value: 'ST', text: 'Santo Tomé y Príncipe' },
            { value: 'SN', text: 'Senegal' },
            { value: 'SC', text: 'Seychelles' },
            { value: 'SL', text: 'Sierra Leona' },
            { value: 'SO', text: 'Somalia' },
            { value: 'ZA', text: 'Sudáfrica' },
            { value: 'SS', text: 'Sudán del Sur' },
            { value: 'SD', text: 'Sudán' },
            { value: 'TZ', text: 'Tanzania' },
            { value: 'TG', text: 'Togo' },
            { value: 'TN', text: 'Túnez' },
            { value: 'UG', text: 'Uganda' },
            { value: 'ZM', text: 'Zambia' },
            { value: 'ZW', text: 'Zimbabue' },
            
            // Oceania
            { value: 'AU', text: 'Australia' },
            { value: 'FJ', text: 'Fiyi' },
            { value: 'KI', text: 'Kiribati' },
            { value: 'MH', text: 'Islas Marshall' },
            { value: 'FM', text: 'Micronesia' },
            { value: 'NR', text: 'Nauru' },
            { value: 'NZ', text: 'Nueva Zelanda' },
            { value: 'PW', text: 'Palaos' },
            { value: 'PG', text: 'Papúa Nueva Guinea' },
            { value: 'WS', text: 'Samoa' },
            { value: 'SB', text: 'Islas Salomón' },
            { value: 'TO', text: 'Tonga' },
            { value: 'TV', text: 'Tuvalu' },
            { value: 'VU', text: 'Vanuatu' }
        ],
        
        // ID Type options
        idTypes: {
            'CC': 'Cédula de Ciudadanía',
            'CE': 'Cédula de Extranjería',
            'PA': 'Pasaporte',
            'TI': 'Tarjeta de Identidad'
        },
        
        // Gender options
        genders: {
            'M': 'Masculino',
            'F': 'Femenino',
            'O': 'Otro'
        },
        
        // Marital Status options
        maritalStatuses: {
            'S': 'Soltero(a)',
            'C': 'Casado(a)',
            'U': 'Unión Libre',
            'D': 'Divorciado(a)',
            'V': 'Viudo(a)'
        }
    };

    // Utility functions
    const FormUtils = {
        // Get sorted nationalities for dropdowns
        getSortedNationalities: function() {
            return [...FormConstants.nationalities].sort(function(a, b) {
                return a.text.localeCompare(b.text, 'es');
            });
        },
        
        // Get display text for a value
        getDisplayText: function(type, value) {
            switch(type) {
                case 'nationality':
                    var nation = FormConstants.nationalities.find(function(n) { 
                        return n.value === value; 
                    });
                    return nation ? nation.text : value || 'Sin respuesta';
                    
                case 'idType':
                    return FormConstants.idTypes[value] || value || 'Sin respuesta';
                    
                case 'gender':
                    return FormConstants.genders[value] || value || 'Sin respuesta';
                    
                case 'maritalStatus':
                    return FormConstants.maritalStatuses[value] || value || 'Sin respuesta';
                    
                default:
                    return value || 'Sin respuesta';
            }
        },
        
        // Generate select options HTML
        renderSelectOptions: function(type, selectedValue) {
            selectedValue = selectedValue || '';
            var options = '';
            
            switch(type) {
                case 'nationality':
                    options = '<option value="">Seleccione nacionalidad</option>';
                    var nations = this.getSortedNationalities();
                    nations.forEach(function(n) {
                        var selected = n.value === selectedValue ? 'selected' : '';
                        options += '<option value="' + n.value + '" ' + selected + '>' + n.text + '</option>';
                    });
                    break;
                    
                case 'idType':
                    options = '<option value="">Seleccione tipo de identificación</option>';
                    for (var idKey in FormConstants.idTypes) {
                        if (FormConstants.idTypes.hasOwnProperty(idKey)) {
                            var selected = idKey === selectedValue ? 'selected' : '';
                            options += '<option value="' + idKey + '" ' + selected + '>' + FormConstants.idTypes[idKey] + '</option>';
                        }
                    }
                    break;
                    
                case 'gender':
                    options = '<option value="">Seleccione género</option>';
                    for (var genderKey in FormConstants.genders) {
                        if (FormConstants.genders.hasOwnProperty(genderKey)) {
                            var selected = genderKey === selectedValue ? 'selected' : '';
                            options += '<option value="' + genderKey + '" ' + selected + '>' + FormConstants.genders[genderKey] + '</option>';
                        }
                    }
                    break;
                    
                case 'maritalStatus':
                    options = '<option value="">Seleccione estado civil</option>';
                    for (var statusKey in FormConstants.maritalStatuses) {
                        if (FormConstants.maritalStatuses.hasOwnProperty(statusKey)) {
                            var selected = statusKey === selectedValue ? 'selected' : '';
                            options += '<option value="' + statusKey + '" ' + selected + '>' + FormConstants.maritalStatuses[statusKey] + '</option>';
                        }
                    }
                    break;
            }
            
            return options;
        }
    };

    // Export to window
    window.FormConstants = {
        FormConstants: FormConstants,
        FormUtils: FormUtils
    };

})(window);