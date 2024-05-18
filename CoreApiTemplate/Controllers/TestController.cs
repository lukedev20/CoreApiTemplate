using CoreApiTemplate.Intergrations.Persistance;
using CoreApiTemplate.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoreApiTemplate.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController(IDataContextFactory dataContextFactory) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var dataContext = dataContextFactory.Get<Test>();

        var data = await dataContext.GetAllEntriesAsync();

        return Ok(data);
    }
}