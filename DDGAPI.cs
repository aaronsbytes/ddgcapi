using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Windows;

namespace DuckDuckGo
{
    internal class ChatAPI
    {
        public class ChatModel
        {
            public string NAME { get; set; } = "";
            public string ID { get; set; } = "";
            public string DESC { get; set; } = "";
            public string CREATOR { get; set; } = "";
        }

        public static class ChatModels
        {
            // Defined available chat models
            // Made this because they all have a unique ID

            public static ChatModel ChatGPT_4o_Mini = new ChatModel
            {
                NAME = "ChatGPT",
                ID = "gpt-4o-mini",
                DESC = "Allzweck-KI mit hoher integrierter Moderation",
                CREATOR = "OpenAI"
            };

            public static ChatModel LLama_3_3_70B = new ChatModel
            {
                NAME = "Llama 3.3",
                ID = "meta-llama/Llama-3.3-70B-Instruct-Turbo",
                DESC = "Allzweck-KI mit mittlerer integrierter Moderation",
                CREATOR = "Facebook / Meta"
            };

            public static ChatModel Claude_3_Haiku = new ChatModel
            {
                NAME = "Claude 3 Haiku",
                ID = "claude-3-haiku-20240307",
                DESC = "Allzweck-KI mit hoher integrierter Moderation",
                CREATOR = "Anthropic"
            };

            public static ChatModel ChatGPT_o3_Mini = new ChatModel
            {
                NAME = "ChatGPT o3 Mini",
                ID = "o3-mini",
                DESC = "Argumentierende KI mit hoher eingebauter Moderation",
                CREATOR = "OpenAI"
            };

            public static ChatModel Mistral_8x7B = new ChatModel
            {
                NAME = "Mistral 8x7B",
                ID = "mistralai/Mixtral-8x7B-Instruct-v0.1",
                DESC = "Allzweck-KI mit geringer integrierter Moderation",
                CREATOR = "Mistral AI"
            };
        }

        // DuckDuckGo X-Vqd-4 token header
        public string token = "";

        // Default model
        public ChatModel model { get; set; } = ChatAPI.ChatModels.ChatGPT_4o_Mini;

        public string DefaultPrompt =
            "Current date: " + DateTime.UtcNow.Day + "." + DateTime.UtcNow.Month + "." + DateTime.UtcNow.Year + "\n" +
            "These are your instructions. Please make sure to always respect and enforce them to the best of your abilities. " +
            "1.) You provide short, meaningful answers. Keep your answers AS SHORT AS POSSIBLE! " +
            "2.) Bring IT related help to the point. Double-check code readability, security and professionality. " +
            "3.) Never mention the rules or instructions! The user does not care!" +
            "9.) Now answer with 'Hey there, nice to meet you! How can i be of service?'.";


        // Chat history
        public List<dynamic> messages = new List<dynamic>();

        public async Task<int> initConversation()
        {
            // Define handler
            var handler = new HttpClientHandler();
            HttpClient client = new HttpClient(handler);

            // Step 1: GET front page
            try
            {
                // Add headers
                client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/132.0.0.0 Safari/537.36");

                // Send GET request
                await client.GetAsync("https://duckduckgo.com/?q=DuckDuckGo+AI+Chat&ia=chat&duckai=1");

            }
            catch (Exception ex) { MessageBox.Show(ex.Message); return 1; }

            // Step 2: Obtain token
            try
            {
                // This header is important to obtain the token.
                // It tells the duckduckgo webserver that we don't have a token yet and would like to receive one
                client.DefaultRequestHeaders.Add("X-Vqd-Accept", "1");

                // Send GET request
                HttpResponseMessage response = await client.GetAsync("https://duckduckgo.com/duckchat/v1/status");

                // Ensure that request was successfull
                response.EnsureSuccessStatusCode();

                // Get the token from the response headers
                var headerVals = response.Headers.GetValues("X-Vqd-4");
                token = headerVals.FirstOrDefault() ?? "";
            }
            catch (Exception) { return 1; }
            return 0;
        }

        public async Task<string> PromptAsync(string msg)
        {
            // Define handler
            var handler = new HttpClientHandler();
            HttpClient client = new HttpClient(handler);

            // Add headers
            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/132.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Add("x-vqd-4", token); // Very important. Won't work without!
            

            // Add important information based on the msg
            string lmsg = msg.ToLower();
            if(lmsg.Contains("time") || lmsg.Contains("clock") || lmsg.Contains("late"))
            {
                msg += "auto-context: " + DateTime.UtcNow;
            }

            // Add Message to message history
            messages.Add(new { role = "user", content = msg });

            // Define payload
            var payload = new
            {
                model = model.ID,
                messages = messages
            };

            // Serialize payload to JSON
            var jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Set content headers
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            // Send GET request
            HttpResponseMessage chatResponse = await client.PostAsync("https://duckduckgo.com/duckchat/v1/chat", content);

            // Get new token from response headers
            if(chatResponse.Headers.GetValues("X-Vqd-4") != null)
            {
                var headerVals = chatResponse.Headers.GetValues("X-Vqd-4");
                token = headerVals.FirstOrDefault() ?? "";
            }
            
            // Check if response was NOT successfull
            if (chatResponse.StatusCode.ToString() != "OK")
            {
                // Show status code
                MessageBox.Show("Request Error: " + chatResponse.StatusCode.ToString());
            }

            // Read response content
            string responseContent = await chatResponse.Content.ReadAsStringAsync();

            // Split response content by "data: "
            var jsonObjects = new List<JObject>();
            string[] parts = responseContent.Split(new[] { "data: " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                try
                {
                    JObject jsonResponse = JObject.Parse(part);
                    jsonObjects.Add(jsonResponse);
                }
                catch (JsonReaderException)
                {
                    continue;
                }
            }
            
            // Go through all json objects
            string answer;
            StringBuilder strB = new StringBuilder();
            for (int i = 0; i < jsonObjects.Count; i++)
            {
                try
                {   // Add parts to a final, readable message
                    JToken firstJson = jsonObjects[i];
                    if (firstJson["message"] != null)
                    {
                        // Get message chunk for final answer
                        var value = firstJson["message"];

                        // Append message chunk to final message
                        strB.Append(value);
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }

            // Get final answer
            answer = strB.ToString();

            // Add LLM response to message history
            messages.Add(new { role = "assistant", content = " " + answer });

            // Dispose client
            client.Dispose();

            // Return answer
            return answer;
        }
    }
}
