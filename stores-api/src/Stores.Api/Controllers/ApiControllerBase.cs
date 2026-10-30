using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Stores.Application.Common;

namespace Stores.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult Problem(Error error)
    {
        if (error.Type == ErrorType.Validation)
        {
            var errors = error.ValidationErrors!.ToDictionary(e => ToJsonPath(e.Key), e => e.Value);
            return ValidationProblem(new ValidationProblemDetails(errors));
        }

        var status = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        var problem = ProblemDetailsFactory.CreateProblemDetails(HttpContext, status, detail: error.Message);
        problem.Extensions["code"] = error.Code;

        return new ObjectResult(problem) { StatusCode = status, ContentTypes = { "application/problem+json" } };
    }

    private static string ToJsonPath(string propertyPath) =>
        string.Join('.', propertyPath.Split('.').Select(JsonNamingPolicy.CamelCase.ConvertName));
}
