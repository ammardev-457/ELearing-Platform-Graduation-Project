using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ELProject.DataAccess.Repositories.Interfaces;
using ELProject.Domain.Models;
using ELProject.ExternalServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ELProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly PaymobService _paymobService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentController(
            PaymobService paymobService,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager)
        {
            _paymobService = paymobService;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _userManager = userManager;
        }

        // =========================
        // CREATE PAYMENT INTENT
        // =========================
        [HttpPost("create-intent/{courseId}")]
        public async Task<IActionResult> CreatePaymentIntent(int courseId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid User Identity");

            var student = await _userManager.FindByIdAsync(userId);
            if (student == null)
                return Unauthorized("User Not Authenticated");

            var alreadyEnrolled = await _unitOfWork.Enrollments.IsFoundAsync(userId, courseId);
            if (alreadyEnrolled)
                return BadRequest("Already enrolled in this course.");

            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null)
                return NotFound("Course not found.");

            var order = new Order
            {
                StudentId = userId,
                CourseId = course.Id,
                PaymentRefernce = Guid.NewGuid().ToString(), // 🔥 MAIN LINK KEY
                Amount = (long)Math.Round(course.Price * 100),
                Status = Domain.Enums.PaymentStatus.Pending.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.CompleteAsync();

            // 🔥 IMPORTANT: Paymob mapping uses PaymentReference
            var clientSecret = await _paymobService.CreatePaymentIntentAsync(order, student);

            return Ok(new { client_secret = clientSecret });
        }

        // =========================
        // WEBHOOK CALLBACK
        // =========================
        [AllowAnonymous]
        [HttpPost("callback")]
        public async Task<IActionResult> HandleWebhook([FromQuery] string hmac, [FromBody] JsonElement payload)
        {
            try
            {
                if (!payload.TryGetProperty("obj", out var obj))
                    return Ok();

                var hmacSecret = _configuration["Paymob:HmacSecret"]!;
                var calculatedHmac = await _paymobService.CalculateHmac(obj, hmacSecret);

                if (!string.Equals(calculatedHmac, hmac, StringComparison.OrdinalIgnoreCase))
                    return Unauthorized();

                // =========================
                // GET PAYMENT REFERENCE
                // =========================
                string? paymentReference = null;

                if (obj.TryGetProperty("order", out var orderNode))
                {
                    if (orderNode.TryGetProperty("merchant_order_id", out var mId))
                        paymentReference = mId.GetString();
                }

                if (string.IsNullOrEmpty(paymentReference))
                    return Ok();

                var isSuccess = obj.GetProperty("success").GetBoolean();
                var paymobTransactionId = obj.GetProperty("id").GetInt64();

                // =========================
                // FIND ORDER BY REFERENCE
                // =========================
                var order = await _unitOfWork.Orders
                    .FindOrderByPaymentReferenceAsync(paymentReference);

                if (order == null)
                    return Ok();

                // =========================
                // IDPOTENT PROTECTION
                // =========================
                if (order.Status != Domain.Enums.PaymentStatus.Pending.ToString())
                    return Ok();

                if (!isSuccess)
                {
                    order.Status = "Failed";
                    order.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Orders.Update(order);

                    await _unitOfWork.CompleteAsync();
                    return Ok();
                }

                // =========================
                // SUCCESS FLOW
                // =========================
                order.Status = Domain.Enums.PaymentStatus.Success.ToString();
                order.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Orders.Update(order);

                await _unitOfWork.Transactions.AddAsync(new Transaction
                {
                    OrderId = order.Id,
                    TransactionId = paymobTransactionId.ToString(),
                    Amount = obj.GetProperty("amount_cents").GetInt64(),
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow
                });

                var alreadyEnrolledFinal =
                    await _unitOfWork.Enrollments.IsFoundAsync(order.StudentId, order.CourseId);

                if (!alreadyEnrolledFinal)
                {
                    await _unitOfWork.Enrollments.AddAsync(new Enrollment
                    {
                        StudentId = order.StudentId,
                        CourseId = order.CourseId,
                        OrderId = order.Id,
                        EnrollDate = DateTime.UtcNow,
                        Progress = 0,
                        IsCompleted = false
                    });
                }

                await _unitOfWork.CompleteAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, ex.StackTrace);
                return StatusCode(500);
            }
        }

        // =========================
        // PAYMENT STATUS (FRONTEND)
        // =========================
        [AllowAnonymous]
        [HttpGet("status")]
        public IActionResult PaymentStatus([FromQuery] string success)
        {
            if (success?.ToLower() == "true")
            {
                return Ok(new
                {
                    Status = "Success",
                    Message = "Course purchased successfully!"
                });
            }

            return BadRequest(new
            {
                Status = "Failed",
                Message = "Payment failed."
            });
        }

    }
}