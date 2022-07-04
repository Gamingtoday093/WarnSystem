using System.Reflection;
using System.Threading.Tasks;
using Rocket.Core.Logging;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using Rocket.Core.Utils;
using WarnSystem.Models;
using WarnSystem.Services;
using System;

namespace WarnSystem.Connections
{
    public class DiscordWebhook
    {
        public static async Task SendDiscordWebhook(string URL, string jsonData)
        {
            WebRequest client = WebRequest.Create(URL);
            client.Method = "POST";
            client.ContentType = "application/json";

            using (var writer = new StreamWriter(client.GetRequestStream()))
            {
                writer.Write(jsonData);
                writer.Flush();
            }

            try
            {
                await client.GetResponseAsync();
            }
            catch (WebException we)
            {
                TaskDispatcher.QueueOnMainThread(() => 
                {
                    Logger.LogError($"[{Assembly.GetExecutingAssembly().FullName.Split(',')[0]}] Failed to Post to Discord API (Status: {we.Status}) - {we}");
                });
            }
        }

        public static string FormatDiscordWebhookWarn(string Username, string AvatarURL, string Title, int Color, string FooterText, string IconUrl, string TargetPlayerName, string TargetSteamID, string ModeratorName, string reason, string ServerName, string IP)
        {
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
                                name = "Moderator",
                                value = ModeratorName,
                                inline = false
                            },
                            new
                            {
                                name = "Reason",
                                value = reason,
                                inline = false
                            },
                            new
                            {
                                name = ServerName,
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

        public static string FormatDiscordWebhookWarnDelete(string Username, string AvatarURL, string Title, int Color, string FooterText, string IconUrl, string TargetPlayerName, string TargetSteamID, string ModeratorName, Warn WarningDeleted, string ServerName, string IP)
        {
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
                                name = "Moderator",
                                value = ModeratorName,
                                inline = false
                            },
                            new
                            {
                                name = "Warning Removed",
                                value = FormatWarning(WarningDeleted),
                                inline = false
                            },
                            new
                            {
                                name = ServerName,
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

        private static string FormatWarning(Warn warning)
        {
            return
                WarnSystem.Instance.Translate("WarnVModerator", warning.moderatorSteamID64) + "\n" +
                WarnSystem.Instance.Translate("WarnVDateTime", warning.dateTime, FormatedTime.FormatSeconds(DateTimeOffset.Now - warning.dateTime)) + "\n" +
                WarnSystem.Instance.Translate("WarnVReason", warning.moderatorSteamID64);
        }

        public static string FormatDiscordWebhookWarnClear(string Username, string AvatarURL, string Title, int Color, string FooterText, string IconUrl, string TargetPlayerName, string TargetSteamID, string ModeratorName, string ServerName, string IP)
        {
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
                                name = "Moderator",
                                value = ModeratorName,
                                inline = false
                            },
                            new
                            {
                                name = ServerName,
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
