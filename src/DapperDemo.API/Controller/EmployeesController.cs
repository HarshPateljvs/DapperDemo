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
        private readonly IGenericRepository<Employee> _repo;

        public EmployeesController(IGenericRepository<Employee> repo) => _repo = repo;

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _repo.GetAllAsync("sp_GetAllEmployees"));

        [HttpPost]
        public async Task<IActionResult> Add(Employee e)
        {
            await _repo.ExecuteAsync("sp_AddEmployee", e);
            return Ok();
        }
    }
}
