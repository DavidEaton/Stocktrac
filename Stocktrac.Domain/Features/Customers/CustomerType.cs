using System.ComponentModel.DataAnnotations;

namespace Stocktrac.Domain.Features.Customers;

public enum CustomerType
{
    Retail,
    Business,
    Fleet,
    [Display(Name = "Billing Center")]
    BillingCenter,
    [Display(Name = "Billing Center - Prepaid")]
    BillingCenterPrepaid,
    Employee
}