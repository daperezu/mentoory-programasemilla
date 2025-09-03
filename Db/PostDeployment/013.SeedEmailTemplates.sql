-- ==========================================================================================
-- Post-Deployment Script for Seeding Email Templates
-- ==========================================================================================
-- Note: These templates contain the actual HTML from the template files
-- The HTML includes inline CSS for proper email rendering
-- ==========================================================================================

-- Helper to escape single quotes in HTML content
DECLARE @WelcomeEmailHTML NVARCHAR(MAX);
DECLARE @WelcomeEmailWithPasswordHTML NVARCHAR(MAX);
DECLARE @WelcomeEmailConfirmRequiredHTML NVARCHAR(MAX);
DECLARE @PasswordResetHTML NVARCHAR(MAX);
DECLARE @FormApprovedHTML NVARCHAR(MAX);
DECLARE @FormRejectedHTML NVARCHAR(MAX);
DECLARE @ProjectInvitationHTML NVARCHAR(MAX);
DECLARE @AccountCreationWithProjectHTML NVARCHAR(MAX);

-- Welcome Email Template (for users who set their own password)
SET @WelcomeEmailHTML = N'<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>¡Bienvenido a {{ApplicationName}}!</title>
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            line-height: 1.6;
            color: #333333;
            margin: 0;
            padding: 0;
            background-color: #f8f9fa;
        }
        .container {
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }
        .header {
            background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
            color: white;
            padding: 40px 30px;
            text-align: center;
        }
        .logo {
            max-width: 120px;
            height: auto;
            margin-bottom: 20px;
        }
        .content {
            padding: 40px 30px;
        }
        .welcome-badge {
            background-color: #28a745;
            color: white;
            padding: 8px 16px;
            border-radius: 20px;
            font-size: 14px;
            font-weight: 600;
            display: inline-block;
            margin-bottom: 20px;
        }
        .info-box {
            background: linear-gradient(135deg, #e7f1ff 0%, #cfe2ff 100%);
            border: 2px solid #007bff;
            border-radius: 8px;
            padding: 25px;
            margin: 25px 0;
            text-align: center;
        }
        .cta-button {
            display: inline-block;
            background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
            color: white;
            text-decoration: none;
            padding: 18px 36px;
            border-radius: 6px;
            font-weight: 700;
            margin: 20px 0;
            text-align: center;
            box-shadow: 0 3px 6px rgba(0, 123, 255, 0.4);
            font-size: 16px;
        }
        .footer {
            background-color: #f8f9fa;
            padding: 30px;
            text-align: center;
            border-top: 1px solid #dee2e6;
        }
        .contact-info {
            font-size: 12px;
            color: #6c757d;
            margin-top: 20px;
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <img src="{{LogoUrl}}" alt="{{ApplicationName}}" class="logo">
            <h1 style="margin: 0; font-size: 32px; font-weight: 300;">🎉 ¡Bienvenido a {{ApplicationName}}!</h1>
            <p style="margin: 10px 0 0 0; opacity: 0.9; font-size: 18px;">Tu cuenta ha sido creada exitosamente</p>
        </div>
        
        <div class="content">
            <div class="welcome-badge">✨ Nuevo Usuario</div>
            
            <h2 style="color: #007bff; margin-bottom: 20px;">¡Hola {{FullName}}!</h2>
            
            <p>Nos complace darte la bienvenida a <strong>{{ApplicationName}}</strong>. Tu cuenta ha sido creada exitosamente y está lista para usar.</p>
            
            <div class="info-box">
                <h3 style="margin-top: 0; color: #0056b3;">🔑 Tu identificación de acceso</h3>
                <p style="font-size: 18px; font-weight: 600; color: #007bff; margin: 10px 0;">{{Email}}</p>
                <p style="margin: 15px 0 0 0; font-size: 14px; color: #6c757d;">
                    Utiliza esta identificación junto con la contraseña que estableciste para acceder a tu cuenta.
                </p>
            </div>
            
            <p style="text-align: center; margin: 30px 0;">
                <a href="{{LoginUrl}}" class="cta-button">🚀 INICIAR SESIÓN AHORA</a>
            </p>
            
            <p style="margin-top: 30px;">
                ¡Esperamos que disfrutes tu experiencia en {{ApplicationName}}!<br><br>
                Atentamente,<br>
                <strong>El equipo de {{ApplicationName}}</strong>
            </p>
        </div>
        
        <div class="footer">
            <div class="contact-info">
                © {{CurrentYear}} {{ApplicationName}}. Todos los derechos reservados.<br>
                ¿Necesitas ayuda? Escríbenos a: <a href="mailto:{{SupportEmail}}" style="color: #007bff;">{{SupportEmail}}</a>
            </div>
        </div>
    </div>
</body>
</html>';

-- Welcome Email Template with Temporary Password
SET @WelcomeEmailWithPasswordHTML = N'<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>¡Bienvenido a {{ApplicationName}}!</title>
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            line-height: 1.6;
            color: #333333;
            margin: 0;
            padding: 0;
            background-color: #f8f9fa;
        }
        .container {
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }
        .header {
            background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
            color: white;
            padding: 40px 30px;
            text-align: center;
        }
        .logo {
            max-width: 120px;
            height: auto;
            margin-bottom: 20px;
        }
        .content {
            padding: 40px 30px;
        }
        .welcome-badge {
            background-color: #28a745;
            color: white;
            padding: 8px 16px;
            border-radius: 20px;
            font-size: 14px;
            font-weight: 600;
            display: inline-block;
            margin-bottom: 20px;
        }
        .credentials-box {
            background: linear-gradient(135deg, #e7f1ff 0%, #cfe2ff 100%);
            border: 2px solid #007bff;
            border-radius: 8px;
            padding: 25px;
            margin: 25px 0;
            text-align: center;
        }
        .credential-item {
            margin: 15px 0;
        }
        .credential-label {
            font-size: 14px;
            color: #6c757d;
            margin-bottom: 5px;
        }
        .credential-value {
            font-size: 18px;
            font-weight: 600;
            color: #007bff;
            background-color: white;
            padding: 10px;
            border-radius: 4px;
            font-family: monospace;
        }
        .cta-button {
            display: inline-block;
            background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
            color: white;
            text-decoration: none;
            padding: 18px 36px;
            border-radius: 6px;
            font-weight: 700;
            margin: 20px 0;
            text-align: center;
            box-shadow: 0 3px 6px rgba(0, 123, 255, 0.4);
            font-size: 16px;
        }
        .footer {
            background-color: #f8f9fa;
            padding: 30px;
            text-align: center;
            border-top: 1px solid #dee2e6;
        }
        .contact-info {
            font-size: 12px;
            color: #6c757d;
            margin-top: 20px;
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <img src="{{LogoUrl}}" alt="{{ApplicationName}}" class="logo">
            <h1 style="margin: 0; font-size: 32px; font-weight: 300;">🎉 ¡Bienvenido a {{ApplicationName}}!</h1>
            <p style="margin: 10px 0 0 0; opacity: 0.9; font-size: 18px;">Tu cuenta ha sido creada exitosamente</p>
        </div>
        
        <div class="content">
            <div class="welcome-badge">✨ Nuevo Usuario</div>
            
            <h2 style="color: #007bff; margin-bottom: 20px;">¡Hola {{FullName}}!</h2>
            
            <p>Nos complace darte la bienvenida a <strong>{{ApplicationName}}</strong>. Tu cuenta ha sido creada y está lista para usar.</p>
            
            <div class="credentials-box">
                <h3 style="margin-top: 0; color: #0056b3;">🔐 Tus Credenciales de Acceso</h3>
                <div class="credential-item">
                    <div class="credential-label">Identificación:</div>
                    <div class="credential-value">{{Email}}</div>
                </div>
                <div class="credential-item">
                    <div class="credential-label">Contraseña temporal:</div>
                    <div class="credential-value">{{TemporaryPassword}}</div>
                </div>
                <p style="margin-top: 20px; color: #dc3545; font-weight: 600;">
                    ⚠️ Por seguridad, deberás cambiar tu contraseña temporal en tu primer inicio de sesión
                </p>
            </div>
            
            <p style="text-align: center; margin: 30px 0;">
                <a href="{{LoginUrl}}" class="cta-button">🚀 INICIAR SESIÓN AHORA</a>
            </p>
            
            <p style="margin-top: 30px;">
                ¡Esperamos que disfrutes tu experiencia en {{ApplicationName}}!<br><br>
                Atentamente,<br>
                <strong>El equipo de {{ApplicationName}}</strong>
            </p>
        </div>
        
        <div class="footer">
            <div class="contact-info">
                © {{CurrentYear}} {{ApplicationName}}. Todos los derechos reservados.<br>
                ¿Necesitas ayuda? Escríbenos a: <a href="mailto:{{SupportEmail}}" style="color: #007bff;">{{SupportEmail}}</a>
            </div>
        </div>
    </div>
</body>
</html>';

-- Seed welcome-email template (for users who set their own password)
IF NOT EXISTS (SELECT 1 FROM [notification].[EmailTemplates] WHERE [Key] = 'welcome-email')
BEGIN
    INSERT INTO [notification].[EmailTemplates] ([Key], [Name], [Subject], [BodyHtml], [Language], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES (
        'welcome-email',
        'Correo de Bienvenida',
        '¡Bienvenido a {{ApplicationName}}!',
        @WelcomeEmailHTML,
        'es',
        1,
        GETDATE(),
        GETDATE()
    );
END

-- Seed welcome-email-with-password template (for users with temporary password)
IF NOT EXISTS (SELECT 1 FROM [notification].[EmailTemplates] WHERE [Key] = 'welcome-email-with-password')
BEGIN
    INSERT INTO [notification].[EmailTemplates] ([Key], [Name], [Subject], [BodyHtml], [Language], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES (
        'welcome-email-with-password',
        'Correo de Bienvenida con Contraseña Temporal',
        '¡Bienvenido a {{ApplicationName}}! - Credenciales de Acceso',
        @WelcomeEmailWithPasswordHTML,
        'es',
        1,
        GETDATE(),
        GETDATE()
    );
END

-- Welcome Email Template with Email Confirmation Required
SET @WelcomeEmailConfirmRequiredHTML = N'<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>¡Bienvenido a {{ApplicationName}}! - Confirma tu cuenta</title>
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            line-height: 1.6;
            color: #333333;
            margin: 0;
            padding: 0;
            background-color: #f8f9fa;
        }
        .container {
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }
        .header {
            background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
            color: white;
            padding: 40px 30px;
            text-align: center;
        }
        .content {
            padding: 40px 30px;
        }
        .warning-box {
            background: #fff3cd;
            border: 2px solid #ffc107;
            border-radius: 8px;
            padding: 20px;
            margin: 25px 0;
            color: #856404;
        }
        .btn-primary {
            display: inline-block;
            padding: 16px 40px;
            background: linear-gradient(135deg, #28a745 0%, #218838 100%);
            color: white;
            text-decoration: none;
            border-radius: 50px;
            font-weight: 600;
            font-size: 18px;
            margin: 20px auto;
            box-shadow: 0 4px 15px rgba(40, 167, 69, 0.3);
            text-align: center;
        }
        .steps-box {
            background: #f8f9fa;
            padding: 20px;
            border-radius: 8px;
            margin: 20px 0;
        }
        .credentials-box {
            background: #e7f3ff;
            border: 2px solid #b3d7ff;
            border-radius: 8px;
            padding: 20px;
            margin: 25px 0;
        }
        .footer {
            background: #f8f9fa;
            padding: 30px;
            text-align: center;
            color: #6c757d;
            font-size: 14px;
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>¡Bienvenido a {{ApplicationName}}! 🎉</h1>
            <p style="margin: 10px 0 0 0; font-size: 18px;">Tu cuenta está casi lista</p>
        </div>
        
        <div class="content">
            <p style="font-size: 18px;">Hola <strong>{{FullName}}</strong>,</p>
            
            <div class="warning-box">
                <h3 style="margin-top: 0;">⚠️ ACCIÓN REQUERIDA: Confirma tu correo electrónico</h3>
                <p>Tu cuenta ha sido creada exitosamente, pero <strong>debes confirmar tu dirección de correo electrónico antes de poder iniciar sesión</strong>.</p>
            </div>

            <div style="text-align: center; margin: 40px 0;">
                <h2 style="color: #28a745;">Paso 1: Confirma tu cuenta</h2>
                <p>Haz clic en el siguiente botón para activar tu cuenta:</p>
                <a href="{{ConfirmationUrl}}" class="btn-primary">✅ Confirmar mi cuenta ahora</a>
            </div>

            <div class="steps-box">
                <h3 style="margin-top: 0;">📋 ¿Qué sucederá después?</h3>
                <ol>
                    <li><strong>Confirmas tu correo</strong> haciendo clic en el botón verde</li>
                    <li><strong>Tu cuenta se activa</strong> inmediatamente</li>
                    <li><strong>Serás redirigido</strong> a la página de inicio de sesión</li>
                    <li><strong>Podrás acceder</strong> con las credenciales que te proporcionamos</li>
                </ol>
            </div>

            <div class="credentials-box">
                <h3 style="margin-top: 0; color: #0056b3;">🔑 Tus credenciales de acceso</h3>
                <p style="color: #6c757d;">Una vez confirmada tu cuenta, podrás iniciar sesión con:</p>
                <div style="margin: 15px 0;">
                    <strong>Identificación:</strong> <span style="font-family: monospace; background: white; padding: 5px 10px; border-radius: 4px;">{{Email}}</span>
                </div>
                <div style="margin: 15px 0;">
                    <strong>Contraseña temporal:</strong> <span style="font-family: monospace; background: white; padding: 5px 10px; border-radius: 4px;">{{TemporaryPassword}}</span>
                </div>
                <p style="color: #dc3545; font-weight: 600; margin-top: 15px;">
                    ⚠️ Por seguridad, deberás cambiar esta contraseña temporal en tu primer inicio de sesión
                </p>
            </div>

            <div style="background: #e8f5e9; padding: 20px; border-radius: 8px; margin: 25px 0;">
                <h4 style="margin-top: 0; color: #2e7d32;">💡 Consejo importante</h4>
                <p>Guarda este correo hasta que hayas confirmado tu cuenta y cambiado tu contraseña. Contiene información importante para tu primer acceso.</p>
            </div>

            <p><strong>¿Necesitas ayuda?</strong><br>
            Si tienes problemas para confirmar tu cuenta o acceder al sistema, contáctanos en <a href="mailto:{{SupportEmail}}">{{SupportEmail}}</a></p>
        </div>
        
        <div class="footer">
            <p>© {{CurrentYear}} {{ApplicationName}}. Todos los derechos reservados.</p>
            <p>Si no solicitaste esta cuenta, puedes ignorar este correo de forma segura.</p>
        </div>
    </div>
</body>
</html>';

-- Seed welcome-email-confirm-required template
IF NOT EXISTS (SELECT 1 FROM [notification].[EmailTemplates] WHERE [Key] = 'welcome-email-confirm-required')
BEGIN
    INSERT INTO [notification].[EmailTemplates] ([Key], [Name], [Subject], [BodyHtml], [Language], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES (
        'welcome-email-confirm-required',
        'Correo de Bienvenida - Confirmación Requerida',
        '🚀 ¡Bienvenido a {{ApplicationName}}! - Confirma tu cuenta',
        @WelcomeEmailConfirmRequiredHTML,
        'es',
        1,
        GETDATE(),
        GETDATE()
    );
END

-- Password Reset Template
SET @PasswordResetHTML = N'<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Restablecer Contraseña - {{ApplicationName}}</title>
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            line-height: 1.6;
            color: #333333;
            margin: 0;
            padding: 0;
            background-color: #f8f9fa;
        }
        .container {
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }
        .header {
            background: linear-gradient(135deg, #dc3545 0%, #c82333 100%);
            color: white;
            padding: 40px 30px;
            text-align: center;
        }
        .logo {
            max-width: 120px;
            height: auto;
            margin-bottom: 20px;
        }
        .content {
            padding: 40px 30px;
        }
        .reset-badge {
            background-color: #ffc107;
            color: #333;
            padding: 8px 16px;
            border-radius: 20px;
            font-size: 14px;
            font-weight: 600;
            display: inline-block;
            margin-bottom: 20px;
        }
        .request-info {
            background-color: #f8d7da;
            border: 1px solid #f5c6cb;
            border-radius: 8px;
            padding: 20px;
            margin: 20px 0;
        }
        .cta-button {
            display: inline-block;
            background: linear-gradient(135deg, #dc3545 0%, #c82333 100%);
            color: white;
            text-decoration: none;
            padding: 18px 36px;
            border-radius: 6px;
            font-weight: 700;
            margin: 20px 0;
            text-align: center;
            box-shadow: 0 3px 6px rgba(220, 53, 69, 0.4);
            font-size: 16px;
        }
        .expiration-warning {
            background-color: #fff3cd;
            border: 2px solid #ffc107;
            border-radius: 6px;
            padding: 20px;
            margin: 25px 0;
            text-align: center;
        }
        .footer {
            background-color: #f8f9fa;
            padding: 30px;
            text-align: center;
            border-top: 1px solid #dee2e6;
        }
        .contact-info {
            font-size: 12px;
            color: #6c757d;
            margin-top: 20px;
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <img src="{{LogoUrl}}" alt="{{ApplicationName}}" class="logo">
            <h1 style="margin: 0; font-size: 28px; font-weight: 300;">🔑 Restablecer tu Contraseña</h1>
            <p style="margin: 10px 0 0 0; opacity: 0.9;">Recupera el acceso a tu cuenta</p>
        </div>
        
        <div class="content">
            <div class="reset-badge">⚡ Acción Requerida</div>
            
            <h2 style="color: #dc3545; margin-bottom: 20px;">Hola {{FullName}},</h2>
            
            <p>Hemos recibido una solicitud para restablecer la contraseña de tu cuenta en <strong>{{ApplicationName}}</strong>.</p>
            
            <div class="request-info">
                <p style="margin: 0;">
                    <strong>📅 Fecha y hora de la solicitud:</strong><br>
                    {{RequestDateTime}}<br><br>
                    <strong>📍 Solicitado desde:</strong><br>
                    {{RequestLocation}}<br><br>
                    <strong>📧 Cuenta:</strong><br>
                    {{Email}}
                </p>
            </div>
            
            <p style="text-align: center; margin: 30px 0;">
                <strong>Para crear una nueva contraseña, haz clic en el siguiente botón:</strong><br>
                <a href="{{ResetLink}}" class="cta-button">🔐 RESTABLECER CONTRASEÑA</a>
            </p>
            
            <div class="expiration-warning">
                <p style="margin: 0;">
                    <strong>⏱️ ¡IMPORTANTE!</strong><br>
                    Este enlace expira en: <strong style="color: #dc3545; font-size: 24px;">2 HORAS</strong><br>
                    <small>Después de este tiempo, deberás solicitar un nuevo enlace.</small>
                </p>
            </div>
            
            <p><strong>¿No solicitaste restablecer tu contraseña?</strong><br>
            Si no realizaste esta solicitud, puedes ignorar este correo de forma segura.</p>
            
            <p style="margin-top: 30px;">
                Saludos,<br>
                <strong>El equipo de seguridad de {{ApplicationName}}</strong>
            </p>
        </div>
        
        <div class="footer">
            <div class="contact-info">
                © {{CurrentYear}} {{ApplicationName}}. Todos los derechos reservados.<br>
                Soporte: <a href="mailto:{{SupportEmail}}" style="color: #007bff;">{{SupportEmail}}</a>
            </div>
        </div>
    </div>
</body>
</html>';

-- Seed password-reset template
IF NOT EXISTS (SELECT 1 FROM [notification].[EmailTemplates] WHERE [Key] = 'password-reset')
BEGIN
    INSERT INTO [notification].[EmailTemplates] ([Key], [Name], [Subject], [BodyHtml], [Language], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES (

        'password-reset',
        'Restablecer Contraseña',
        'Solicitud de restablecimiento de contraseña - {{ApplicationName}}',
        @PasswordResetHTML,
        'es',
        1,
        GETDATE(),
        GETDATE()
    );
END

-- Form Approved Template
SET @FormApprovedHTML = N'<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Formulario Aprobado - {{ApplicationName}}</title>
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            line-height: 1.6;
            color: #333333;
            margin: 0;
            padding: 0;
            background-color: #f8f9fa;
        }
        .container {
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }
        .header {
            background: linear-gradient(135deg, #28a745 0%, #20c997 100%);
            color: white;
            padding: 40px 30px;
            text-align: center;
        }
        .logo {
            max-width: 120px;
            height: auto;
            margin-bottom: 20px;
        }
        .content {
            padding: 40px 30px;
        }
        .approval-badge {
            background-color: #28a745;
            color: white;
            padding: 10px 20px;
            border-radius: 25px;
            font-size: 16px;
            font-weight: 700;
            display: inline-block;
            margin-bottom: 20px;
            box-shadow: 0 2px 4px rgba(40, 167, 69, 0.3);
        }
        .celebration-box {
            background: linear-gradient(135deg, #d4edda 0%, #c3e6cb 100%);
            border: 2px solid #28a745;
            border-radius: 8px;
            padding: 25px;
            margin: 25px 0;
            text-align: center;
        }
        .project-name {
            font-size: 22px;
            font-weight: 700;
            color: #28a745;
            margin: 10px 0;
        }
        .cta-button {
            display: inline-block;
            background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
            color: white;
            text-decoration: none;
            padding: 16px 32px;
            border-radius: 6px;
            font-weight: 600;
            margin: 20px 0;
            text-align: center;
            box-shadow: 0 2px 4px rgba(0, 123, 255, 0.3);
        }
        .footer {
            background-color: #f8f9fa;
            padding: 30px;
            text-align: center;
            border-top: 1px solid #dee2e6;
        }
        .contact-info {
            font-size: 12px;
            color: #6c757d;
            margin-top: 20px;
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <img src="{{LogoUrl}}" alt="{{ApplicationName}}" class="logo">
            <h1 style="margin: 0; font-size: 32px; font-weight: 300;">¡Felicitaciones!</h1>
            <p style="margin: 10px 0 0 0; opacity: 0.9; font-size: 18px;">Tu formulario ha sido aprobado</p>
        </div>
        
        <div class="content">
            <div class="approval-badge">🎉 APROBADO</div>
            
            <h2 style="color: #28a745; margin-bottom: 20px;">Hola {{ParticipantName}},</h2>
            
            <p style="font-size: 18px;">¡Excelentes noticias! Tu formulario ha sido revisado y <strong>aprobado exitosamente</strong>.</p>
            
            <div class="celebration-box">
                <p style="margin: 0; color: #155724; font-size: 16px;">Tu participación en el proyecto</p>
                <div class="project-name">{{ProjectName}}</div>
                <p style="margin: 10px 0 0 0; color: #155724; font-size: 16px;">ha sido confirmada</p>
            </div>
            
            <p>Como participante aprobado, ahora tienes acceso al dashboard del proyecto y todas sus herramientas.</p>
            
            <p style="text-align: center; margin: 30px 0;">
                <a href="{{ProjectDashboardUrl}}" class="cta-button">🏁 Acceder al Dashboard del Proyecto</a>
            </p>
            
            <p style="margin-top: 30px;">
                ¡Bienvenido oficialmente al proyecto!<br><br>
                <strong>El equipo de {{ApplicationName}}</strong>
            </p>
        </div>
        
        <div class="footer">
            <div class="contact-info">
                © {{CurrentYear}} {{ApplicationName}}. Todos los derechos reservados.<br>
                Si tienes preguntas: <a href="mailto:{{SupportEmail}}" style="color: #007bff;">{{SupportEmail}}</a>
            </div>
        </div>
    </div>
</body>
</html>';

-- Seed form-approved template
IF NOT EXISTS (SELECT 1 FROM [notification].[EmailTemplates] WHERE [Key] = 'form-approved')
BEGIN
    INSERT INTO [notification].[EmailTemplates] ([Key], [Name], [Subject], [BodyHtml], [Language], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES (

        'form-approved',
        'Formulario Aprobado',
        '¡Felicitaciones! Tu formulario ha sido aprobado',
        @FormApprovedHTML,
        'es',
        1,
        GETDATE(),
        GETDATE()
    );
END

-- Form Rejected Template
SET @FormRejectedHTML = N'<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Formulario Requiere Correcciones - {{ApplicationName}}</title>
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            line-height: 1.6;
            color: #333333;
            margin: 0;
            padding: 0;
            background-color: #f8f9fa;
        }
        .container {
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }
        .header {
            background: linear-gradient(135deg, #ffc107 0%, #ff9800 100%);
            color: white;
            padding: 40px 30px;
            text-align: center;
        }
        .logo {
            max-width: 120px;
            height: auto;
            margin-bottom: 20px;
        }
        .content {
            padding: 40px 30px;
        }
        .revision-badge {
            background-color: #ffc107;
            color: #856404;
            padding: 8px 16px;
            border-radius: 20px;
            font-size: 14px;
            font-weight: 600;
            display: inline-block;
            margin-bottom: 20px;
        }
        .reason-box {
            background-color: #fff3cd;
            border: 1px solid #ffeaa7;
            border-radius: 6px;
            padding: 20px;
            margin: 20px 0;
        }
        .project-name {
            font-size: 20px;
            font-weight: 700;
            color: #ff9800;
            margin: 5px 0;
        }
        .cta-button {
            display: inline-block;
            background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
            color: white;
            text-decoration: none;
            padding: 16px 32px;
            border-radius: 6px;
            font-weight: 600;
            margin: 20px 0;
            text-align: center;
            box-shadow: 0 2px 4px rgba(0, 123, 255, 0.3);
        }
        .footer {
            background-color: #f8f9fa;
            padding: 30px;
            text-align: center;
            border-top: 1px solid #dee2e6;
        }
        .contact-info {
            font-size: 12px;
            color: #6c757d;
            margin-top: 20px;
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <img src="{{LogoUrl}}" alt="{{ApplicationName}}" class="logo">
            <h1 style="margin: 0; font-size: 28px; font-weight: 300;">Tu formulario necesita ajustes</h1>
            <p style="margin: 10px 0 0 0; opacity: 0.9;">Requiere algunas correcciones antes de ser aprobado</p>
        </div>
        
        <div class="content">
            <div class="revision-badge">⚠️ Requiere Correcciones</div>
            
            <h2 style="color: #ff9800; margin-bottom: 20px;">Hola {{ParticipantName}},</h2>
            
            <p>Después de revisar tu formulario, hemos identificado algunos puntos que necesitan ser corregidos.</p>
            
            <div class="reason-box">
                <p style="margin: 0 0 10px 0; color: #856404; font-size: 14px;"><strong>Proyecto:</strong></p>
                <div class="project-name">{{ProjectName}}</div>
                <p style="margin: 15px 0 10px 0; color: #856404; font-size: 14px;"><strong>Motivo de la revisión:</strong></p>
                <p style="margin: 0; color: #856404; font-style: italic;">{{RejectionReason}}</p>
            </div>
            
            <p><strong>No te preocupes</strong>, esto es parte normal del proceso. Solo necesitas hacer las correcciones indicadas.</p>
            
            <p style="text-align: center; margin: 30px 0;">
                <a href="{{FormEditUrl}}" class="cta-button">✏️ Corregir y Reenviar Formulario</a>
            </p>
            
            <p style="margin-top: 30px;">
                Estamos aquí para ayudarte.<br><br>
                <strong>El equipo de {{ApplicationName}}</strong>
            </p>
        </div>
        
        <div class="footer">
            <div class="contact-info">
                © {{CurrentYear}} {{ApplicationName}}. Todos los derechos reservados.<br>
                Si tienes preguntas: <a href="mailto:{{SupportEmail}}" style="color: #007bff;">{{SupportEmail}}</a>
            </div>
        </div>
    </div>
</body>
</html>';

-- Seed form-rejected template
IF NOT EXISTS (SELECT 1 FROM [notification].[EmailTemplates] WHERE [Key] = 'form-rejected')
BEGIN
    INSERT INTO [notification].[EmailTemplates] ([Key], [Name], [Subject], [BodyHtml], [Language], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES (

        'form-rejected',
        'Formulario Requiere Correcciones',
        'Tu formulario requiere correcciones',
        @FormRejectedHTML,
        'es',
        1,
        GETDATE(),
        GETDATE()
    );
END

-- Additional templates
-- For brevity, using simplified versions. In production, these would contain full HTML with styles like the templates above.

-- Project Invitation Template
IF NOT EXISTS (SELECT 1 FROM [notification].[EmailTemplates] WHERE [Key] = 'project-invitation')
BEGIN
    INSERT INTO [notification].[EmailTemplates] ([Key], [Name], [Subject], [BodyHtml], [Language], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES (

        'project-invitation',
        'Invitación a Proyecto',
        'Has sido invitado al proyecto {{ProjectName}}',
        N'<!DOCTYPE html><html lang="es"><head><meta charset="utf-8"><title>Invitación a Proyecto</title></head><body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;"><div style="max-width: 600px; margin: 0 auto; padding: 20px;"><h1>¡Hola {{FullName}}!</h1><p>Has sido invitado a participar en el proyecto <strong>{{ProjectName}}</strong>.</p><p style="text-align: center;"><a href="{{InvitationLink}}" style="background-color: #007bff; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;">Aceptar Invitación</a></p><p>Esta invitación expirará en 72 horas.</p></div></body></html>',
        'es',
        1,
        GETDATE(),
        GETDATE()
    );
END

-- Invitation Reminder Template
IF NOT EXISTS (SELECT 1 FROM [notification].[EmailTemplates] WHERE [Key] = 'invitation-reminder')
BEGIN
    INSERT INTO [notification].[EmailTemplates] ([Key], [Name], [Subject], [BodyHtml], [Language], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES (

        'invitation-reminder',
        'Recordatorio de Invitación',
        'Recordatorio: Tu invitación al proyecto {{ProjectName}} expira pronto',
        N'<!DOCTYPE html><html lang="es"><head><meta charset="utf-8"><title>Recordatorio de Invitación</title></head><body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;"><div style="max-width: 600px; margin: 0 auto; padding: 20px;"><h1>Hola {{FullName}},</h1><p>Te recordamos que tienes una invitación pendiente para el proyecto <strong>{{ProjectName}}</strong>.</p><p>Tu invitación expirará en <strong>{{DaysRemaining}} días</strong>.</p><p style="text-align: center;"><a href="{{InvitationLink}}" style="background-color: #ffc107; color: #333; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;">Aceptar Invitación Ahora</a></p></div></body></html>',
        'es',
        1,
        GETDATE(),
        GETDATE()
    );
END

-- Form Submission Confirmation Template
IF NOT EXISTS (SELECT 1 FROM [notification].[EmailTemplates] WHERE [Key] = 'form-submission-confirmation')
BEGIN
    INSERT INTO [notification].[EmailTemplates] ([Key], [Name], [Subject], [BodyHtml], [Language], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES (

        'form-submission-confirmation',
        'Confirmación de Envío de Formulario',
        'Confirmación: Formulario recibido - {{ProjectName}}',
        N'<!DOCTYPE html><html lang="es"><head><meta charset="utf-8"><title>Confirmación de Envío</title></head><body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;"><div style="max-width: 600px; margin: 0 auto; padding: 20px;"><h1>Hola {{ParticipantName}},</h1><p>Hemos recibido exitosamente tu formulario para el proyecto <strong>{{ProjectName}}</strong>.</p><div style="background-color: #d4edda; padding: 15px; border-radius: 5px; margin: 20px 0;"><p><strong>Detalles del envío:</strong></p><p>ID de envío: {{SubmissionId}}<br>Fecha y hora: {{SubmissionDateTime}}</p></div><p>Tu formulario será revisado pronto y te notificaremos el resultado.</p></div></body></html>',
        'es',
        1,
        GETDATE(),
        GETDATE()
    );
END

-- Form Submission Admin Notification Template
IF NOT EXISTS (SELECT 1 FROM [notification].[EmailTemplates] WHERE [Key] = 'form-submission-admin-notification')
BEGIN
    INSERT INTO [notification].[EmailTemplates] ([Key], [Name], [Subject], [BodyHtml], [Language], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES (

        'form-submission-admin-notification',
        'Notificación de Envío para Administrador',
        'Nuevo formulario recibido - {{ProjectName}}',
        N'<!DOCTYPE html><html lang="es"><head><meta charset="utf-8"><title>Nuevo Formulario Recibido</title></head><body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;"><div style="max-width: 600px; margin: 0 auto; padding: 20px;"><h1>Hola {{ReviewerName}},</h1><p>Se ha recibido un nuevo formulario para revisión.</p><div style="background-color: #e8f4fd; padding: 15px; border-radius: 5px; margin: 20px 0;"><p><strong>Detalles:</strong></p><p>Proyecto: {{ProjectName}}<br>Participante: {{ParticipantName}} ({{ParticipantEmail}})<br>Fecha: {{SubmissionDateTime}}<br>ID: {{SubmissionId}}</p></div><div style="background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 20px 0;"><p><strong>Estadísticas:</strong></p><p>Pendientes: {{PendingCount}}<br>Revisados hoy: {{ReviewedToday}}<br>Total: {{TotalSubmissions}}<br>Tiempo promedio: {{AverageReviewTime}} horas</p></div><p style="text-align: center;"><a href="{{ReviewDashboardUrl}}" style="background-color: #007bff; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;">Revisar Formulario</a></p></div></body></html>',
        'es',
        1,
        GETDATE(),
        GETDATE()
    );
END

-- Account Creation with Project Template
IF NOT EXISTS (SELECT 1 FROM [notification].[EmailTemplates] WHERE [Key] = 'account-creation-with-project')
BEGIN
    INSERT INTO [notification].[EmailTemplates] ([Key], [Name], [Subject], [BodyHtml], [Language], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES (

        'account-creation-with-project',
        'Cuenta Creada con Invitación a Proyecto',
        'Bienvenido a {{ApplicationName}} - Invitación al proyecto {{ProjectName}}',
        N'<!DOCTYPE html><html lang="es"><head><meta charset="utf-8"><title>Cuenta Creada e Invitación</title></head><body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;"><div style="max-width: 600px; margin: 0 auto; padding: 20px;"><h1>¡Bienvenido {{FullName}}!</h1><p>Se ha creado una cuenta para ti en {{ApplicationName}} y has sido invitado al proyecto <strong>{{ProjectName}}</strong>.</p><div style="background-color: #e7f1ff; padding: 20px; border-radius: 5px; margin: 20px 0;"><p><strong>Credenciales de acceso:</strong></p><p>Email: {{Email}}<br>Contraseña temporal: {{TemporaryPassword}}</p></div><p style="text-align: center;"><a href="{{ActivationLink}}" style="background-color: #28a745; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;">Activar Cuenta y Aceptar Invitación</a></p></div></body></html>',
        'es',
        1,
        GETDATE(),
        GETDATE()
    );
END

-- Email Change Verification Template
IF NOT EXISTS (SELECT 1 FROM [notification].[EmailTemplates] WHERE [Key] = 'email-change-verification')
BEGIN
    INSERT INTO [notification].[EmailTemplates] ([Key], [Name], [Subject], [BodyHtml], [Language], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES (

        'email-change-verification',
        'Verificación de Cambio de Email',
        'Verifica tu nuevo correo electrónico',
        N'<!DOCTYPE html><html lang="es"><head><meta charset="utf-8"><title>Verificación de Email</title></head><body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;"><div style="max-width: 600px; margin: 0 auto; padding: 20px;"><h1>Hola {{FullName}},</h1><p>Has solicitado cambiar tu correo electrónico de <strong>{{OldEmail}}</strong> a <strong>{{NewEmail}}</strong>.</p><p>Para confirmar este cambio, haz clic en el siguiente enlace:</p><p style="text-align: center;"><a href="{{VerificationLink}}" style="background-color: #007bff; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;">Verificar Nuevo Email</a></p><p>Si no solicitaste este cambio, ignora este mensaje.</p></div></body></html>',
        'es',
        1,
        GETDATE(),
        GETDATE()
    );
END

-- Identification Change Notification Template
IF NOT EXISTS (SELECT 1 FROM [notification].[EmailTemplates] WHERE [Key] = 'identification-change-notification')
BEGIN
    INSERT INTO [notification].[EmailTemplates] ([Key], [Name], [Subject], [BodyHtml], [Language], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES (

        'identification-change-notification',
        'Notificación de Cambio de Identificación',
        'Tu identificación ha sido actualizada',
        N'<!DOCTYPE html><html lang="es"><head><meta charset="utf-8"><title>Cambio de Identificación</title></head><body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;"><div style="max-width: 600px; margin: 0 auto; padding: 20px;"><h1>Hola {{FullName}},</h1><p>Te informamos que tu identificación ha sido actualizada en nuestro sistema.</p><div style="background-color: #f8d7da; padding: 15px; border-radius: 5px; margin: 20px 0;"><p><strong>Detalles del cambio:</strong></p><p>Identificación anterior: {{OldIdentification}}<br>Nueva identificación: {{NewIdentification}}<br>Fecha del cambio: {{ChangeDateTime}}<br>Solicitado por: {{RequestedBy}}</p></div><p>Si no autorizaste este cambio, contacta inmediatamente a <a href="{{SupportUrl}}">soporte</a>.</p></div></body></html>',
        'es',
        1,
        GETDATE(),
        GETDATE()
    );
END

-- Review Request Template
DECLARE @ReviewRequestHTML NVARCHAR(MAX);
SET @ReviewRequestHTML = N'<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Solicitud de Revisión - {{ApplicationName}}</title>
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, ''Segoe UI'', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            line-height: 1.6;
            color: #333333;
            margin: 0;
            padding: 0;
            background-color: #f8f9fa;
        }
        .container {
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }
        .header {
            background: linear-gradient(135deg, #17a2b8 0%, #138496 100%);
            color: white;
            padding: 40px 30px;
            text-align: center;
        }
        .logo {
            max-width: 120px;
            height: auto;
            margin-bottom: 20px;
        }
        .content {
            padding: 40px 30px;
        }
        .review-badge {
            background-color: #17a2b8;
            color: white;
            padding: 8px 16px;
            border-radius: 20px;
            font-size: 14px;
            font-weight: 600;
            display: inline-block;
            margin-bottom: 20px;
        }
        .comments-box {
            background-color: #fff3cd;
            border-left: 4px solid #ffc107;
            padding: 20px;
            margin: 20px 0;
            border-radius: 4px;
        }
        .deadline-alert {
            background-color: #f8d7da;
            border: 2px solid #dc3545;
            border-radius: 6px;
            padding: 20px;
            margin: 25px 0;
            text-align: center;
        }
        .deadline-date {
            font-size: 22px;
            font-weight: 700;
            color: #dc3545;
            margin: 10px 0;
        }
        .cta-button {
            display: inline-block;
            background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
            color: white;
            text-decoration: none;
            padding: 16px 32px;
            border-radius: 6px;
            font-weight: 600;
            margin: 20px 0;
            text-align: center;
            box-shadow: 0 2px 4px rgba(0, 123, 255, 0.3);
        }
        .footer {
            background-color: #f8f9fa;
            padding: 30px;
            text-align: center;
            border-top: 1px solid #dee2e6;
        }
        .contact-info {
            font-size: 12px;
            color: #6c757d;
            margin-top: 20px;
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <img src="{{LogoUrl}}" alt="{{ApplicationName}}" class="logo">
            <h1 style="margin: 0; font-size: 28px; font-weight: 300;">📝 Solicitud de Revisión</h1>
            <p style="margin: 10px 0 0 0; opacity: 0.9;">Tu formulario ha recibido comentarios para revisar</p>
        </div>
        
        <div class="content">
            <div class="review-badge">🔍 Revisión Solicitada</div>
            
            <h2 style="color: #17a2b8; margin-bottom: 20px;">Hola {{ParticipantName}},</h2>
            
            <p>Tu formulario para el proyecto <strong>{{ProjectName}}</strong> ha sido revisado y el evaluador ha solicitado que realices algunos cambios o aclaraciones.</p>
            
            <div class="comments-box">
                <h3 style="margin-top: 0; color: #856404;">💬 Comentarios del Revisor ({{ReviewerName}}):</h3>
                <p style="margin: 10px 0; color: #856404; font-style: italic; line-height: 1.8;">
                    {{ReviewComments}}
                </p>
            </div>
            
            <div class="deadline-alert">
                <p style="margin: 0;">
                    <strong>⏰ FECHA LÍMITE PARA RESPONDER:</strong>
                </p>
                <div class="deadline-date">{{ReviewDeadline}}</div>
                <p style="margin: 0; font-size: 14px; color: #721c24;">
                    Por favor, realiza los cambios antes de esta fecha
                </p>
            </div>
            
            <p style="text-align: center; margin: 30px 0;">
                <strong>Para revisar y actualizar tu formulario, haz clic aquí:</strong><br>
                <a href="{{FormEditUrl}}" class="cta-button">📋 Revisar y Actualizar Formulario</a>
            </p>
            
            <p style="background-color: #d4edda; border: 1px solid #c3e6cb; border-radius: 6px; padding: 15px; margin: 20px 0;">
                <strong>✅ Nota:</strong> Una vez que hayas realizado los cambios solicitados, tu formulario será revisado nuevamente de manera prioritaria.
            </p>
            
            <p>Si tienes alguna pregunta sobre los comentarios recibidos, no dudes en contactarnos.</p>
            
            <p style="margin-top: 30px;">
                Agradecemos tu colaboración.<br><br>
                Saludos cordiales,<br>
                <strong>El equipo de {{ApplicationName}}</strong>
            </p>
        </div>
        
        <div class="footer">
            <div class="contact-info">
                © {{CurrentYear}} {{ApplicationName}}. Todos los derechos reservados.<br>
                ¿Necesitas ayuda? Escríbenos a: <a href="mailto:{{SupportEmail}}" style="color: #007bff;">{{SupportEmail}}</a>
            </div>
        </div>
    </div>
</body>
</html>';

-- Seed review-request template
IF NOT EXISTS (SELECT 1 FROM [notification].[EmailTemplates] WHERE [Key] = 'review-request')
BEGIN
    INSERT INTO [notification].[EmailTemplates] ([Key], [Name], [Subject], [BodyHtml], [Language], [IsActive], [CreatedAt], [UpdatedAt])
    VALUES (

        'review-request',
        'Solicitud de Revisión',
        'Se requiere tu revisión - {{ProjectName}}',
        @ReviewRequestHTML,
        'es',
        1,
        GETDATE(),
        GETDATE()
    );
END