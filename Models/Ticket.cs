using System.ComponentModel.DataAnnotations;

namespace TicketingLite.Models;

public enum TicketStatus { Open, InProgress, Resolved }
public enum TicketPriority { Low, Medium, High }

public class Ticket
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Title { get; set; } = "";

    [Required, StringLength(2000)]
    public string Description { get; set; } = "";

    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public string ClientUserId { get; set; } = "";

    public string? AssignedAgentUserId { get; set; }

    public List<Comment> Comments { get; set; } = new();
}
