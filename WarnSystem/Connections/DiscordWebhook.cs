using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Rocket.Core.Logging;
using Newtonsoft.Json;

namespace WarnSystem.Connections
{
    public class DiscordWebhook
    {
        public static async Task SendDiscordWebhook(string URL, string jsonData)
        {
            HttpClient client = new HttpClient();
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(URL, content);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError($"[{Assembly.GetExecutingAssembly().FullName.Split(',')[0]}] Status Code: {response.StatusCode} - Failed to Post to Discord API");
            }
            response.EnsureSuccessStatusCode();
        }

        public static string FormatDiscordWebhook(string Username, string AvatarURL, string Title, int Color, string FooterText, string IconUrl, string TargetPlayerName, string TargetSteamID, string ModeratorName, string reason, string IP)
        {
            if (!string.IsNullOrEmpty(reason)) return JsonConvert.SerializeObject(new
            {
                username = Username,
                avatar_url = AvatarURL,
                embeds = new[]
                {
                    new
                    {
                        title = Title,
                        color = Color,
                        fields = new[]
                        {
                            new
                            {
                                name = TargetPlayerName,
                                value = TargetSteamID,
                                inline = true
                            },
                            new
                            {
                                name = "Moderator:",
                                value = ModeratorName,
                                inline = false
                            },
                            new
                            {
                                name = "Reason:",
                                value = reason,
                                inline = false
                            },
                            new
                            {
                                name = "Server IP",
                                value = IP,
                                inline = false
                            }
                        },
                        footer = new
                        {
                            text = FooterText,
                            icon_url = IconUrl
                        }
                    }
                }
            });

            return JsonConvert.SerializeObject(new
            {
                username = Username,
                avatar_url = AvatarURL,
                embeds = new[]
                {
                    new
                    {
                        title = Title,
                        color = Color,
                        fields = new[]
                        {
                            new
                            {
                                name = TargetPlayerName,
                                value = TargetSteamID,
                                inline = true
                            },
                            new
                            {
                                name = "Moderator:",
                                value = ModeratorName,
                                inline = false
                            },
                            new
                            {
                                name = "Server IP",
                                value = IP,
                                inline = false
                            }
                        },
                        footer = new
                        {
                            text = FooterText,
                            icon_url = IconUrl
                        }
                    }
                }
            });
        }
    }
}
