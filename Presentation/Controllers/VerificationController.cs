
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;
using Presentation.Services;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerificationController(IVerificationService verificationService) : ControllerBase
    {
        private readonly IVerificationService _verificationService = verificationService;

        [HttpPost("send")]
        public async Task<IActionResult> send(SendCodeRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Error = "Email adress is required" });

           var result = await _verificationService.SendCodeAsync(request);
            return result.Success
                ? Ok(result)
                : StatusCode(500, result);
        }

        [HttpPost("verify")]
        public IActionResult Verify(VerifyCodeRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Error = "Invalid Code" });

            var result = _verificationService.VerifyCode(request);
            return result.Success
                ? Ok(result)
                : StatusCode(500, result);
        }
    }
}
