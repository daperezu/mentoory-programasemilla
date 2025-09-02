using System.Text;
using System.Text.RegularExpressions;
using LinaSys.Web.Models.Shared;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace LinaSys.Web.TagHelpers;

[HtmlTargetElement("datatable")]
public class DatatableTagHelper : TagHelper
{
    [HtmlAttributeName("ajax-url")]
    public string? AjaxUrl { get; set; }

    [HtmlAttributeName("columns")]
    public List<DatatableColumn> Columns { get; set; } = [];

    [HtmlAttributeName("hover-actions")]
    public List<HoverAction>? HoverActions { get; set; }

    [HtmlAttributeName("show-filter-toggle")]
    public bool ShowFilterToggle { get; set; } = true;

    [HtmlAttributeName("table-id")]
    public string TableId { get; set; } = "dataTableId";

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext? ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.Attributes.SetAttribute("class", "mx-n4 px-4 mx-lg-n6 px-lg-6 bg-body-emphasis border-top border-bottom border-translucent position-relative top-1");

        var scriptsBuffer = new StringBuilder();

        var (toggleMarkup, clickOnToggleMarkup, onDocumentReadyCloneHeaders, tableSetupFilterInputs) = BuildFilterScripts();
        scriptsBuffer.Append(clickOnToggleMarkup);

        var ajaxBlock = string.IsNullOrWhiteSpace(AjaxUrl)
            ? string.Empty
            : $@"ajax: {{
                    url: '{AjaxUrl}',
                    type: 'POST'
                }},";

        var columnsJs = BuildColumnsDefinitionScripts();

        var tableSetupHoverActions = BuildHoverActionsScripts();

        scriptsBuffer.Append($@"
            <script>
                $(document).ready(function () {{
                    console.log('Initializing DataTable for #{TableId}');
                    console.log('AJAX URL: {AjaxUrl}');
                    {onDocumentReadyCloneHeaders}

                    let table = $('#{TableId}').DataTable({{
                        layout: {{
                            topStart: null,
                            topEnd: null,
                            bottomEnd: null,
                            bottomStart: null,
                            bottom: {{
                                paging: {{
                                    buttons: 3,
                                    firstLast: false
                                }}
                            }}
                        }},
                        processing: true,
                        serverSide: true,
                        stateSave: true,
                        stateDuration: 60 * 60 * 24 * 7, // State saved for 7 days
                        {ajaxBlock}
                        columns: [
                            {columnsJs}
                        ],
                        orderCellsTop: true,
                        fixedHeader: true,
                        responsive: true,
                        language: {{
                            processing: 'Procesando...',
                            search: 'Buscar:',
                            lengthMenu: 'Mostrar _MENU_ registros',
                            info: 'Mostrando _START_ a _END_ de _TOTAL_ registros',
                            infoEmpty: 'Mostrando 0 a 0 de 0 registros',
                            infoFiltered: '(filtrado de _MAX_ registros totales)',
                            loadingRecords: 'Cargando...',
                            zeroRecords: 'No se encontraron registros',
                            emptyTable: 'No hay datos disponibles',
                            paginate: {{
                                first: 'Primero',
                                previous: 'Anterior',
                                next: 'Siguiente',
                                last: 'Último'
                            }}
                        }},
                        drawCallback: function (settings) {{
                            const api = this.api();
                            const pagination = $(this)
                                .closest('.dt-container')
                                .find('.dt-paging');

                            if (api.page.info().pages <= 1) {{
                                pagination.hide();
                            }} else {{
                                pagination.show();
                            }}
                        }}
                    }});

                    {tableSetupFilterInputs}
                    {tableSetupHoverActions}
                    
                    // Expose table instance globally for SignalR updates
                    window.userDataTable = table;
                    
                    console.log('DataTable initialized successfully for #{TableId}');
                }});
            </script>");

        var thead = "<thead><tr>" + string.Join(string.Empty, Columns.Select(c => $"<th>{c.Header}</th>")) + "</tr></thead>";
        output.Content.SetHtmlContent($@"
            {toggleMarkup}
            <table id='{TableId}' class='table fs-9 mb-0' style='width:100%'>
                {thead}
            </table>");

        PassDataTableScriptsToViewData(scriptsBuffer.ToString());
    }

    /// <summary>
    /// Handles the columns definition for the DataTable.
    /// </summary>
    /// <returns>A string representing the columns definition in JavaScript format.</returns>
    private string BuildColumnsDefinitionScripts()
    {
        return string.Join(",\n", Columns.Select(c =>
        {
            var baseProps = $@"data: '{c.Data}', orderable: {c.Orderable.ToString().ToLower()}, searchable: {c.Searchable.ToString().ToLower()}";
            string? renderJs = null;

            switch (c.RenderType)
            {
                case ColRenderType.Custom:
                    // If RenderJs is provided, use it as a function reference
                    // It should be wrapped to ensure it's available when DataTable initializes
                    renderJs = !string.IsNullOrWhiteSpace(c.RenderJs)
                        ? $"window.{c.RenderJs} || function(data) {{ return data; }}"
                        : "function(data) { return data; }";
                    break;

                case ColRenderType.Link:
                    var url = c.LinkUrl.Replace("%7B", "{").Replace("%7D", "}");

                    var tokenMatches = Regex.Matches(url, @"\{(.*?)\}")
                                        .Select(m => m.Groups[1].Value)
                                        .Distinct()
                                        .ToList();

                    if (tokenMatches.Count > 0)
                    {
                        // Generate chained .replace('{token}', row.token)
                        var replacementChain = string.Join(".", tokenMatches.Select(token => $"replace('{{{token}}}', row.{token})"));

                        var finalUrlExpr = $"`{url}`.{replacementChain}";
                        string linkTextExpr;

                        if (string.IsNullOrWhiteSpace(c.LinkTextField))
                        {
                            // Default to the current column data
                            linkTextExpr = $"data";
                        }
                        else if (Regex.IsMatch(c.LinkTextField, @"\{.*?\}"))
                        {
                            // Replace tokens in link text using row values
                            var textTokens = Regex.Matches(c.LinkTextField, @"\{(.*?)\}")
                                                  .Select(m => m.Groups[1].Value)
                                                  .Distinct()
                                                  .ToList();

                            var textReplacementChain = string.Join(".", textTokens.Select(token => $"replace('{{{token}}}', row.{token})"));

                            linkTextExpr = $"`{c.LinkTextField}`.{textReplacementChain}";
                        }
                        else
                        {
                            // Plain static text
                            linkTextExpr = $"`{c.LinkTextField}`";
                        }

                        renderJs = $@"
                        function(data, type, row) {{
                            const url = {finalUrlExpr};
                            const text = {linkTextExpr};
                            return '<a href=""' + url + '"" class=""fw-semibold text-primary"">' + text + '</a>';
                        }}";
                    }
                    else
                    {
                        // Static link — no token replacement
                        string linkTextExpr;

                        if (string.IsNullOrWhiteSpace(c.LinkTextField))
                        {
                            // Use the column data itself
                            linkTextExpr = $"data.{c.Data}";
                        }
                        else if (Regex.IsMatch(c.LinkTextField, @"\{.*?\}"))
                        {
                            var textTokens = Regex.Matches(c.LinkTextField, @"\{(.*?)\}")
                                .Select(m => m.Groups[1].Value)
                                .Distinct()
                                .ToList();

                            var textReplacementChain = string.Join(".", textTokens.Select(token => $"replace('{{{token}}}', data.{token})"));

                            linkTextExpr = $"`{c.LinkTextField}`.{textReplacementChain}";
                        }
                        else
                        {
                            linkTextExpr = $"`{c.LinkTextField}`";
                        }

                        renderJs = $@"
                            function(data, type, row) {{
                                const url = `{c.LinkUrl}`;
                                const text = {linkTextExpr};
                                return '<a href=""' + url + '"" class=""fw-semibold text-primary"">' + text + '</a>';
                            }}";
                    }

                    break;
                case ColRenderType.Badge:
                    var labelMap = string.Join(", ", c.BadgeMap.Select(kv => $"'{kv.Key}': '{kv.Value.Replace("'", "\\'")}'"));
                    var classMap = c.BadgeClassMap is { Count: > 0 }
                        ? string.Join(", ", c.BadgeClassMap.Select(kv => $"'{kv.Key}': '{kv.Value.Replace("'", "\\'")}'"))
                        : string.Empty;

                    renderJs = $@"
                    function(data) {{
                        const labels = {{{labelMap}}};
                        const classes = {{{classMap}}};
                        const label = labels[data] || data;
                        const badgeClass = classes[data] || 'primary';
                        return '<span class=""badge badge-phoenix badge-phoenix-' + badgeClass + '"">' + label + '</span>';
                    }}";

                    break;
            }

            return !string.IsNullOrWhiteSpace(renderJs)
                ? $"{{ {baseProps}, render: {renderJs} }}"
                : $"{{ {baseProps} }}";
        }));
    }

    /// <summary>
    /// Handles the filter toggle and filter input fields for the datatable.
    /// </summary>
    /// <returns>A tuple containing the toggle markup, click event script, document ready clone headers, and table setup filter inputs.</returns>
    private (string ToggleMarkup, string ClickOnToggleMarkup, string OnDocumentReadyCloneHeaders, string TableSetupFilterInputs) BuildFilterScripts()
    {
        // This method handles the filter toggle and filter input fields.
        var toggleId = $"toggle-filters-{TableId}";
        var toggleMarkup = ShowFilterToggle
            ? $@"
                <div class='d-flex justify-content-end mb-2'>
                    <a href='#' id='{toggleId}' class='btn btn-link btn-sm'>
                        <i class='fas fa-filter me-1'></i> Mostrar filtros
                    </a>
                </div>"
            : string.Empty;

        // This script clones the header row to create a filter row.
        var documentReadyCloneHeaders = ShowFilterToggle
            ? $"$('#{TableId} thead tr').clone(true).css('display', 'none').addClass('filters').appendTo('#{TableId} thead');"
            : string.Empty;

        // This script handles the filter input fields based on the filter type.
        string tableColumnsFilterInput = string.Empty, scriptHandleClickOnToggleMarkup = string.Empty;

        if (ShowFilterToggle)
        {
            var filterPerColumn = new StringBuilder();
            for (var i = 0; i < Columns.Count; i++)
            {
                var col = Columns[i];
                var baseFilter = $@"
                        const cell{i} = $('.filters th').eq({i});
                        $(cell{i}).empty();";

                switch (col.FilterType)
                {
                    case ColFilterType.Default:
                        filterPerColumn.AppendLine($@"
                        {baseFilter}
                        const wrapper{i} = $('<div class=""input-group input-group-sm""></div>');
                        const icon{i} = $('<div class=""input-group-text""><i class=""fas fa-search""></i></div>');
                        const input{i} = $('<input type=""text"" placeholder=""Buscar..."" class=""form-control"" />');
                        input{i}.on('keyup change', function () {{
                            table.column({i}).search(this.value).draw();
                        }});
                        wrapper{i}.append(icon{i}).append(input{i}).appendTo(cell{i});");
                        break;

                    case ColFilterType.Custom:
                        filterPerColumn.AppendLine($"{baseFilter}{col.FilterJs}");
                        break;

                    case ColFilterType.Select:
                        var options = string.Join(string.Empty, col.FilterOptions.Select(opt =>
                            $"<option value=\\\"{opt.Key}\\\">{System.Web.HttpUtility.HtmlEncode(opt.Value)}</option>"));

                        filterPerColumn.AppendLine($@"
                        {baseFilter}
                        const wrapper{i} = $('<div class=""input-group input-group-sm""></div>');
                        const icon{i} = $('<div class=""input-group-text""><i class=""fas fa-search""></i></div>');
                        const select{i} = $('<select class=""form-select form-select-sm"">{options}</select>');
                        select{i}.on('change', function () {{
                            table.column({i}).search(this.value).draw();
                        }});
                        wrapper{i}.append(icon{i}).append(select{i}).appendTo(cell{i});");
                        break;
                }
            }

            tableColumnsFilterInput = $@"
                table.columns().every(function (colIdx) {{
                    {filterPerColumn}
                }});";

            scriptHandleClickOnToggleMarkup = $@"
                <script>
                    document.addEventListener('DOMContentLoaded', function() {{
                        let filtersVisible = false;
                        const toggleBtn = document.getElementById('{toggleId}');
                        if (toggleBtn) {{
                            toggleBtn.addEventListener('click', function (e) {{
                                e.preventDefault();
                                filtersVisible = !filtersVisible;
                                document.querySelectorAll('#{TableId} thead .filters').forEach(el => {{
                                    el.style.display = filtersVisible ? '' : 'none';
                                }});
                                this.innerHTML = `<i class='fas fa-filter me-1'></i> ${{ filtersVisible ? 'Ocultar filtros' : 'Mostrar filtros' }}`;
                            }});
                        }}
                    }});
                </script>";
        }

        // Return the toggle markup, filter clone script, and filter script.
        return (toggleMarkup, scriptHandleClickOnToggleMarkup, documentReadyCloneHeaders, tableColumnsFilterInput);
    }

    /// <summary>
    /// Handles hover actions for the datatable.
    /// </summary>
    /// <returns>JavaScript code to handle hover actions.</returns>
    private string BuildHoverActionsScripts()
    {
        if (HoverActions is not { Count: > 0 })
        {
            return string.Empty;
        }

        var actionsHtml = new StringBuilder();

        foreach (var action in HoverActions)
        {
            var icon = !string.IsNullOrWhiteSpace(action.IconClass)
                ? $"<i class=\"{action.IconClass}\"></i> "
                : string.Empty;

            var url = action.Url.Replace("%7B", "{").Replace("%7D", "}");

            // Handle both single {token} and double {{token}} braces
            var tokenMatches = Regex.Matches(url, @"\{\{?(.*?)\}?\}")
                .Select(m => m.Groups[1].Value)
                .Distinct()
                .ToList();

            var replacementChain = string.Join(".", tokenMatches.Select(token =>
                $"replace('{{{{{token}}}}}', data.{token})"));

            var urlJs = tokenMatches.Count > 0
                ? $"`{url}`.{replacementChain}"
                : $"'{url}'";

            var js = $@"'<a href=""' + {urlJs} + '"" class=""btn btn-sm btn-outline-primary me-1"">{icon}{action.Text}</a>'";

            actionsHtml.AppendLine(js);
        }

        var actionsJs = string.Join(" + ", actionsHtml.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries));

        return $@"
                table.on('draw', function () {{
                    table.rows().every(function () {{
                        const row = this.node();
                        row.classList.add('hover-actions-trigger', 'position-relative');
                        const data = this.data();

                        const actionContainer = document.createElement('div');
                        actionContainer.className = 'hover-actions position-absolute top-50 end-0 translate-middle-y me-2';
                        actionContainer.innerHTML = {actionsJs};
                        row.appendChild(actionContainer);
                    }});
                }});";
    }

    /// <summary>
    /// Passes the DataTable scripts to the ViewData dictionary.
    /// </summary>
    private void PassDataTableScriptsToViewData(string dataTableScripts)
    {
        if (ViewContext is null)
        {
            throw new InvalidOperationException("ViewContext is required.");
        }

        const string sectionKey = "__DataTableScripts";
        if (!ViewContext.ViewData.ContainsKey(sectionKey))
        {
            ViewContext.ViewData[sectionKey] = new List<string>();
        }

        ((List<string>)ViewContext.ViewData[sectionKey]!).Add(dataTableScripts);
    }
}
