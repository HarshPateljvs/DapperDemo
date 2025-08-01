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
        private readonly IEmployeeRepository _repo;

        public EmployeesController(IEmployeeRepository repo) => _repo = repo;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _repo.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id) => Ok(await _repo.GetByIdAsync(id));

        [HttpPost]
        public async Task<IActionResult> Add(Employee emp)
        {
            await _repo.AddAsync(emp);
            return Ok();
        }

        [HttpPost("save-with-items")]
        public async Task<IActionResult> SaveWithItems(Employee emp, List<EmployeeItem> items)
        {
            var success = await _repo.SaveWithTransactionAsync(emp, items);
            return success ? Ok() : StatusCode(500, "Failed to save");
        }
    }

}
