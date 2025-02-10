using System;
using Microsoft.AspNetCore.Mvc;

namespace API.Models
{
    public enum ErrorResponseSeverity
    {
        Success,
        Info,
        Warning,
        Error,
    }
    
    public class ErrorResponse
    {
        public int StatusCode { get; private set; }
        public string ErrorCode { get; private set; }
        public string Severity { get; private set; }
        public string Message { get; private set; }
        public string Stacktrace { get; private set; }

        // defaults to internal server error 500
        private ErrorResponse()
        {
        }

        public static ErrorResponse Exception(Exception exception, bool showStackTrace)
        {
            return new ErrorResponse
            {
                StatusCode = 500,
                ErrorCode = "server/error",
                Message = exception.Message,
                Severity = ErrorResponseSeverity.Error.ToString().ToLower(),
                Stacktrace = showStackTrace ? exception.StackTrace : null,
            };
        }

        public static ActionResult Status(int code)
        {
            return ActionResult(new ErrorResponse
            {
                StatusCode = code,
                ErrorCode = "server/error",
                Message = "An error occoured",
                Severity = ErrorResponseSeverity.Error.ToString().ToLower(),
            });
        }

        public static ActionResult ActionResult(ErrorResponse response)
        {
            return new ObjectResult(response)
            {
                StatusCode = response.StatusCode
            };
        }

        public static ActionResult BadRequest(string message)
        {
            return ActionResult(new ErrorResponse
            {
                StatusCode = 400,
                ErrorCode = "client/invalid-model",
                Severity = ErrorResponseSeverity.Warning.ToString().ToLower(),
                Message = message,
            });
        }

        public static ActionResult NotFound(string message)
        {
            return ActionResult(new ErrorResponse
            {
                StatusCode = 404,
                ErrorCode = "server/ressource-not-found",
                Severity = ErrorResponseSeverity.Warning.ToString().ToLower(),
                Message = message,
            });
        }

        public static ActionResult Forbidden(string message)
        {
            return ActionResult(new ErrorResponse
            {
                StatusCode = 403,
                Message = message,
                Severity = ErrorResponseSeverity.Warning.ToString().ToLower(),
                ErrorCode = "client/invalid-authorization",
            });
        }

        public static ActionResult Conflict(string message)
        {
            return ActionResult(new ErrorResponse
            {
                StatusCode = 409,
                ErrorCode = "server/conflict",
                Severity = ErrorResponseSeverity.Warning.ToString().ToLower(),
                Message = message,
            });
        }

        public static ActionResult InvalidVersionHeader()
        {
            return ActionResult(new ErrorResponse
            {
                StatusCode = 426,
                ErrorCode = "client/update-required",
                Severity = ErrorResponseSeverity.Warning.ToString().ToLower(),
                Message = "The app requires and update",
            });
        }

        public static ActionResult UserNotFound(string message)
        {
            return ActionResult(new ErrorResponse
            {
                StatusCode = 404,
                ErrorCode = "auth/user-not-found",
                Severity = ErrorResponseSeverity.Warning.ToString().ToLower(),
                Message = message,
            });
        }
        
        public static ActionResult InvalidPasswordOrEmail()
        {
            return ActionResult(new ErrorResponse
            {
                StatusCode = 401,
                ErrorCode = "auth/invalid-password",
                Severity = ErrorResponseSeverity.Warning.ToString().ToLower(),
                Message = "Den angivede email eller adgangskode er forkert",
            });
        }
        
        public static ActionResult UserDoesntExist()
        {
            return ActionResult(new ErrorResponse
            {
                StatusCode = 403,
                ErrorCode = "auth/user-doesnt-exist",
                Severity = ErrorResponseSeverity.Warning.ToString().ToLower(),
                Message = "Denne bruger er ikke blevet oprettet",
            });
        }

        public static ActionResult UserEmailAlreadyExists()
        {
            return ActionResult(new ErrorResponse
            {
                StatusCode = 403,
                ErrorCode = "auth/email-already-exists",
                Severity = ErrorResponseSeverity.Warning.ToString().ToLower(),
                Message = "En bruger med den angivede email eksisterer allerede",
            });
        }
        
        public static ActionResult UserPhoneAlreadyExists()
        {
            return ActionResult(new ErrorResponse
            {
                StatusCode = 403,
                ErrorCode = "auth/phone-already-exists",
                Severity = ErrorResponseSeverity.Warning.ToString().ToLower(),
                Message = "En bruger med den angivede telefonnummer eksisterer allerede",
            });
        }
    }
}