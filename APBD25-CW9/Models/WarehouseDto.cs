using Microsoft.Build.Framework;

namespace APBD25_CW9.Models;

public class WarehouseDto
{
    [Required]
    public int IdProduct { get; set; }
    [Required]
    public int IdWarehouse { get; set; }
    [Required]
    public int Amount { get; set; }
    [Required]
    public DateTime CreatedAt  { get; set; }
}