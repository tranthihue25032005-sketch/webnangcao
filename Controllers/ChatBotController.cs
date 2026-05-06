using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System;
using FashionStoreAdmin.Data; // Đảm bảo dòng này đúng với namespace trong file DbContext của bạn

namespace FashionStoreAdmin.Controllers
{
    [ApiController]
    [Route("chatbot")]
    public class ChatBotController : ControllerBase
    {
        private readonly ClientOrdersDbContext _context;

        public ChatBotController(ClientOrdersDbContext context)
        {
            _context = context;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] ChatRequest req)
        {
            var msg = req.message.ToLower().Trim();

            // 1. Kiểm tra sản phẩm
            if (msg.Contains("áo") || msg.Contains("quần") || msg.Contains("shirt") || msg.Contains("jean"))
            {
                var products = _context.Products
                    .Where(p => p.IsActive && p.Name.ToLower().Contains(msg))
                    .Take(5)
                    .Select(p => new { p.Name, p.Price })
                    .ToList();

                if (products.Count > 0)
                {
                    var productText = string.Join("\n", products.Select(p => $"- {p.Name} ({p.Price}đ)"));
                    var aiReply = await AskGemini($@"User hỏi: {req.message}. Danh sách sản phẩm: {productText}. Trả lời như nhân viên bán hàng.");
                    return Ok(new { reply = aiReply });
                }
            }

            // 2. Trả lời bình thường
            var normalReply = await AskGemini(req.message);
            return Ok(new { reply = normalReply });
        }

        private async Task<string> AskGemini(string prompt)
        {
            var apiKey = "AIzaSyB6lC8W2HoSy2fRv6Wu906bAxM1Tac4u48"; 
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemma-3-4b-it:generateContent?key={apiKey}";

            using var client = new HttpClient();
            var body = new { contents = new[] { new { parts = new[] { new { text = "Bạn là chatbot bán hàng thời trang. Trả lời bằng tiếng Việt. Câu hỏi: " + prompt } } } } };
            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            try 
            {
                var res = await client.PostAsync(url, content);
                var json = await res.Content.ReadAsStringAsync();
                if (!res.IsSuccessStatusCode) return "AI đang bận, bạn thử lại sau nhé!";
                
                dynamic result = JsonConvert.DeserializeObject(json);
                return result.candidates[0].content.parts[0].text;
            }
            catch { return "Lỗi hệ thống!"; }
        }
    }

    // LỚP NÀY PHẢI NẰM Ở ĐÂY (Trong cùng Namespace)
    public class ChatRequest
    {
        public string message { get; set; }
    }
}