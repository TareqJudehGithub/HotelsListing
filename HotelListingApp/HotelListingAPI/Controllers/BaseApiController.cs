using Microsoft.AspNetCore.Mvc;
using HotelListingAPI.Common.Constants;
using HotelListingAPI.Common.Results;

namespace HotelListingAPI.Controllers;

public abstract class BaseApiController : ControllerBase
{
    // Action result methods
    protected ActionResult<T> ToActionResult<T>(Result<T> result)
        => result.IsSuccess ? Ok(result.Value) : MapErrorsToResponse(result.Errors);
    protected ActionResult ToActionResult(Result result)
        => result.IsSuccess ? NoContent() : MapErrorsToResponse(result.Errors);

    protected ActionResult MapErrorsToResponse(Error[] errors)
    {
        if (errors is null || errors.Length == 0) return Problem(
             statusCode: StatusCodes.Status500InternalServerError,
                title: "An error occurred",
                detail: "No error details provided"
            );
        var e = errors[0];
        var errorDetails = string.Join("; ", errors.Select(x => x.Description));

        return e.Code switch
        {
            ErrorCodes.NotFound => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Resource not found",
                detail: errorDetails
            ),
            ErrorCodes.Validation => ValidationProblem(
                title: "Validation failed",
                detail: errorDetails
            ),
            ErrorCodes.BadRequest => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad request",
                detail: errorDetails
            ),
            ErrorCodes.Conflict => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: ErrorCodes.Conflict,
                detail: errorDetails
            ),
            ErrorCodes.Forbid => Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden",
                detail: errorDetails
            ),
            _ => Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                detail: string.Join("; ", errors.Select(x => x.Description)),
                title: e.Code
            )
        };
        //return e.Code switch
        //{
        //    ErrorCodes.NotFound => NotFound(e.Description),
        //    ErrorCodes.Validation => BadRequest(e.Description),
        //    ErrorCodes.BadRequest => BadRequest(e.Description),
        //    ErrorCodes.Conflict => Conflict(e.Description),
        //    ErrorCodes.Forbid => Forbid(e.Description),
        //    _ => Problem(detail: string.Join("; ", errors.Select(x => x.Description)), title: e.Code)
        //};
    }
}
