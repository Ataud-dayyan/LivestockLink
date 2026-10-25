namespace LivestockLink.Core.Enums;

public enum OrderStatus
{
    Submitted,   // Order requested by restaurant, awaiting supplier review
    Accepted,    // Supplier accepted the order
    Rejected,    // Supplier rejected the order
    Dispatched,  // Order is on the road / dispatched by supplier
    Completed,   // Delivery confirmed by restaurant, commission calculated
    Cancelled,   // Cancelled by restaurant before acceptance, or by admin
    Disputed     // Delivery disputed by restaurant, awaiting resolution
}
