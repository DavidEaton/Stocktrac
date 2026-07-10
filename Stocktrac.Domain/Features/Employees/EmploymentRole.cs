using System.ComponentModel.DataAnnotations;

namespace Stocktrac.Domain.Features.Employees;

public enum EmploymentRole
{
    [Display(Name = "Service Advisor")]
    ServiceAdvisor,

    [Display(Name = "Technician")]
    Technician,

    [Display(Name = "Inspector")]
    Inspector
}
