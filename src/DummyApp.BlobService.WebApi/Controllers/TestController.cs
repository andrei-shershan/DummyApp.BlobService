using Microsoft.AspNetCore.Mvc;

namespace DummyApp.BlobService.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("hello world");
    }
}
