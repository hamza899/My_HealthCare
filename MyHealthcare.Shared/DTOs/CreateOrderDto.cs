using System.ComponentModel.DataAnnotations;
using MyHealthcare.Shared.Enums;

namespace MyHealthcare.Shared.DTOs;

public class CreateOrderDto
{
    [Required, MinLength(1)]
    public List<CartItemDto> Items { get; set; } = new();

    [Required]
    public AddressDto DeliveryAddress { get; set; } = new();

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

    public string? PrescriptionId { get; set; }
}
