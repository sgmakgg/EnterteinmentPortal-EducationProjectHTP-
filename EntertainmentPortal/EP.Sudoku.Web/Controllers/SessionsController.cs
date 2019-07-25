﻿using System.Net;
using System.Threading.Tasks;
using EP.Sudoku.Logic.Commands;
using EP.Sudoku.Logic.Models;
using EP.Sudoku.Logic.Queries;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NJsonSchema.Annotations;
using NSwag.Annotations;

namespace EP.Sudoku.Web.Controllers
{
    /// <summary>
    /// Here are CRUD operations that touch upon the game itself.
    /// </summary>
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SessionsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SessionsController> _logger;

        /// <summary>
        /// Is used for DI usage.
        /// </summary>
        public SessionsController(IMediator mediator, ILogger<SessionsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Fetches a gamesession from the Db by the unique Id.
        /// </summary>        
        [HttpGet("api/sessions/{id}")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(Session), Description = "Success")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(void), Description = "Session not found")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(void), Description = "Invalid data")]
        public async Task<IActionResult> GetSessionByIdAsync(long id)
        {
            if (id <= 0)
            {
                _logger.LogError($"Incorrect value for the sessions's Id was set. '{id}' - is <= 0...");
                return BadRequest();
            }
            var result = await _mediator.Send(new GetSessionById(id));

            return result != null ? (IActionResult)Ok(result.Value) : NotFound();
        }

        /// <summary>
        /// Creates a new gamesession due to the set parametrs and saves it in the Db.
        /// </summary>
        [HttpPost("api/sessions")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(Session), Description = "Success")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(void), Description = "Invalid data")]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionCommand model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            model.UserId = User.FindFirst("sub")?.Value;
            var result = await _mediator.Send(model);

            return result.IsFailure ? (IActionResult)BadRequest(result.Error) : Ok(result.Value);
        }

        /// <summary>
        /// Changes the value of a cell durring a gamesession and saves it in the Db.
        /// </summary>
        [HttpPut("api/setCellValue")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(Session), Description = "Success")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(void), Description = "Invalid data")]
        public async Task<IActionResult> SetCellValue([FromBody, NotNull, 
            CustomizeValidator(RuleSet = "PreValidationCell")]SetCellValueCommand model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Incorrect value for the cell's Value was set up...");
                return BadRequest();
            }

            var result = await _mediator.Send(model);

            return result.IsFailure ? (IActionResult)BadRequest(result.Error) : Ok(result.Value);
        }

        /// <summary>    
        /// Changes the value of a cell after each of three possibilies to get automatically set values durring the game as prompts and saves it in the Db.
        /// </summary>
        [HttpPut("api/getHint")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(Session), Description = "Success")]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(void), Description = "Invalid data")]
        public async Task<IActionResult> GetHint([FromBody, NotNull,
            CustomizeValidator(RuleSet = "PreValidationGetHint")]GetHintCommand model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Incorrect value for the cell's Value was set up...");
                return BadRequest();
            }

            var result = await _mediator.Send(model);

            return result.IsFailure ? (IActionResult)BadRequest(result.Error) : Ok(result.Value);
        }
    }
}
