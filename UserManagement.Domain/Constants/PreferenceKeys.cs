namespace LinaSys.UserManagement.Domain.Constants;

public static class PreferenceKeys
{
    // Email preferences
    public const string EmailSystemWelcome = "email.system.welcome";
    public const string EmailProjectWelcome = "email.project.welcome";
    public const string EmailApprovals = "email.approvals";
    public const string EmailRejections = "email.rejections";
    public const string EmailReminders = "email.reminders";
    public const string EmailVerificationRequired = "email.verification.required";
    public const string EmailDigest = "email.digest";
    public const string EmailAnnouncements = "email.announcements";
    public const string EmailTaskAssignments = "email.task.assignments";
    public const string EmailFormDeadlines = "email.form.deadlines";
    public const string EmailMentorMessages = "email.mentor.messages";
    public const string EmailPasswordReset = "email.password.reset";
    public const string EmailAccountChanges = "email.account.changes";
    public const string EmailFormSubmissions = "email.form.submissions";
    public const string EmailInvitations = "email.invitations";
    public const string EmailNotifications = "email.notifications";

    // Notification preferences
    public const string NotificationInApp = "notification.inapp";
    public const string NotificationSound = "notification.sound";
    public const string NotificationBrowser = "notification.browser";

    // Display preferences
    public const string DisplayLanguage = "display.language";
    public const string DisplayTimeZone = "display.timezone";
    public const string DisplayDateFormat = "display.dateformat";

    // Privacy preferences
    public const string PrivacyProfileVisible = "privacy.profile.visible";
    public const string PrivacyEmailVisible = "privacy.email.visible";
    public const string PrivacyProjectsVisible = "privacy.projects.visible";
}

public static class PreferenceDefaults
{
    public static readonly Dictionary<string, string> DefaultValues = new()
    {
        // Email preferences - all opt-in by default
        { PreferenceKeys.EmailSystemWelcome, "true" },
        { PreferenceKeys.EmailProjectWelcome, "true" },
        { PreferenceKeys.EmailApprovals, "true" },
        { PreferenceKeys.EmailRejections, "true" },
        { PreferenceKeys.EmailReminders, "true" },
        { PreferenceKeys.EmailVerificationRequired, "true" },
        { PreferenceKeys.EmailDigest, "false" },
        { PreferenceKeys.EmailAnnouncements, "true" },
        { PreferenceKeys.EmailTaskAssignments, "true" },
        { PreferenceKeys.EmailFormDeadlines, "true" },
        { PreferenceKeys.EmailMentorMessages, "true" },
        { PreferenceKeys.EmailPasswordReset, "true" },
        { PreferenceKeys.EmailAccountChanges, "true" },
        { PreferenceKeys.EmailFormSubmissions, "true" },
        { PreferenceKeys.EmailInvitations, "true" },
        { PreferenceKeys.EmailNotifications, "false" },

        // Notification preferences
        { PreferenceKeys.NotificationInApp, "true" },
        { PreferenceKeys.NotificationSound, "false" },
        { PreferenceKeys.NotificationBrowser, "false" },

        // Display preferences
        { PreferenceKeys.DisplayLanguage, "es-ES" },
        { PreferenceKeys.DisplayTimeZone, "Central Standard Time (Mexico)" },
        { PreferenceKeys.DisplayDateFormat, "dd/MM/yyyy" },

        // Privacy preferences
        { PreferenceKeys.PrivacyProfileVisible, "true" },
        { PreferenceKeys.PrivacyEmailVisible, "false" },
        { PreferenceKeys.PrivacyProjectsVisible, "true" }
    };
}