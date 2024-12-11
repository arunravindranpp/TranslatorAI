using Microsoft.AspNetCore.Mvc;
using TranslatorAPI.Services;

namespace TranslatorAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeService _employeeService;

        public EmployeeController(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }
        [HttpGet("SearchEmployees")]
        public async Task<IActionResult> SearchEmployees(string search)
        {
            var result =await _employeeService.SearchEmployees(search);
            return Ok(result);
        }
    }
}
