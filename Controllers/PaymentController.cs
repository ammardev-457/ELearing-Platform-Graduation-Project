using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ELProject.DataAccess.Repositories.Interfaces;
using ELProject.Domain.Enums;
using ELProject.Domain.Models;
using ELProject.ExternalServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ELProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController(PaymobService paymobService, IUnitOfWork unitOfWork, IConfiguration configuration, UserManager<ApplicationUser> userManager) : ControllerBase
    {
        private readonly PaymobService _paymobService = paymobService;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IConfiguration _configuration = configuration;

        [HttpPost("create-intent/{courseId}")]
        public async Task<IActionResult> CreatePaymentIntetion(int courseId)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized(new { message = "Invalid User Identity" });


            var student = await _userManager.FindByIdAsync(userId);
            if (student == null) return Unauthorized("User Not Authenticated");


            var existingEnrollment = await _unitOfWork.Enrollments.ExistsAsync(userId, courseId);
            if (existingEnrollment != null)
                return BadRequest("Already enrolled in this course.");

            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null) return NotFound("Course not found.");

            var order = new Order
            {
                StudentId = userId,
                CourseId = course.Id,
                Amount = (long)Math.Round(course.Price * 100),
                Status = Domain.Enums.PaymentStatus.Pending.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.CompleteAsync();

            var clientSecret = await _paymobService.CreatePaymentIntentAsync(order, student);



            return Ok(new { client_secret = clientSecret });
        }

        [AllowAnonymous]
        [HttpPost("callback")]
        public async Task<IActionResult> HandleWebhook([FromQuery] string hmac, [FromBody] JsonElement payload)
        {
            try
            {
                if (!payload.TryGetProperty("obj", out var obj)) return Ok();

                string hmacSecret = _configuration["Paymob:HmacSecret"]!;
                string calculatedHmac = CalculateHmac(obj, hmacSecret);

                if (calculatedHmac != hmac)
                {
                    Console.WriteLine("⚠️ HMAC Mismatch - Unauthorized Request!");
                    return Unauthorized();
                }

                string? myOrderIdStr = null;
                if (obj.TryGetProperty("order", out var orderNode))
                {
                    if (orderNode.TryGetProperty("merchant_order_id", out var mId))
                        myOrderIdStr = mId.GetString();
                }

                bool isSuccess = obj.GetProperty("success").GetBoolean();
                long paymobTransactionId = obj.GetProperty("id").GetInt64();

                if (long.TryParse(myOrderIdStr, out long orderId))
                {
                    var order = await _unitOfWork.Orders.GetByIdAsync(orderId);

                    if (order != null && order.Status == Domain.Enums.PaymentStatus.Pending.ToString())
                    {
                        if (isSuccess)
                        {
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

              
                            var existingEnroll = await _unitOfWork.Enrollments.ExistsAsync(order.StudentId, order.CourseId);

                            if (existingEnroll == null)
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
                            Console.WriteLine($"DONE: Order {orderId} Paid & Student Enrolled.");
                        }
                        else
                        {
                            order.Status = "Failed";
                            _unitOfWork.Orders.Update(order);
                            await _unitOfWork.CompleteAsync();
                        }
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Webhook: {ex.Message}");
                return Ok(); 
            }
        }

        [AllowAnonymous]
        [HttpGet("status")]
        public IActionResult PaymentStatus([FromQuery] string success)
        {
            if (success?.ToLower() == "true")
            {
                return Ok(new
                {
                    Status = "Success",
                    Message = "Course purchased successfully! You can start watching now."
                });
            }

            return BadRequest(new
            {
                Status = "Failed",
                Message = "Payment failed. Please try again or contact support."
            });
        }



        private string CalculateHmac(JsonElement obj, string hmacSecret)
        {
            string GetVal(JsonElement element, string prop)
            {
                if (!element.TryGetProperty(prop, out var val)) return "";

                return val.ValueKind switch
                {
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    JsonValueKind.String => val.GetString() ?? "",
                    JsonValueKind.Number => val.ToString(),
                    _ => ""
                };
            }

            // الخطوة 1 و 2: التجميع حسب القائمة الـ 21 بالترتيب الأبجدي
            StringBuilder sb = new StringBuilder();

            sb.Append(GetVal(obj, "amount_cents"));
            sb.Append(GetVal(obj, "created_at"));
            sb.Append(GetVal(obj, "currency"));
            sb.Append(GetVal(obj, "error_occured"));
            sb.Append(GetVal(obj, "has_parent_transaction"));
            sb.Append(GetVal(obj, "id")); // هذا هو obj.id في الـ POST
            sb.Append(GetVal(obj, "integration_id"));
            sb.Append(GetVal(obj, "is_3d_secure"));
            sb.Append(GetVal(obj, "is_auth"));
            sb.Append(GetVal(obj, "is_capture"));
            sb.Append(GetVal(obj, "is_refunded"));
            sb.Append(GetVal(obj, "is_standalone_payment"));
            sb.Append(GetVal(obj, "is_voided"));


            if (obj.TryGetProperty("order", out var order))
                sb.Append(GetVal(order, "id")); // هذا هو order.id

            sb.Append(GetVal(obj, "owner"));
            sb.Append(GetVal(obj, "pending"));

            if (obj.TryGetProperty("source_data", out var sd))
            {
                sb.Append(GetVal(sd, "pan"));
                sb.Append(GetVal(sd, "sub_type"));
                sb.Append(GetVal(sd, "type"));
            }

            sb.Append(GetVal(obj, "success"));

            string concatenatedString = sb.ToString();

            Console.WriteLine("Concatenated String for HMAC: " + concatenatedString);

            var keyBytes = Encoding.UTF8.GetBytes(hmacSecret);
            using var hmacSha512 = new HMACSHA512(keyBytes);
            var hashBytes = hmacSha512.ComputeHash(Encoding.UTF8.GetBytes(concatenatedString));

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}