using System.ComponentModel.DataAnnotations;
using TicketingLite.Models;

namespace TicketingLite.ViewModels;

public class TicketManageVm
{
    public int Id { get; set; }

    [Required]
    public TicketStatus Status { get; set; }

    public string? AssignedAgentUserId { get; set; }
}
