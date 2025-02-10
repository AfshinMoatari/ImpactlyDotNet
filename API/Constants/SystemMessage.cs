using API.Handlers;
using System;
using System.Globalization;
using System.Linq;

namespace API.Constants;

public interface ISystemMessage
{
    public string GetLanguageName();
    public string ErrorNotFoundProject();

    public string ErrorNoProjects();
    public string ErrorNotFoundSurvey();
    public string ErrorSurveyInUsed();
    public string ErrorNotLogin();
    public string ErrorNoPermission();
    public string ErrorUserNotExist();
    public string ErrorUserNotAdmin();
    public string ErrorProjectNotLogin();
    public string ErrorUserNotConnectedToProject();
    public string ErrorUserNotConnectedToAny();
    public string ErrorRefreshTokenEmpty();
    public string ErrorNoUserId();
    public string ErrorNotFoundStrategy();
    public string ErrorNotFoundReport();
    public string ErrorNotFoundTag();
    public string ErrorNotFoundCitizen();
    public string ErrorNotFoundRegistration();
    public string ErrorNotFoundFreeText();
    public string ErrorNotFoundReportCode();
    public string ErrorNotFoundSurveyCode();
    public string ErrorNotFoundUser();
    public string ErrorEmailFailed();

    public string ErrorCodeCreateFailed();

    public string ErrorPatientHasStrategy();
    public string ErrorCreateFrequency();
    public string ErrorCreateStrategy();
    public string ErrorDifferentUser();

    public string ErrorTagExisted();
    public string ErrorSetRequestedCulture();
    public string OkExportReady();
    
    
    public string OkLogEmailReady();

    public string InfoImportCitizens();
    public string BulkUploadTitle(string fileName);
    public string DefaultWelcomeMessage();

    public string DefaultSurveyMessage();

    public string ShortMessage(string to, string content, string from);
    public string WelcomeTo(string name);
    public string Unnamed();

    public string PasswordReset();

    public string[] ForgetPasswordEmailLines();
    public string[] BulkUploadEmailLines();
    public string[] DataDumpEmailLines();
    public string[] SurveyEmailLines();
    public string[] WelcomeAuthUserEmailLines();
    public string[] WelcomePatientEmailLines();

    public string SurveyEmailTitle();

    public string BulkUploadUpdateStatus(string valueName, string oldValue, string newValue);

    public string ReportResponseKeys(string frequency, DateTime? date, int? year, int? weeknr, int? monthnr, int? quarternr);
    public string ReportResponsePeriod(string period, DateTime? startPeriod, DateTime? endPeriod); 

    public string DumpField(string fieldName);
    public string GetRegistrationTypeName(string registrationType);

    public string SmsSurvey(string patientName, string messageText, string linkUrl, string projectName);

    public string DumpFieldMapToFieldName(string displayName);
}

public class MessageEnglish: ISystemMessage
{

    public string ErrorNoProjects()
    {
        return "There are no projects";
    }

    public string GetLanguageName()
    {
        return Languages.English;
    }

    public string ErrorNotFoundProject()
    {
        return "The project you are trying to access does not exist";
    }

    public string ErrorNotFoundSurvey()
    {
        return "The questionnaire does not exist";
    }

    public string ErrorSurveyInUsed()
    {
        return "The questionnaire is being used in a strategy.  Remove it first.";
    }

    public string ErrorNotLogin()
    {
        return "You cannot access without being logged in";
    }

    public string ErrorNoPermission()
    {
        return "You don't have the permission with your current login.";
    }

    public string ErrorUserNotExist()
    {
        return "The user does not exist";
    }

    public string ErrorUserNotAdmin()
    {
        return "The user is not an admin";
    }

    public string ErrorProjectNotLogin()
    {
        return "You cannot access a project without being logged in";
    }

    public string ErrorUserNotConnectedToProject()
    {
        return "Your user is not connected to the project";
    }

    public string ErrorUserNotConnectedToAny()
    {
        return "Your user is not connected to any project";
    }

    public string ErrorRefreshTokenEmpty()
    {
        return "Refresh token cannot be empty";
    }

    public string ErrorNoUserId()
    {
        return "Invalid ID";
    }

    public string ErrorNotFoundStrategy()
    {
        return "The strategy you are trying to access does not exist";
    }

    public string ErrorNotFoundReport()
    {
        return "The report you are trying to access does not exist";
    }

    public string ErrorNotFoundTag()
    {
        return "The tag you are trying to access does not exist";
    }

    public string ErrorNotFoundCitizen()
    {
        return "The citizen you are trying to access does not exist";
    }

    public string ErrorNotFoundRegistration()
    {
        return "Registration you are trying to access does not exist";
    }

    public string ErrorNotFoundFreeText()
    {
        return "The free text you are trying to access does not exist";
    }

    public string ErrorNotFoundReportCode()
    {
        return "Could not find the report code";
    }

    public string ErrorNotFoundSurveyCode()
    {
        return "Could not find survey code";
    }

    public string ErrorNotFoundUser()
    {
        return "The user you are trying to access does not exist";
    }

    public string ErrorEmailFailed()
    {
        return "The email send failed";
    }

    public string ErrorCodeCreateFailed()
    {
        return "Code creation failed";
    }

    public string ErrorPatientHasStrategy()
    {
        return "The citizen has a focused strategy";
    }

    public string ErrorCreateFrequency()
    {
        return "Could not create the frequency";
    }

    public string ErrorCreateStrategy()
    {
        return "Could not create the data strategy";
    }

    public string ErrorDifferentUser()
    {
        return "Different users are specified";
    }

    public string ErrorTagExisted()
    {
        return "This tag already exists";
    }

    public string OkExportReady()
    {
        return "Export data is ready";
    }

    public string OkLogEmailReady()
    {
        return "New Import Citizen Log";
    }

    public string InfoImportCitizens()
    {
        return
            "Only isActive, name, last name, email, phone number, municipality, postal number and sex can be updated.";
    }

    public string BulkUploadTitle(string fileName)
    {
        return "Your dataimport “ + fileName + ” has been uploaded to Impactly.";
    }

    public string DefaultWelcomeMessage()
    {
        return "We are delighted to welcome you to @Model.ProjectName.\n" +
               "During your course, you will receive questionnaires from us via SMS or email.\n" +
               "\n" +
               "You don't have to do anything right now, we'll send you an email with a questionnaire shortly.\n" +
               "\n" +
               "Sincerely \n";
    }

    public string DefaultSurveyMessage()
    {
        return 
        "We have sent you a questionnaire that we ask you to answer. \n" +
            "\n" +
            "Your answer is important and helps us here at @Model.ProjectName to follow your development and make your process even better. \n";
    }

    public string ErrorSetRequestedCulture()
    {
        return "The Language does not support the requested culture.";
    }

    public string ShortMessage(string to, string content, string from)
    {
        return "Dear " + to + "\n\n" + content + "\n\nRegards " + from;
    }

    public string WelcomeTo(string name)
    {
        return "Welcome to " + name;
    }

    public string Unnamed()
    {
        return "Unnamed";
    }

    public string PasswordReset()
    {
        return "Password Reset";
    }

    public string[] ForgetPasswordEmailLines()
    {
        var lines = new string[11];
        lines[0] = "Dear";
        lines[1] = "We have received a request to reset your password.";
        lines[2] = "You can reset the password by clicking the “Reset” button. The link will work for the next 24 hours.";
        lines[3] = "If you have not requested to reset your password or you have regretted it, you can ignore this email.";
        lines[4] = "Sincerely ";
        lines[5] = "Reset";
        lines[6] = "*If the button doesn't work, copy the link and paste it into your browser:";
        lines[7] = "We take care of your data and comply with GDPR. Do you want to read more about data security? Then read here.";
        lines[8] = "Contact our support";
        lines[9] = "If you have any questions, feel free to contact us on weekdays 10:00-16:00 through:";
        lines[10] =
            "The product Impactly is owned by Impactly ApS, with office at Kronsprinsensgade 13, 1. 1114, Copenhagen, Denmark. Contact via info@impactly.dk and (+45) 31 33 54 55.";
        return lines;
    }

    public string[] BulkUploadEmailLines()
    {
        var lines = new string[11];
        lines[0] = "You are receiving this email because you have imported data into Impactly. It is important that you open the file and " +
                   "checking for errors in your import.";
        lines[1] = "What should I do if there are errors in my import?";
        lines[2] =
            "If there are errors in your import, you must read the error code in the excel file via the link, and correct the lines," +
            "on which there are errors. After you have corrected the errors, you must import the errored lines again. You should not " +
            "import the approved lines one more time!";
        lines[3] = "What should I do if I can't figure out how to fix the errors?";
        lines[4] = "Contact our support at daniel@impactly.dk or +4520729960.";
        lines[5] = "Import name";
        lines[6] = "Date";
        lines[7] = "Download link expires in 7 days.";
        lines[8] = "Download";
        lines[9] = "This email has been sent to";
        lines[10] =
            "because a data export has been generated via the Impactly platform, by a user with this email. If this is not correct, please contact info@impactly.dk";
        return lines;
    }

    public string[] DataDumpEmailLines()
    {
        var lines = new string[11];
        lines[0] = "Your data export";
        lines[1] = "has been delivered.";
        lines[2] = "Export type";
        lines[3] = "Export name";
        lines[4] = "Data types exported";
        lines[5] = "Date";
        lines[6] = "Date range for export data";
        lines[7] = "Download link expires in 7 days.";
        lines[8] = "Download";
        lines[9] = "This email has been sent to";
        lines[10] =
            "because a data export has been generated via the Impactly platform, by a user with this email. If this is not correct, please contact info@impactly.dk";
        return lines;
    }

    public string[] SurveyEmailLines()
    {
        var lines = new string[6];
        lines[0] = "Question form from ";
        lines[1] = "Dear";
        lines[2] = "*If the button doesn't work, copy the link and paste it into your browser:";
        lines[3] =
            "The product Impactly is owned by Impactly ApS, with office at Kronsprinsensgade 13, 1. 1114 Copenhagen, Denmark. Contact via info@impactly.dk and (+45) 31 33 54 55.";
        lines[4] = "Answer questionnaire";
        lines[5] = "Sincerely";
        return lines;
    }

    public string[] WelcomeAuthUserEmailLines()
    {
        var lines = new string[14];
        lines[0] = "Welcome to Impactly";
        lines[1] = "Dear";
        lines[2] = "from";
        lines[3] = "have invited you to become part of Impactly so you can start measuring your impact.";
        lines[4] =
            "We are happy to have you as a customer, and look forward to supporting you in showing your social impact.";
        lines[5] = "Click the button to start using Impactly.";
        lines[6] = "Sincerely ";
        lines[7] = "Get started";
        lines[8] = "*If the button doesn't work, copy the link and paste it into your browser:";
        lines[9] = "We take care of your data and comply with GDPR. Do you want to read more about data security? Then read here.";
        lines[10] = "Do you want help with something?";
        lines[11] = "Contact our support";
        lines[12] = "If you have any questions, please feel free to contact us on weekdays 10:00-16:00 through:";
        lines[13] =
            "The product Impactly is owned by Impactly ApS, with office at Kronsprinsensgade 13, 1. 1114, Copenhagen, Denmark. Contact via info@impactly.dk and (+45) 31 33 54 55.";
        return lines;
    }

    public string[] WelcomePatientEmailLines()
    {
        var lines = new string[4];
        lines[0] = "Welcome to";
        lines[1] = "Dear";
        lines[2] = "Greetings";
        lines[3] =
            "The product Impactly is owned by Impactly ApS, with office at Kronsprinsensgade 13, 1. 1114, Copenhagen, Denmark. Contact via info@impactly.dk and (+45) 31 33 54 55.";
        return lines;
    }

    public string SurveyEmailTitle()
    {
        return "New Questionnaire";
    }

    public string BulkUploadUpdateStatus(string valueName, string oldValue, string newValue)
    {
        return valueName switch
        {
            "IsActive" => "IsActive: from " + oldValue + " to " + newValue,
            "Email" => "Email: from "  +  oldValue +  " to " + newValue,
            "PhoneNumber" => "PhoneNumber: from "  +  oldValue +  " to " + newValue,
            "Municipality" => "Municipality: from "  +  oldValue +  " to " + newValue,
            "Sex" => "Sex: from "  +  oldValue +  " to " + newValue,
            "PostNumber" => "PostNumber: from "  +  oldValue +  " to " + newValue,
            _ => ""
        };
    }

    public string ReportResponseKeys(string frequency, DateTime? date, int? year, int? weeknr, int? monthnr, int? quarternr)
    {
        return frequency switch
        {
            "Daily" => date.Value.ToString("dd/MM/yy", new CultureInfo("en-US")),
            "Weekly" => year + " - Week " + weeknr,
            "Monthly" => year + " - " + monthnr,
            "Quarterly" => year + " - Quarter " + quarternr,
            "Annual" => year.ToString(),
            _ => ""
        };
    }

    public string ReportResponsePeriod(string period, DateTime? startPeriod, DateTime? endPeriod)
    {
        return period switch
        {
            "ThisWeek" => "This week",
            "LastWeek" => "Last week",
            "ThisMonth" => "This month",
            "LastMonth" => "Last month",
            "ThisQuarter" => "This quarter",
            "LastQuarter" => "Last quarter",
            "ThisYear" => "This year",
            "LastYear" => "Last year",
            _ => startPeriod.Value.Date.ToString("dd/MM/yy", new CultureInfo("en-US")) + "-" + endPeriod.Value.Date.ToString("dd/MM/yy", new CultureInfo("en-US"))
        };
    }

    public string DumpField(string fieldName)
    {
        DumpFields.FieldsInEnglish.TryGetValue(fieldName, out var value);
        return value;
    }

    public string GetRegistrationTypeName(string registrationType)
    {
        DumpFields.FieldsInEnglish.TryGetValue(DumpFields.RegistrationTypeStatus, out var typeStatus);
        DumpFields.FieldsInEnglish.TryGetValue(DumpFields.RegistrationTypeNumeric, out var typeNumeric);
        DumpFields.FieldsInEnglish.TryGetValue(DumpFields.RegistrationTypeIncident, out var typeIncident);
        if (registrationType == typeStatus)
        {
            DumpFields.FieldsInEnglish.TryGetValue(DumpFields.FilterStatusRegistration, out var rs);
            return rs;
        }

        if (registrationType == typeNumeric)
        {
            DumpFields.FieldsInEnglish.TryGetValue(DumpFields.FilterNumericRegistration, out var rs);
            return rs;
        }

        if (registrationType == typeIncident)
        {
            DumpFields.FieldsInEnglish.TryGetValue(DumpFields.FilterIncidentRegistration, out var rs);
            return rs;
        }

        return string.Empty;

    }

    public string SmsSurvey(string patientName, string messageText, string linkUrl, string projectName)
    {
        return "Dear " + patientName + "\n\n " + messageText + "   Download here: " + linkUrl + "\n\nRegards " +
               projectName;
    }

    public string DumpFieldMapToFieldName(string displayName)
    {
        return DumpFields.FieldsInEnglish.FirstOrDefault(d => d.Value == displayName).Key.ToString();
    }
}

public class MessageDanish: ISystemMessage
{
    public string ErrorNoProjects()
    {
        return "Der er ingen projekter";
    }

    public string GetLanguageName()
    {
        return Languages.Danish;
    }

    public string ErrorNotFoundProject()
    {
        return "Projektet du prøver at tilgå eksisterer ikke";
    }

    public string ErrorNotFoundSurvey()
    {
        return "Dette spørgeskema findes ikke";
    }

    public string ErrorSurveyInUsed()
    {
        return "Spørgeskemaet bliver brugt i en strategi. Fjern det først";
    }

    public string ErrorNotLogin()
    {
        return "Du kan ikke få adgang til admin uden at være logget ind";
    }

    public string ErrorNoPermission()
    {
        return "Du kan ikke få adgang til denne admin med dit nuværende login";
    }

    public string ErrorUserNotExist()
    {
        return "Brugeren eksistere ikke";
    }

    public string ErrorUserNotAdmin()
    {
        return "Din bruger er ikke admin";
    }

    public string ErrorProjectNotLogin()
    {
        return "Du kan ikke få adgang til et projekt uden at være logget ind";
    }

    public string ErrorUserNotConnectedToProject()
    {
        return "Din bruger er ikke forbundet til projektet";
    }

    public string ErrorUserNotConnectedToAny()
    {
        return "Din bruger er ikke forbundet til noget projekt";
    }

    public string ErrorRefreshTokenEmpty()
    {
        return "Refresh token blev ikke fundet";
    }

    public string ErrorNoUserId()
    {
        return "Ugyldigt ID";
    }

    public string ErrorNotFoundStrategy()
    {
        return "Strategien du prøver at tilgå eksisterer ikke";
    }

    public string ErrorNotFoundReport()
    {
        return "Rapporten du prøver at tilgå eksisterer ikke";
    }

    public string ErrorNotFoundTag()
    {
        return "Tagen du prøver at tilgå eksisterer ikke";
    }

    public string ErrorNotFoundCitizen()
    {
        return "Borgeren du prøver at tilgå eksisterer ikke";
    }

    public string ErrorNotFoundRegistration()
    {
        return "Registration du prøver at tilgå eksisterer ikke";
    }

    public string ErrorNotFoundFreeText()
    {
        return "Fri tekst du prøver at tilgå eksisterer ikke";
    }

    public string ErrorNotFoundReportCode()
    {
        return "Kunne ikke finde rapportkoden";
    }

    public string ErrorNotFoundSurveyCode()
    {
        return "Kunne ikke finde spørgeskemakoden";

    }

    public string ErrorNotFoundUser()
    {
        return "Brugeren du prøver at tilgå eksisterer ikke";
    }

    public string ErrorEmailFailed()
    {
        return "Email mislykkedes";
    }

    public string ErrorCodeCreateFailed()
    {
        return "Kodeoprettelse mislykkedes";
    }

    public string ErrorPatientHasStrategy()
    {
        return "Borgeren har en tilnykktet strategi";
    }

    public string ErrorCreateFrequency()
    {
        return "Kunne ikke oprette udsendelserne";
    }

    public string ErrorCreateStrategy()
    {
        return "Kunne ikke oprette datastrategien";
    }

    public string ErrorDifferentUser()
    {
        return "Forskellige brugere er angivet";
    }

    public string ErrorTagExisted()
    {
        return "Dette tag eksisterer allerede";
    }

    public string ErrorSetRequestedCulture()
    {
        return "Sproget understøtter ikke den ønskede kultur.";
    }

    public string OkExportReady()
    {
        return "Eksportdata er klar";
    }

    public string OkLogEmailReady()
    {
        return "Log email er klar";
    }

    public string InfoImportCitizens()
    {
        return "Kun isActive, navn, efternavn, email, telefonnummer, kommune, postnummer og køn kan opdateres.";;
    }

    public string BulkUploadTitle(string fileName)
    {
        return "Din dataimport “ + fileName + ” er blevet uploadet til Impactly.";
    }

    public string DefaultWelcomeMessage()
    {
        return 
            "Vi er glade for at kunne byde dig velkommen hos @Model.ProjectName.\n" +
            "Du vil under dit forløb modtage spørgeskemaer fra os via sms eller email.\n" +
            " \n" +
            "Du skal ikke gøre noget lige nu, vi sender dig snart en email med et spørgeskema.\n" +
            " \n" +
            "Med venlig hilsen \n";
    }

    public string DefaultSurveyMessage()
    {
        return "Vi har sendt dig et spørgeskema, som vi beder dig om at besvare. \n" +
               " \n" +
               "Din besvarelse er vigtig og hjælper os her hos @Model.ProjectName med at følge din udvikling og gøre dit forløb endnu bedre. \n" +
               " \n" +
               "Med venlig hilsen \n";
    }

    public string ShortMessage(string to, string content, string from)
    {
        return "Kære " + to + "\n\n" + content + "\n\nHilsen " + from;
    }

    public string WelcomeTo(string name)
    {
        return "Velkommen til " + name;
    }

    public string Unnamed()
    {
        return "Unavngivet";
    }

    public string PasswordReset()
    {
        return "Nulstilling af adgangskode";
    }

    public string[] ForgetPasswordEmailLines()
    {
        var lines = new string[11];
        lines[0] = "Kære";
        lines[1] = "Vi har modtaget en anmodning om at nulstille din adgangskode.";
        lines[2] = "Du kan nulstille adgangskoden ved at klikke på knappen “Nulstil”. Linket virker de næste 24 timer.";
        lines[3] = "Hvis ikke du har anmodet om at nulstille din adgangskode eller du har fortrudt, så kan du ignorere denne mail.";
        lines[4] = "Med venlig hilsen ";
        lines[5] = "Nulstil";
        lines[6] = "*Hvis knappen ikke fungerer, så kopier linket og indsæt det i din browser:";
        lines[7] = "Vi passer på jeres data og efterlever GDPR. Vil du læse mere om datasikkerhed? Så læs med her.";
        lines[8] = "Kontakt vores support";
        lines[9] = "Hvis du har spørgsmål, er du velkommen til at kontakte os på hverdage 10:00-16:00 gennem:";
        lines[10] =
            "Produktet Impactly ejes af Impactly ApS, med kontor på Kronsprinsensgade 13, 1. 1114, København, Denmark. Kontakt sker via info@impactly.dk og (+45) 31 33 54 55.";
        return lines;
    }

    public string[] BulkUploadEmailLines()
    {
        var lines = new string[11];
        lines[0] = "Du modtager denne mail, fordi du har importeret data til Impactly. Det er vigtigt at du åbner filen og " +
                   "tjekker, om der er fejl i din import.";
        lines[1] = "Hvad skal jeg gøre, hvis der er fejl i min import?";
        lines[2] =
            "Hvis der er fejl i din import, skal du læse fejlkoden i excel filen via linket, og tilrette de linjer," +
            "som der er fejl på. Når du har rettet fejlene, skal du importere de fejlede linjer igen. Du bør ikke " +
            "importere de godkendte linjer en gang til!";
        lines[3] = "Hvad skal jeg gøre, hvis jeg ikke kan finde ud af at rette fejlene?";
        lines[4] = "Kontakt vores support på daniel@impactly.dk eller +4520729960.";
        lines[5] = "Import navn";
        lines[6] = "Dato";
        lines[7] = "Linket til download udløber om 7 dage.";
        lines[8] = "Download";
        lines[9] = "Denne mail er blevet sendt til";
        lines[10] =
            "fordi en dataeksport er blevet genereret via Impactly platformen, af en bruger med denne email. Hvis ikke dette er korrekt, kontakt venligst info@impactly.dk";
        return lines;
    }

    public string[] DataDumpEmailLines()
    {
        var lines = new string[11];
        lines[0] = "Din dataeksport";
        lines[1] = "er blevet leveret.";
        lines[2] = "Eksport type";
        lines[3] = "Eksport navn";
        lines[4] = "Datatyper eksporteret";
        lines[5] = "Dato";
        lines[6] = "Datointerval for eksportdata";
        lines[7] = "Linket til download udløber om 7 dage.";
        lines[8] = "Download";
        lines[9] = "Denne mail er blevet sendt til";
        lines[10] =
            "fordi en dataeksport er blevet genereret via Impactly platformen, af en bruger med denne email. Hvis ikke dette er korrekt, kontakt venligst info@impactly.dk";
        return lines;
    }

    public string[] SurveyEmailLines()
    {
        var lines = new string[6];
        lines[0] = "Spørgeskema fra ";
        lines[1] = "Kære";
        lines[2] = "*Hvis knappen ikke fungerer, så kopier linket og indsæt det i din browser:";
        lines[3] =
            "Produktet Impactly ejes af Impactly ApS, med kontor på Kronsprinsensgade 13, 1. 1114 København, Denmark. Kontakt sker via info@impactly.dk og (+45) 31 33 54 55.";
        lines[4] = "Besvar spørgeskema";
        lines[5] = "Hilsen";
        return lines;
    }

    public string[] WelcomeAuthUserEmailLines()
    {
        var lines = new string[14];
        lines[0] = "Velkommen til Impactly";
        lines[1] = "Kære";
        lines[2] = "fra";
        lines[3] = "har inviteret dig til at blive en del af Impactly, så I kan begynde at måle på jeres effekt.";
        lines[4] =
            "Vi er glade for at have jer som kunde, og glæder os til at understøtte jer i at vise jeres sociale impact.";
        lines[5] = "Klik på knappen for at komme i gang med at bruge Impactly.";
        lines[6] = "Med venlig hilsen ";
        lines[7] = "Kom i gang";
        lines[8] = "*Hvis knappen ikke fungerer, så kopier linket og indsæt det i din browser:";
        lines[9] = "Vi passer på jeres data og efterlever GDPR. Vil du læse mere om datasikkerhed? Så læs med her.";
        lines[10] = "Vil du have hjælp til noget?";
        lines[11] = "Kontakt vores support";
        lines[12] = "Hvis du har spørgsmål, er du velkommen til at kontakte os på hverdage 10:00-16:00 gennem:";
        lines[13] =
            "Produktet Impactly ejes af Impactly ApS, med kontor på Kronsprinsensgade 13, 1. 1114, København, Denmark. Kontakt sker via info@impactly.dk og (+45) 31 33 54 55.";
        
        return lines;
    }

    public string[] WelcomePatientEmailLines()
    {
        var lines = new string[4];
        lines[0] = "Velkommen til";
        lines[1] = "Kære";
        lines[2] = "Hilsen";
        lines[3] =
            "Produktet Impactly ejes af Impactly ApS, med kontor på Kronsprinsensgade 13, 1. 1114, København, Denmark. Kontakt sker via info@impactly.dk og (+45) 31 33 54 55.";
        return lines;
    }

    public string SurveyEmailTitle()
    {
        return "Nyt spørgeskema!";
    }

    public string BulkUploadUpdateStatus(string valueName, string oldValue, string newValue)
    {
        return valueName switch
        {
            "IsActive" => "IsActive: fra " + oldValue + " til " + newValue,
            "Email" => "Email: fra "  +  oldValue +  " til " + newValue,
            "PhoneNumber" => "Telefonnummer: fra "  +  oldValue +  " til " + newValue,
            "Municipality" => "Kommune: fra "  +  oldValue +  " til " + newValue,
            "Sex" => "Køn: fra "  +  oldValue +  " til " + newValue,
            "PostNumber" => "Postnummer: fra "  +  oldValue +  " til " + newValue,
            _ => ""
        };
    }

    public string ReportResponseKeys(string frequency, DateTime? date, int? year, int? weeknr, int? monthnr, int? quarternr)
    {
        return frequency switch
        {
            "Daily" => date.Value.ToString("dd/MM/yy", new CultureInfo("en-US")),
            "Weekly" => year + " - Uge " + weeknr,
            "Monthly" => year + " - " + monthnr,
            "Quarterly" => year + " - Kvartal " + quarternr,
            "Annual" => year.ToString(),
            _ => ""
        };
    }

    public string ReportResponsePeriod(string period, DateTime? startPeriod, DateTime? endPeriod)
    {
        return period switch
        {
            "ThisWeek" => "Denne uge",
            "LastWeek" => "Sidste uge",
            "ThisMonth" => "Denne måned",
            "LastMonth" => "Sidste måned",
            "ThisQuarter" => "Dette kvartal",
            "LastQuarter" => "Sidste kvartal",
            "ThisYear" => "Dette år",
            "LastYear" => "Sidste år",
            _ => startPeriod.Value.Date.ToString("dd/MM/yy", new CultureInfo("en-US")) + "-" + endPeriod.Value.Date.ToString("dd/MM/yy", new CultureInfo("en-US"))
        };
    }

    public string DumpField(string fieldName)
    {
        DumpFields.FieldsInDanish.TryGetValue(fieldName, out var value);
        return value;
    }

    public string DumpFieldMapToFieldName(string displayName)
    {
        return 
            DumpFields.FieldsInDanish.FirstOrDefault(d => d.Value == displayName).Key.ToString();
    }

    public string GetRegistrationTypeName(string registrationType)
    {
        DumpFields.FieldsInDanish.TryGetValue(DumpFields.RegistrationTypeStatus, out var typeStatus);
        DumpFields.FieldsInDanish.TryGetValue(DumpFields.RegistrationTypeNumeric, out var typeNumeric);
        DumpFields.FieldsInDanish.TryGetValue(DumpFields.RegistrationTypeIncident, out var typeIncident);
        if (registrationType == typeStatus)
        {
            DumpFields.FieldsInDanish.TryGetValue(DumpFields.FilterStatusRegistration, out var rs);
            return rs;
        }

        if (registrationType == typeNumeric)
        {
            DumpFields.FieldsInDanish.TryGetValue(DumpFields.FilterNumericRegistration, out var rs);
            return rs;
        }

        if (registrationType == typeIncident)
        {
            DumpFields.FieldsInDanish.TryGetValue(DumpFields.FilterIncidentRegistration, out var rs);
            return rs;
        }

        return string.Empty;
    }

    public string SmsSurvey(string patientName, string messageText, string linkUrl, string projectName)
    {
        return "Kære " + patientName + "\n\n " + messageText + "   Besvar her: " + linkUrl + "\n\nHilsen " +
               projectName;
    }
}