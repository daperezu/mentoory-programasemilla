using LinaSys.Shared.Domain.Constants;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace LinaSys.BusinessIncubator.Application.Participants.Services;

/// <summary>
/// Service for generating Excel files related to participant management.
/// </summary>
public class ParticipantExcelService
{
    /// <summary>
    /// Generates an Excel template for bulk participant invitations.
    /// </summary>
    /// <returns>The Excel template as a byte array.</returns>
    public static byte[] GenerateInvitationTemplate()
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Participantes");

        // Define headers
        var headers = new[]
        {
            "Email",
            "Nombre",
            "Apellido",
            "Identificación"
        };

        // Style headers
        for (int col = 1; col <= headers.Length; col++)
        {
            var cell = worksheet.Cells[1, col];
            cell.Value = headers[col - 1];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
        }

        // Add example data
        worksheet.Cells[2, 1].Value = "participante@ejemplo.com";
        worksheet.Cells[2, 2].Value = "Juan";
        worksheet.Cells[2, 3].Value = "Pérez";
        worksheet.Cells[2, 4].Value = "1234567890";

        // Add validation notes
        worksheet.Cells[4, 1].Value = "Instrucciones:";
        worksheet.Cells[4, 1].Style.Font.Bold = true;

        worksheet.Cells[5, 1].Value = "• Email: Requerido, debe ser una dirección válida";
        worksheet.Cells[6, 1].Value = "• Nombre: Requerido";
        worksheet.Cells[7, 1].Value = "• Apellido: Requerido";
        worksheet.Cells[8, 1].Value = "• Identificación: Requerido (cédula, pasaporte, etc.)";
        worksheet.Cells[9, 1].Value = "• Todos los participantes serán registrados con rol 'Participante'";

        // Auto-fit columns
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        // Set column widths for better readability
        worksheet.Column(1).Width = 30; // Email
        worksheet.Column(2).Width = 20; // Nombre
        worksheet.Column(3).Width = 20; // Apellido
        worksheet.Column(4).Width = 20; // Identificación

        return package.GetAsByteArray();
    }

    /// <summary>
    /// Exports participant data to Excel.
    /// </summary>
    /// <param name="participants">The participant data to export.</param>
    /// <returns>The Excel file as a byte array.</returns>
    public static byte[] ExportParticipants(IEnumerable<ParticipantExportData> participants)
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Participantes");

        // Define headers
        var headers = new[]
        {
            "Email",
            "Nombre Completo",
            "Rol",
            "Estado",
            "Estado del Formulario",
            "Fecha de Ingreso",
            "Última Actividad"
        };

        // Style headers
        for (int col = 1; col <= headers.Length; col++)
        {
            var cell = worksheet.Cells[1, col];
            cell.Value = headers[col - 1];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
        }

        // Add data
        int row = 2;
        foreach (var participant in participants)
        {
            worksheet.Cells[row, 1].Value = participant.Email;
            worksheet.Cells[row, 2].Value = participant.FullName;
            worksheet.Cells[row, 3].Value = participant.Role;
            worksheet.Cells[row, 4].Value = participant.IsActive ? "Activo" : "Inactivo";
            worksheet.Cells[row, 5].Value = participant.FormStatus;
            worksheet.Cells[row, 6].Value = participant.JoinedAt?.ToString("yyyy-MM-dd");
            worksheet.Cells[row, 7].Value = participant.LastActivity?.ToString("yyyy-MM-dd HH:mm");

            // Apply borders
            for (int col = 1; col <= headers.Length; col++)
            {
                worksheet.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            row++;
        }

        // Auto-fit columns
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        return package.GetAsByteArray();
    }
}

/// <summary>
/// Data transfer object for participant export.
/// </summary>
public class ParticipantExportData
{
    /// <summary>
    /// Gets or sets the participant email.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the participant full name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the participant role.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the participant is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the form submission status.
    /// </summary>
    public string FormStatus { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the join date.
    /// </summary>
    public DateTime? JoinedAt { get; set; }

    /// <summary>
    /// Gets or sets the last activity date.
    /// </summary>
    public DateTime? LastActivity { get; set; }
}
