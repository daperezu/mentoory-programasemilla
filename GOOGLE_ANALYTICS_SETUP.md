# Google Analytics Integration Setup

## Overview
Google Analytics has been integrated into the LinaSys web application using Google Analytics 4 (GA4) with the Global Site Tag (gtag.js).

## Configuration

### 1. Setting up Google Analytics

Google Analytics configuration is managed through **Aspire.AppHost** as secret parameters, following the same pattern as Mailgun configuration.

#### For Local Development
Update `Aspire.AppHost/appsettings.Development.json`:

```json
{
  "GoogleAnalytics": {
    "MeasurementId": "G-XXXXXXXXXX",
    "Enabled": "false"
  }
}
```

#### For Production (Azure)
The configuration is managed through Aspire parameters:
- `googleanalytics-measurementid` (secret parameter)
- `googleanalytics-enabled` (regular parameter)

These will be set as environment variables in Azure and passed to the Web application.

**Important:** 
- Replace `G-XXXXXXXXXX` with your actual Google Analytics 4 Measurement ID
- Set `Enabled` to `"false"` (as string) in development to avoid tracking development data
- The Measurement ID is marked as a secret parameter for security

### 2. Getting your Measurement ID

1. Go to [Google Analytics](https://analytics.google.com/)
2. Navigate to Admin (gear icon in bottom left)
3. Under the Property column, click on "Data Streams"
4. Select your web data stream or create a new one
5. Copy the Measurement ID (starts with "G-")

### 3. Environment-Specific Configuration

The Aspire.AppHost automatically manages environment variables:

**Local Development (IsRunMode):**
- Reads from `appsettings.Development.json`
- Parameters are created but values come from local config

**Azure Deployment:**
- Parameters are defined in Aspire configuration
- Set through Azure Container Apps environment variables:
  - `googleanalytics-measurementid`: Your GA4 Measurement ID
  - `googleanalytics-enabled`: "true" or "false"

The Web application receives these as:
- `GoogleAnalytics__MeasurementId`
- `GoogleAnalytics__Enabled`

## Implementation Details

### Components Created

1. **IGoogleAnalyticsService** (`Web/Services/`)
   - Interface defining the contract for Google Analytics functionality
   - Located in the Web project as this is a web-only concern

2. **GoogleAnalyticsService** (`Web/Services/`)
   - Implementation that reads configuration and generates tracking scripts
   - Handles enabled/disabled state
   - Generates gtag.js initialization script
   - Properly scoped to the Web layer following Clean Architecture principles

3. **Configuration in Aspire.AppHost**
   - Added parameters in `Program.cs` for secure configuration management
   - Local config in `appsettings.Development.json`
   - Azure parameters: `googleanalytics-measurementid` (secret) and `googleanalytics-enabled`

4. **Integration in _HeadPartial.cshtml**
   - Conditionally includes Google Analytics scripts when enabled
   - Scripts are placed in the `<head>` section for optimal loading

### How It Works

1. The service checks if Google Analytics is enabled via configuration
2. If enabled and a valid Measurement ID is present, the tracking scripts are included
3. The gtag.js library is loaded asynchronously
4. Basic page view tracking is automatically enabled
5. The service is registered as a singleton in `Program.cs` for performance
6. Following Clean Architecture, the service resides in the Web layer as it's a presentation concern

## Testing

### Verify Installation

1. Set your Measurement ID in `Aspire.AppHost/appsettings.Development.json`
2. Set `Enabled` to `"true"` (as string)
3. Run the application
4. Open browser Developer Tools (F12)
5. Check the Network tab for requests to `www.googletagmanager.com`
6. Check the Console for any Google Analytics errors

### Google Analytics Debug Mode

To enable debug mode for testing, modify the initialization script in `GoogleAnalyticsService.cs`:

```csharp
gtag('config', '{_measurementId}', {{ 'debug_mode': true }});
```

## Custom Events (Future Enhancement)

To track custom events, you can add JavaScript code to your views:

```javascript
// Example: Track button clicks
gtag('event', 'click', {
  'event_category': 'engagement',
  'event_label': 'header navigation'
});

// Example: Track form submissions
gtag('event', 'form_submit', {
  'event_category': 'form',
  'event_label': 'contact'
});
```

## Privacy Considerations

- Ensure compliance with GDPR, CCPA, and other privacy regulations
- Consider implementing a cookie consent banner
- Update your privacy policy to mention Google Analytics usage
- Consider implementing IP anonymization if required:

```javascript
gtag('config', 'G-XXXXXXXXXX', {
  'anonymize_ip': true
});
```

## Troubleshooting

### Analytics Not Showing Up
1. Verify the Measurement ID is correct
2. Check that `Enabled` is set to `true`
3. Wait 24-48 hours for data to appear in Google Analytics
4. Use Google Analytics Realtime view to verify immediate tracking

### Multiple Environments
- Use different Measurement IDs for different environments (dev, staging, production)
- Or disable tracking in non-production environments

## Security Notes

- The Measurement ID is not sensitive information (it's visible in your public HTML)
- However, avoid committing environment-specific IDs to source control
- Use configuration management for different environments

## References

- [Google Analytics 4 Documentation](https://developers.google.com/analytics/devguides/collection/ga4)
- [gtag.js Developer Guide](https://developers.google.com/tag-platform/gtagjs)
- [Google Analytics Debug](https://support.google.com/analytics/answer/7201382)