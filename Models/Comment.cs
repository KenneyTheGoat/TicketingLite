using System.ComponentModel.DataAnnotations;

namespace TicketingLite.Models;

public class Comment
{
    public int Id { get; set; }
    public int TicketId { get; set; }

    [Required, StringLength(2000)]
    public string Body { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public string AuthorUserId { get; set; } = "";

    public Ticket? Ticket { get; set; }
}
