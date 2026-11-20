using ExpenseTracker.Application.DTOs.Chatbot;
using ExpenseTracker.Application.DTOs.Common;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController(
    IChatService chatService,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!currentUserService.UserId.HasValue)
            return Unauthorized();

        var result = await chatService.GetSessionsAsync(
            currentUserService.UserId.Value,
            page,
            pageSize,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(
                ApiResponseDto<PagedResultDto<ChatSessionDto>>.Fail(
                    "Unable to retrieve chat sessions."));
        }

        return Ok(
            ApiResponseDto<PagedResultDto<ChatSessionDto>>.Ok(
                result.Data!));
    }

    [HttpPost("sessions")]
    public async Task<IActionResult> CreateSession(
        [FromBody] CreateChatSessionRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
            return Unauthorized();

        var result = await chatService.CreateSessionAsync(
            currentUserService.UserId.Value,
            request,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(
                ApiResponseDto<ChatSessionDto>.Fail(
                    result.Error ?? "Unable to create chat session."));
        }

        return Created(
            $"/api/chat/sessions/{result.Data!.Id}/messages",
            ApiResponseDto<ChatSessionDto>.Ok(result.Data));
    }

    [HttpGet("sessions/{sessionId:int}/messages")]
    public async Task<IActionResult> GetMessages(
        int sessionId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30,
        CancellationToken cancellationToken = default)
    {
        if (!currentUserService.UserId.HasValue)
            return Unauthorized();

        var result = await chatService.GetMessagesAsync(
            currentUserService.UserId.Value,
            sessionId,
            page,
            pageSize,
            cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error == "Chat session not found.")
            {
                return NotFound(
                    ApiResponseDto<PagedResultDto<ChatMessageDto>>.Fail(
                        "Chat session not found."));
            }

            return BadRequest(
                ApiResponseDto<PagedResultDto<ChatMessageDto>>.Fail(
                    "Unable to retrieve chat messages."));
        }

        return Ok(
            ApiResponseDto<PagedResultDto<ChatMessageDto>>.Ok(
                result.Data!));
    }

    [HttpPost("sessions/{sessionId:int}/messages")]
    public async Task<IActionResult> AddUserMessage(
        int sessionId,
        [FromBody] SendChatMessageRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
            return Unauthorized();

        var result = await chatService.AddUserMessageAsync(
            currentUserService.UserId.Value,
            sessionId,
            request,
            cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error == "Chat session not found.")
            {
                return NotFound(
                    ApiResponseDto<ChatMessageDto>.Fail(
                        "Chat session not found."));
            }

            return BadRequest(
                ApiResponseDto<ChatMessageDto>.Fail(
                    result.Error ?? "Unable to save chat message."));
        }

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponseDto<ChatMessageDto>.Ok(result.Data));
    }
}
