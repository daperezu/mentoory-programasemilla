using System;
using System.Collections.Generic;
using System.Linq;
using LinaSys.Shared.Domain.SeedWork;

namespace LinaSys.Core.Domain.Aggregates.Navigation
{
    public class NavigationMenuItem : Entity, IAggregateRoot
    {
        // Constructors
        public NavigationMenuItem(
            string code,
            string displayText,
            long? parentId = null,
            int sortOrder = 0)
        {
            SetCode(code);
            SetDisplayText(displayText);
            ParentId = parentId;
            SortOrder = sortOrder;
            Level = 0;
            IsActive = true;
            RequiresAuthentication = true;
            Children = [];
        }

        protected NavigationMenuItem()
        {
            Children = [];
        }

        // Core Properties
        public string Code { get; private set; } = string.Empty;
        public string DisplayText { get; private set; } = string.Empty;

        // Hierarchy
        public long? ParentId { get; private set; }
        public int SortOrder { get; private set; }
        public int Level { get; private set; }

        // Presentation
        public string? Icon { get; private set; }
        public string? CssClass { get; private set; }

        // Navigation
        public string? Url { get; private set; }

        // Display Control
        public bool IsSection { get; private set; }
        public bool IsActive { get; private set; }

        // Context Requirements
        public bool RequiresAuthentication { get; private set; }
        public bool RequiresIncubator { get; private set; }
        public bool RequiresProject { get; private set; }

        // Authorization
        public string? AllowedRoles { get; private set; }

        // Navigation Properties
        public virtual NavigationMenuItem? Parent { get; private set; }
        public virtual ICollection<NavigationMenuItem> Children { get; private set; }

        // Domain Methods
        public void SetCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Code is required");
            }

            if (code.Length > 50)
            {
                throw new ArgumentException("Code cannot exceed 50 characters");
            }

            Code = code.ToUpperInvariant();
        }

        public void SetDisplayText(string displayText)
        {
            if (string.IsNullOrWhiteSpace(displayText))
            {
                throw new ArgumentException("Display text is required");
            }

            if (displayText.Length > 100)
            {
                throw new ArgumentException("Display text cannot exceed 100 characters");
            }

            DisplayText = displayText;
        }

        public void SetUrl(string? url)
        {
            if (!string.IsNullOrWhiteSpace(url) && url != "#")
            {
                Url = url;
                IsSection = false;
            }
        }

        public void SetAsSection()
        {
            Url = "#";
            IsSection = true;
        }

        public void SetPresentation(string? icon, string? cssClass = null)
        {
            Icon = icon;
            CssClass = cssClass;
        }

        public void SetContextRequirements(bool requiresIncubator, bool requiresProject)
        {
            RequiresIncubator = requiresIncubator;
            RequiresProject = requiresProject;
        }

        public void SetAllowedRoles(params string[] roles)
        {
            AllowedRoles = roles?.Any() == true ? string.Join(",", roles) : null;
        }

        public void Activate() => IsActive = true;

        public void Deactivate() => IsActive = false;

        public void UpdateSortOrder(int sortOrder)
        {
            if (sortOrder < 0)
            {
                throw new ArgumentException("Sort order must be non-negative");
            }

            SortOrder = sortOrder;
        }

        public void CalculateLevel()
        {
            Level = Parent?.Level + 1 ?? 0;
            foreach (var child in Children)
            {
                child.CalculateLevel();
            }
        }

        // Business Logic Methods
        public bool IsAllowedForRole(string? roleName)
        {
            if (string.IsNullOrWhiteSpace(AllowedRoles))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(roleName))
            {
                return false;
            }

            var roles = AllowedRoles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(r => r.Trim());
            return roles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
        }

        public bool IsAllowedForRoles(IEnumerable<string>? roleNames)
        {
            if (string.IsNullOrWhiteSpace(AllowedRoles))
            {
                return true;
            }

            return roleNames?.Any(role => IsAllowedForRole(role)) ?? false;
        }

        public bool IsVisibleInContext(bool hasIncubator, bool hasProject, bool isAuthenticated)
        {
            if (!IsActive)
            {
                return false;
            }

            if (RequiresAuthentication && !isAuthenticated)
            {
                return false;
            }

            if (RequiresIncubator && !hasIncubator)
            {
                return false;
            }

            if (RequiresProject && !hasProject)
            {
                return false;
            }

            return true;
        }

        public string GetUrl(long? incubatorId = null, long? projectId = null)
        {
            if (string.IsNullOrWhiteSpace(Url))
            {
                return "#";
            }

            var result = Url;

            // Replace placeholders with actual values
            if (incubatorId.HasValue && result.Contains("{incubatorId}"))
            {
                result = result.Replace("{incubatorId}", incubatorId.Value.ToString());
            }

            if (projectId.HasValue && result.Contains("{projectId}"))
            {
                result = result.Replace("{projectId}", projectId.Value.ToString());
            }

            return result;
        }
    }
}