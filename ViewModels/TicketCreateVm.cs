using System.ComponentModel.DataAnnotations;
using TicketingLite.Models;

namespace TicketingLite.ViewModels;

public class TicketCreateVm
{
    [Required, StringLength(100)]
    public string Title { get; set; } = "";

    [Required, StringLength(2000)]
    public string Description { get; set; } = "";

    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
}
