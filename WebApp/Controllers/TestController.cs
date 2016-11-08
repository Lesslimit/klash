using Microsoft.AspNetCore.Mvc;
 
public class TestController : Controller
{
    [Route("test")]
    public IActionResult Index()
    {
        return Ok("Test Controller");
    }
}