using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketingLite.Data;
using TicketingLite.Models;

namespace TicketingLite.Controllers;

[ApiController]
[Route("api/tickets/{ticketId:int}/comments")]
public class TicketCommentsApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userMgr;

    public TicketCommentsApiController(ApplicationDbContext db, UserManager<IdentityUser> userMgr)
    {
        _db = db;
        _userMgr = userMgr;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(int ticketId, [FromBody] CreateCommentDto dto)
    {
        if (dto == null) return BadRequest("Body required.");

        var body = (dto.Body ?? "").Trim();
        if (body.Length == 0) return BadRequest("Body required.");
        if (body.Length > 2000) return BadRequest("Comment too long.");

        var user = await _userMgr.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null) return NotFound();

        var isStaff = User.IsInRole("Admin") || User.IsInRole("Agent");
        if (!isStaff && ticket.ClientUserId != user.Id) return Forbid();

        var comment = new Comment
        {
            TicketId = ticketId,
            Body = body,
            AuthorUserId = user.Id,
            CreatedAt = DateTime.UtcNow
        };

        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        return Ok(new { comment.Id, comment.Body, comment.CreatedAt, Author = user.Email });
    }


    public record CreateCommentDto(string Body);
}
