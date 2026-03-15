using System.ComponentModel.DataAnnotations;

namespace PromoCodeFactory.WebHost.Models;

public record RoleCreateRequest(
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
    string Name,

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
    string Description);
