using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using API.Constants;
using API.Models.Notifications;
using API.Models.Strategy;
using API.Repositories;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.web.v1;

[Authorize]
[ApiController]
[Route("api/web/v1/projects/{projectId}/notifications")]
public class NotificationController : BaseController
{
    private readonly NotificationService _notificationService;
    private readonly ICodeContext _codeContext;
    private readonly IStrategyContext _strategyContext;

    public NotificationController(NotificationService notificationService, ICodeContext codeContext, IStrategyContext strategyContext)
    {
        _notificationService = notificationService;
        _codeContext = codeContext;
        _strategyContext = strategyContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<Notification>>> GetNotifications([FromRoute] string projectId)
    {
        IEnumerable<Notification> notifications = await _notificationService.GetNotifications(projectId);
        foreach (var notification in notifications)
        {
            if (notification.NotificationType == NotificationType.Survey)
            {
                notification.Surveys = await GetSurveysFromCode(notification.SurveyCode);
            }
        }
        return Ok(notifications);
    }

    private Type GetConfigType(NotificationType notificationType)
    {
        string configTypeName = $"{notificationType}NotificationModuleConfig";
        Type configType = Assembly.GetExecutingAssembly()
            .GetType($"API.Models.Notifications.NotificationModuleConfig.{configTypeName}");
        if (configType == null)
        {
            throw new ArgumentOutOfRangeException($"Config type not found for notification type: {notificationType}");
        }

        return configType;
    }

    private async  Task<IEnumerable<Survey>> GetSurveysFromCode(string code)
    {
        var surveyCode = await _codeContext.SurveyCodes.Read(code);
        var surveys = await _strategyContext.ReadSurveysFromProperty(surveyCode.Properties);
        return surveys;
    }


    [HttpPost("notification")]
    public async Task<ActionResult<bool>> SaveNotification([FromBody] Notification notification)
    {
        bool success = await _notificationService.SaveNotification(notification);
        return Ok(success);
    }

    [HttpDelete("{notificationId}"), Authorize(Permissions.Users.Write)]
    public async Task<ActionResult<bool>> DeleteNotification([FromRoute] string projectId,
        [FromRoute] string notificationId)
    {
        var response = await _notificationService.DeleteNotification(projectId, notificationId);
        if (response) return Ok(true);
        return Problem();
    }

    [HttpPost("deleteall"), Authorize(Permissions.Users.Write)]
    public async Task<ActionResult> DeleteNotifications([FromBody] List<Notification> notifications)
    {
        var response = await _notificationService.DeleteNotifications(notifications);
        if (response) return Ok(true);
        return Problem();
    }

}