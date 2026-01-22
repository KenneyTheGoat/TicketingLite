using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketingLite.Data;
using TicketingLite.Models;
using TicketingLite.ViewModels;

namespace TicketingLite.Controllers;

[Authorize]
public class TicketsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userMgr;

    public TicketsController(ApplicationDbContext db, UserManager<IdentityUser> userMgr)
    {
        _db = db;
        _userMgr = userMgr;
    }

    private bool IsStaff() => User.IsInRole("Admin") || User.IsInRole("Agent");

    // All tickets for staff; own tickets for clients
    public async Task<IActionResult> Index(string? q, string? status)
    {
        var user = await _userMgr.GetUserAsync(User);
        var isStaff = IsStaff();

        var query = _db.Tickets.AsNoTracking().AsQueryable();

        if (!isStaff)
            query = query.Where(t => t.ClientUserId == user!.Id);

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(t => t.Title.Contains(q) || t.Description.Contains(q));

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TicketStatus>(status, out var st))
            query = query.Where(t => t.Status == st);

        var list = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();

        ViewBag.Q = q ?? "";
        ViewBag.Status = status ?? "";
        ViewBag.IsStaff = isStaff;

        return View(list);
    }

    // Client-focused: always only my tickets
    public async Task<IActionResult> My(string? q, string? status)
    {
        var user = await _userMgr.GetUserAsync(User);

        var query = _db.Tickets.AsNoTracking()
            .Where(t => t.ClientUserId == user!.Id);

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(t => t.Title.Contains(q) || t.Description.Contains(q));

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TicketStatus>(status, out var st))
            query = query.Where(t => t.Status == st);

        var list = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();

        ViewBag.Q = q ?? "";
        ViewBag.Status = status ?? "";
        ViewBag.IsStaff = false;
        ViewBag.ViewName = "My Tickets";

        return View("Index", list);
    }

    // Agent-focused: only tickets assigned to me
    [Authorize(Roles = "Admin,Agent")]
    public async Task<IActionResult> MyWork(string? q, string? status)
    {
        var user = await _userMgr.GetUserAsync(User);

        var query = _db.Tickets.AsNoTracking()
            .Where(t => t.AssignedAgentUserId == user!.Id);

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(t => t.Title.Contains(q) || t.Description.Contains(q));

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TicketStatus>(status, out var st))
            query = query.Where(t => t.Status == st);

        var list = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();

        ViewBag.Q = q ?? "";
        ViewBag.Status = status ?? "";
        ViewBag.IsStaff = true;
        ViewBag.ViewName = "My Work";

        return View("Index", list);
    }

    public IActionResult Create() => View(new TicketCreateVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TicketCreateVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _userMgr.GetUserAsync(User);

        var ticket = new Ticket
        {
            Title = vm.Title.Trim(),
            Description = vm.Description.Trim(),
            Priority = vm.Priority,
            Status = TicketStatus.Open,
            CreatedAt = DateTime.UtcNow,
            ClientUserId = user!.Id
        };

        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Ticket created.";
        return RedirectToAction(nameof(Details), new { id = ticket.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var ticket = await _db.Tickets
            .Include(t => t.Comments.OrderByDescending(c => c.CreatedAt))
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null) return NotFound();

        var user = await _userMgr.GetUserAsync(User);
        var isStaff = IsStaff();

        if (!isStaff && ticket.ClientUserId != user!.Id)
            return Forbid();

        ViewBag.IsStaff = isStaff;
        ViewBag.CurrentUserId = user!.Id;

        string? assignedEmail = null;
        if (!string.IsNullOrWhiteSpace(ticket.AssignedAgentUserId))
        {
            var assigned = await _userMgr.FindByIdAsync(ticket.AssignedAgentUserId);
            assignedEmail = assigned?.Email;
        }
        ViewBag.AssignedAgentEmail = assignedEmail;

        return View(ticket);
    }

    [Authorize(Roles = "Admin,Agent")]
    public async Task<IActionResult> Manage(int id)
    {
        var ticket = await _db.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        if (ticket == null) return NotFound();

        var agents = await _userMgr.GetUsersInRoleAsync("Agent");
        ViewBag.Agents = agents.Select(a => new { a.Id, a.Email }).ToList();

        var vm = new TicketManageVm
        {
            Id = ticket.Id,
            Status = ticket.Status,
            AssignedAgentUserId = ticket.AssignedAgentUserId
        };

        return View(vm);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Agent")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Manage(TicketManageVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == vm.Id);
        if (ticket == null) return NotFound();

        ticket.Status = vm.Status;
        ticket.AssignedAgentUserId = string.IsNullOrWhiteSpace(vm.AssignedAgentUserId) ? null : vm.AssignedAgentUserId;

        await _db.SaveChangesAsync();

        TempData["Success"] = "Ticket updated.";
        return RedirectToAction(nameof(Details), new { id = vm.Id });
    }
}
