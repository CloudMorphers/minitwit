using Microsoft.AspNetCore.Mvc;

namespace MiniTwit.Controllers
{
    [ApiController]
    public class LatestController : Controller
    {
        private static int _latest = 0;

        // Get the latest update value
        [HttpGet("/latest")]
        public IActionResult GetLatest()
        {
            return Ok(new { latest = _latest }); // Returns HTTP 200 OK with the latest value
        }

        // Update the latest value (used internally when posting messages or following/unfollowing)
        public static void UpdateLatest(int newValue)
        {
            if (newValue > _latest)
            {
                _latest = newValue;
            }
        }
    }
}
