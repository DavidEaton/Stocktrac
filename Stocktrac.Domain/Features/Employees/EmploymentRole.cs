using System.ComponentModel.DataAnnotations;

namespace Stocktrac.Domain.Features.Employees;

public enum EmploymentRole
{
    [Display(Name = "Service Advisor")]
    ServiceAdvisor,

    [Display(Name = "Technician")]
    Technician,

    [Display(Name = "Inspector")]
    Inspector,

    [Display(Name = "Parts Specialist")]
    PartsSpecialist,

    [Display(Name = "Service Manager")]
    ServiceManager,

    [Display(Name = "Service Director")]
    ServiceDirector,

    [Display(Name = "General Manager")]
    GeneralManager,
    
    [Display(Name = "Other")]
    Other

}
