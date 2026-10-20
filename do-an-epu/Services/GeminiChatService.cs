using System.Text;
using System.Text.Json;
using do_an_epu.DTOs.Chat;
using do_an_epu.Services.Interface;

namespace do_an_epu.Services
{
    public class GeminiChatService : IChatService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        // Dùng model mới nhất từ danh sách bạn đã check
        private readonly string _model = "gemini-2.5-flash"; 

        public GeminiChatService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _apiKey = _configuration["Gemini:ApiKey"];
        }

        public async Task<string> GetResponseAsync(string message, List<ChatMessageDto> history)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return "Lỗi: Chưa cấu hình API Key trong appsettings.json";
            }

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            try
            {
                // 1. Chuẩn bị System Prompt
                var systemPromptText = @"""Bạn là Trợ lý ảo AI của hệ thống đặt phòng Golden Lotus. Nhiệm vụ của bạn là tư vấn khách sạn phù hợp nhất dựa trên vị trí, ngân sách và nhu cầu của khách hàng. Dưới đây là cơ sở dữ liệu về 5 khách sạn thuộc hệ thống:""

1. KHU VỰC MIỀN BẮC
Golden Lotus Luxury Hotel Hà Nội (4 Sao - Cao cấp)

Vị trí: 53-55 Hàng Trống, Q. Hoàn Kiếm, ngay trung tâm Phố Cổ, cách Hồ Gươm và Nhà thờ Lớn vài bước chân.

Phong cách: Boutique sang trọng, giao thoa cổ điển và hiện đại.

Tiện ích nổi bật: Bể bơi bốn mùa trên tầng mái, nhà hàng cao cấp, dịch vụ Spa.

Phù hợp: Khách quốc tế, khách công tác cao cấp, thích trải nghiệm văn hóa Phố Cổ.

Khách sạn Sen Vàng Hạ Long (3 Sao - Tiêu chuẩn)

Vị trí: Đối diện cổng công viên Sun World, Bãi Cháy. Đi bộ ra biển rất gần.

Tiện ích nổi bật: View hướng vịnh/hướng phố, buffet sáng miễn phí, nhà hàng hải sản.

Mức giá: 550.000đ - 700.000đ/đêm (Tiết kiệm).

Phù hợp: Khách du lịch gia đình, khách tự túc muốn ở trung tâm vui chơi ăn uống.

2. KHU VỰC MIỀN TRUNG (ĐÀ NẴNG)
Golden Lotus Luxury Đà Nẵng (4 Sao - Cao cấp)

Vị trí: 198B Trần Bạch Đằng (Khu phố Tây An Thượng), sát biển Mỹ Khê.

Quy mô: 16 tầng, 94 phòng có ban công view biển/thành phố.

Tiện ích nổi bật: Hồ bơi vô cực trên tầng thượng (Rooftop pool), Sky Bar, miễn phí Tiệc trà chiều (Afternoon Tea).

Phù hợp: Nghỉ dưỡng sang chảnh, các cặp đôi, thích dịch vụ đẳng cấp.

Khách sạn Sen Vàng Đà Nẵng (2 Sao - Tiết kiệm)

Vị trí: Đường Hồ Nghinh, đi bộ ra bãi biển Phạm Văn Đồng/Mỹ Khê.

Quy mô: 8 tầng, 24 phòng gọn gàng, có sân thượng ngắm cảnh.

Phù hợp: Khách du lịch bụi, tự túc, chú trọng vị trí trung tâm ẩm thực và chi phí thấp.

3. KHU VỰC MIỀN NAM
Khách sạn Sen Vàng Hồ Chí Minh (2 Sao - Tiết kiệm)

Vị trí: Đường Sư Vạn Hạnh, Quận 10 (đối diện Vạn Hạnh Mall), khu trung tâm mua sắm, giải trí sôi động.

Quy mô: 7 tầng, phòng ốc hiện đại, sạch sẽ.

Tiện ích: Lễ tân trực 24/24. An ninh tốt.

Phù hợp: Khách công tác, giới trẻ khám phá Sài Gòn về đêm với ngân sách hợp lý.";

                // 2. Xử lý lịch sử chat
                var contentsList = new List<object>();

                if (history != null && history.Any())
                {
                    foreach (var chat in history)
                    {
                        contentsList.Add(new
                        {
                            role = chat.Role.ToLower() == "user" ? "user" : "model",
                            parts = new[] { new { text = chat.Content } }
                        });
                    }
                }

                // Thêm tin nhắn mới nhất
                contentsList.Add(new
                {
                    role = "user",
                    parts = new[] { new { text = message } }
                });

                // 3. Tạo Payload (SỬA LỖI TẠI ĐÂY)
                // Chúng ta dùng PascalCase (Chữ hoa đầu) cho C# Object, 
                // sau đó JsonSerializerOptions sẽ tự đổi thành camelCase (chữ thường đầu) chuẩn Google.
                var requestBody = new
                {
                    SystemInstruction = new // Đã đổi từ snake_case sang PascalCase
                    {
                        Parts = new[] { new { Text = systemPromptText } }
                    },
                    Contents = contentsList,
                    GenerationConfig = new
                    {
                        Temperature = 0.7,
                        MaxOutputTokens = 800
                    }
                };

                // 4. Serialize với Policy CamelCase (Quan trọng)
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Tự động đổi SystemInstruction -> systemInstruction
                    WriteIndented = false
                };
                
                var jsonContent = JsonSerializer.Serialize(requestBody, jsonOptions);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // 5. Gửi Request
                var response = await _httpClient.PostAsync(url, httpContent);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"API ERROR: {response.StatusCode} - {responseString}");
                    return $"Lỗi kết nối AI: {response.StatusCode}";
                }

                using var doc = JsonDocument.Parse(responseString);
                var root = doc.RootElement;

                if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    if (firstCandidate.TryGetProperty("content", out var content) &&
                        content.TryGetProperty("parts", out var parts) &&
                        parts.GetArrayLength() > 0)
                    {
                        return parts[0].GetProperty("text").GetString();
                    }
                }

                return "Xin lỗi, tôi không tìm thấy câu trả lời.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return "Lỗi hệ thống xử lý tin nhắn.";
            }
        }
    }
}