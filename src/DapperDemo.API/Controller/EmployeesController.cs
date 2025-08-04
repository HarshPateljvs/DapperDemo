using DapperDemo.DAL.Interface;
using DapperDemo.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DapperDemo.API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeRepository _service;

        public EmployeesController(IEmployeeRepository repo) => _service = repo;

        [HttpPost]
        public async Task<IActionResult> Create(EmployeeRequest request)
        {
            var success = await _service.CreateEmployeeWithItemsAsync(request.Employee, request.Items);
            return success ? Ok("Employee and items saved.") : StatusCode(500, "Something went wrong.");
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllEmployeesAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetEmployeeByIdAsync(id);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpGet("single/{id}")]
        public async Task<IActionResult> GetSingle(int id)
        {
            var result = await _service.GetEmployeeSingleAsync(id);
            return Ok(result);
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            var count = await _service.GetEmployeeCountAsync();
            return Ok(new { Count = count });
        }
    }
}
public class EmployeeRequest
{
    public Employee Employee { get; set; } = new();
    public List<EmployeeItem> Items { get; set; } = new();
}