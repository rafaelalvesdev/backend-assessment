using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Products.Functions.Common;

namespace Products.Functions.Functions;

internal static class ProblemResults
{
    public static IActionResult From(Error error) => error.Type switch
    {
        ErrorType.Validation => Validation(error.ValidationErrors!),
        ErrorType.NotFound => Problem(StatusCodes.Status404NotFound, "Not Found", error.Message),
        ErrorType.Conflict => Problem(StatusCodes.Status409Conflict, "Conflict", error.Message),
        _ => Problem(StatusCodes.Status400BadRequest, "Bad Request", error.Message)
    };

    public static IActionResult BadRequest(string detail) => Problem(StatusCodes.Status400BadRequest, "Bad Request", detail);

    public static IActionResult Validation(IReadOnlyDictionary<string, string[]> errors)
    {
        var problem = new ValidationProblemDetails(errors.ToDictionary(e => JsonNamingPolicy.CamelCase.ConvertName(e.Key), e => e.Value))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred."
        };

        return Json(problem, StatusCodes.Status400BadRequest);
    }

    private static IActionResult Problem(int status, string title, string detail) =>
        Json(new ProblemDetails { Status = status, Title = title, Detail = detail }, status);

    private static ObjectResult Json(ProblemDetails problem, int status) =>
        new(problem) { StatusCode = status, ContentTypes = { "application/problem+json" } };
}
