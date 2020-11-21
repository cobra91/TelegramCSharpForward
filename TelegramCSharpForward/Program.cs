using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TeleSharp.TL.Upload;

using TLSharp.Core;
using TLSharp.Core.Utils;

namespace TelegramCSharpForward
{
    class Program
    {
        private static int ApiId;
        private static string ApiHash;
        private static string PhoneNumber;
        private static TelegramClient Client;
        private static int UserId;
        private static FileSessionStore FileSessionStore = new FileSessionStore(new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TLSharp"));
        private static double TimerIntervalInMs = 16000.00;  // Timer interval in ms
        private static Dictionary<int, List<object>> ChannelId = new Dictionary<int, List<object>>();
        private static long AccessHash = 0;
        private static TLAbsDialogs TLAbsDialogs = null;
        private static string SessionName;
        // RealChanId
        private static int MyChanId = 0;

        // Offset Indice
        private static double DowJonesOffset = -83.91;
        private static double NasdaqOffset = -10.47;
        private static double DaxOffset = -7.95;

        static async Task Main()
        {
            Console.WriteLine("Load variable from app.config");
            GatherTestConfiguration();
            Console.WriteLine("Variable from app.config loaded");
            Client = new TelegramClient(ApiId, ApiHash, FileSessionStore, SessionName);
            await Connect(Client);
            TLAbsDialogs = await Client.GetUserDialogsAsync();
            ListChannelAndChooseToForward(TLAbsDialogs);
            Chooselistchannel();
            Timer myTimer = new Timer(TimerIntervalInMs);
            myTimer.Elapsed += OnMyTimedEvent;
            myTimer.Enabled = true;
            myTimer.Start();
            System.Threading.Thread.Sleep(5000);
            OnMyTimedEvent(null, null);
            Console.WriteLine("Type exit if you want to close the program");
            string input = Console.ReadLine();
            while (input != "exit")
            {
                Console.WriteLine("Type exit if you want to close the program");
                input = Console.ReadLine();
            }
        }

        private static void GatherTestConfiguration()
        {
            string appConfigMsgWarning = "{0} not configured in app.config! Some tests may fail.";

            string apidId = ConfigurationManager.AppSettings["apidId"];
            if (string.IsNullOrEmpty(apidId))
            {
                Debug.WriteLine(appConfigMsgWarning, nameof(apidId));
            }
            else
            {
                ApiId = int.Parse(apidId);
            }

            ApiHash = ConfigurationManager.AppSettings[nameof(ApiHash)];
            if (string.IsNullOrEmpty(ApiHash))
            {
                Debug.WriteLine(appConfigMsgWarning, nameof(ApiHash));
            }

            SessionName = ConfigurationManager.AppSettings[nameof(SessionName)];
            if (string.IsNullOrEmpty(SessionName))
            {
                Debug.WriteLine(appConfigMsgWarning, nameof(SessionName));
            }

            PhoneNumber = ConfigurationManager.AppSettings[nameof(PhoneNumber)];
            if (string.IsNullOrEmpty(PhoneNumber))
            {
                Debug.WriteLine(appConfigMsgWarning, nameof(PhoneNumber));
            }

            string myChanId = ConfigurationManager.AppSettings["myChanId"];
            if (string.IsNullOrEmpty(myChanId))
            {
                Debug.WriteLine(appConfigMsgWarning, nameof(myChanId));
            }
            else
            {
                MyChanId = int.Parse(myChanId);
            }
        }

        static async Task Connect(TelegramClient client)
        {
            if (!client.IsUserAuthorized())
            {
                await ConnectAsync(client);
                Console.WriteLine("connected");
            }
            else
            {
                Console.WriteLine("User authorized = " + client.IsUserAuthorized());
                await client.ConnectAsync(false);
                UserId = client.Session.TLUser.Id;
                Console.WriteLine("connected");
            }
        }

        static async Task ConnectAsync(TelegramClient client)
        {
            try
            {
                await client.ConnectAsync(false);
                string hash = await client.SendCodeRequestAsync(PhoneNumber);
                Console.WriteLine("Please enter the Telegram code:");
                string TelegramCode = Console.ReadLine();
                TLUser user = await client.MakeAuthAsync(PhoneNumber, hash, TelegramCode);
                UserId = user.Id;
            }
            catch (Exception e)
            {
                while (e.InnerException != null)
                {
                    e = e.InnerException;
                }

                Console.WriteLine(e.Message);
            }
        }

        static void ListChannelAndChooseToForward(TLAbsDialogs tLAbsDialogs)
        {
            // List all channel and store ID, channelTitle and AccessHash
            Console.WriteLine("List all channel and store ID, channelTitle and AccessHash");
            foreach (TLAbsChat tLAbsChat in ((TLDialogs)tLAbsDialogs).Chats.Where(x => x is TLChannel channel))
            {
                if (tLAbsChat is TLChannel channel)
                {
                    if (!ChannelId.ContainsKey(channel.Id))
                    {
                        List<object> paramList = new List<object>
                        {
                            channel.Title,
                            channel.AccessHash
                        };
                        ChannelId.Add(channel.Id, paramList);
                        if (channel.Id == MyChanId)
                        {
                            AccessHash = channel.AccessHash.Value;
                        }
                    }
                }
            }
        }

        static void Chooselistchannel()
        {
            Console.WriteLine("List all existing channel :");
            //1) Ajouter un mécanisme avant de faire les Timer pour choisir la liste des Channel qui seront pris en compte !
            int cpt = 0;
            foreach (KeyValuePair<int, List<object>> channel in ChannelId.OrderBy(obj => obj.Key).ToDictionary(obj => obj.Key, obj => obj.Value))
            {
                Console.WriteLine(cpt + ") " + channel.Value[0]);
                cpt++;
            }
            Console.WriteLine("Type each channel number you want to transfer separate by comma (or type Enter to take 8,11 and 31)");
            string input = Console.ReadLine();
            if (input == "")
            {
                input = "8,11,31";
            }
            Dictionary<int, List<object>> channelIdToKeep = new Dictionary<int, List<object>>();
            foreach (string chanId in input.Split(','))
            {
                Dictionary<int, List<object>> crtChannel = ChannelId.OrderBy(obj => obj.Key).ToDictionary(obj => obj.Key, obj => obj.Value);
                if (!channelIdToKeep.ContainsKey(crtChannel.ElementAt(int.Parse(chanId)).Key))
                {
                    List<object> paramList = new List<object>
                    {
                        crtChannel.ElementAt(int.Parse(chanId)).Value[0],
                        crtChannel.ElementAt(int.Parse(chanId)).Value[1]
                    };
                    channelIdToKeep.Add(crtChannel.ElementAt(int.Parse(chanId)).Key, paramList);
                }
            }
            if (!channelIdToKeep.ContainsKey(MyChanId))
            {
                List<object> paramList = new List<object>
                {
                    ChannelId[MyChanId][0],
                    ChannelId[MyChanId][1]
                };
                channelIdToKeep.Add(MyChanId, paramList);
            }
            ChannelId = channelIdToKeep;
        }

        private static async void OnMyTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                Console.WriteLine("On timer event");
                DateTime nowDateTime = DateTime.Now.ToLocalTime();
                // Check that we are well connected
                if (Client != null && Client.IsConnected && UserId != 0)
                {
                    if (ChannelId != null && ChannelId.Count > 0)
                    {
                        TLAbsDialogs = await Client.GetUserDialogsAsync();
                        foreach (TLAbsMessage tLAbsMessage in ((TLDialogs)TLAbsDialogs).Messages.Where(x => x is TLMessage message && TimeUnixTOWindows(message.Date, true) >= nowDateTime.AddMilliseconds(-(TimerIntervalInMs - 1))))
                        {
                            if (FilterMessage(((TLMessage)tLAbsMessage).Message) != null)
                            {
                                ((TLMessage)tLAbsMessage).Message = CalculOffset(((TLMessage)tLAbsMessage).Message);
                                if (((TLMessage)tLAbsMessage).ToId is TLPeerUser tLPeerUser)
                                {
                                    // Personal Chat Do Not Forward!
                                }
                                else if (((TLMessage)tLAbsMessage).ToId is TLPeerChannel channel0 && ((TLMessage)tLAbsMessage).ReplyToMsgId != null)
                                {
                                    int crtChannelId = channel0.ChannelId;
                                    if (crtChannelId != MyChanId && ChannelId.ContainsKey(crtChannelId))
                                    {
                                        Console.WriteLine("ReplyChannelId " + ((TLMessage)tLAbsMessage).ReplyToMsgId);
                                        await ReplyMessage((TLMessage)tLAbsMessage);
                                    }
                                }
                                else if (((TLMessage)tLAbsMessage).ToId is TLPeerChat chat && ((TLMessage)tLAbsMessage).ReplyToMsgId != null)
                                {
                                    Console.WriteLine("ReplyChatId " + ((TLMessage)tLAbsMessage).ReplyToMsgId);
                                    await ReplyMessage((TLMessage)tLAbsMessage);
                                }
                                else if (((TLMessage)tLAbsMessage).ToId is TLPeerChannel channel && ((TLMessage)tLAbsMessage).ReplyToMsgId == null)
                                {
                                    int crtChannelId = channel.ChannelId;
                                    if (crtChannelId != MyChanId && ChannelId.ContainsKey(crtChannelId))
                                    {
                                        Console.WriteLine("New Message Channel " + ChannelId[crtChannelId][0] + " \n" + ((TLMessage)tLAbsMessage).Message);
                                        if (ChannelId.ContainsKey(crtChannelId))
                                        {
                                            if (((TLMessage)tLAbsMessage).Message != "")
                                            {
                                                if (((TLMessage)tLAbsMessage).Message.ToLower().StartsWith("tp") || ((TLMessage)tLAbsMessage).Message.ToLower().StartsWith("sl"))
                                                {
                                                    TLChannelMessages historyFromSourceCanal = (TLChannelMessages)await Client.GetHistoryAsync(new TLInputPeerChannel() { ChannelId = channel.ChannelId, AccessHash = (long)ChannelId[channel.ChannelId][1] });
                                                    List<TLAbsMessage> tLMessageList = historyFromSourceCanal.Messages.ToList().Where(x => x is TLMessage tL).ToList();
                                                    List<TLMessage> orderedtLMessageList = tLMessageList.Cast<TLMessage>().OrderByDescending(x => x.Id).ToList();
                                                    string newMessage = CalculOffset(orderedtLMessageList[1].Message + "\n" + ((TLMessage)tLAbsMessage).Message);
                                                    if (orderedtLMessageList[1].Message.ToLower().Contains("sell") && orderedtLMessageList[1].Message.ToLower().Contains("sl"))
                                                    {
                                                        await Client.SendMessageAsync(new TLInputPeerChannel() { ChannelId = MyChanId, AccessHash = AccessHash }, newMessage);
                                                    }
                                                    else if (orderedtLMessageList[1].Message.ToLower().Contains("vente") && orderedtLMessageList[1].Message.ToLower().Contains("sl"))
                                                    {
                                                        await Client.SendMessageAsync(new TLInputPeerChannel() { ChannelId = MyChanId, AccessHash = AccessHash }, newMessage);
                                                    }
                                                    else if (orderedtLMessageList[1].Message.ToLower().Contains("buy") && orderedtLMessageList[1].Message.ToLower().Contains("sl"))
                                                    {
                                                        await Client.SendMessageAsync(new TLInputPeerChannel() { ChannelId = MyChanId, AccessHash = AccessHash }, newMessage);
                                                    }
                                                    else if (orderedtLMessageList[1].Message.ToLower().Contains("achat") && orderedtLMessageList[1].Message.ToLower().Contains("sl"))
                                                    {
                                                        await Client.SendMessageAsync(new TLInputPeerChannel() { ChannelId = MyChanId, AccessHash = AccessHash }, newMessage);
                                                    }
                                                }
                                                else
                                                {
                                                    await Client.SendMessageAsync(new TLInputPeerChannel() { ChannelId = MyChanId, AccessHash = AccessHash }, ((TLMessage)tLAbsMessage).Message);
                                                }
                                            }                                            
                                        }
                                    }
                                }
                            }                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }
                Console.WriteLine(ex.Message);
            }
        }

        private static string FilterMessage(string message)
        {
            List<string> keywordList = new List<string>
            {
                "sell",
                "buy",
                "achat",
                "achète",
                "vente",
                "tp ",
                "sl ",
                "cloture",
                "clôture",
                " limit ",
                " partiel",
                "tp1",
                "tp2",
                "tp3",
                "sécurisez",
                "break even",
                "modifier",
                "fermer",
                "fermez",
                "close",
                "supprimer",
                "supprimez"
            };

            foreach (string keyword in keywordList)
            {
                if (message.ToLower().Contains(keyword))
                {
                    return message;
                }
            }
            return null;
        }

        private static string CalculOffset(string message)
        {
            Regex regexp = new Regex("([0-9]{2} [0-9]{3})|([0-9]{5})");
            if (message.ToLower().Contains("dax30") || message.ToLower().Contains("dax"))
            {
                string firstPartOfMessage = null;
                if (message.ToLower().Contains("dax30"))
                {
                    firstPartOfMessage = message.Split(' ')[0] + " " + message.Split(' ')[1];
                    message = message.Substring(message.IndexOf(message.Split(' ')[2]));
                }
                MatchCollection matchCollection = regexp.Matches(message);
                if (matchCollection.Count() > 0 && matchCollection.Where(x => x.Success == true).Count() > 0)
                {
                    foreach (Match match in matchCollection)
                    {
                        message = message.Replace(match.Value, (int.Parse(match.Value.Replace(" ", "")) + DaxOffset).ToString());
                    }
                }
                if (firstPartOfMessage != null)
                {
                    message = firstPartOfMessage + " " + message;
                }
            }
            else if (message.ToLower().Contains("us30") || message.ToLower().Contains("dj30") || message.ToLower().Contains("dowjones") || message.ToLower().Contains("dow jones"))
            {
                string firstPartOfMessage = null;
                if (message.ToLower().Contains("us30") || message.ToLower().Contains("dj30"))
                {
                    firstPartOfMessage = message.Split(' ')[0] + " " + message.Split(' ')[1];
                    message = message.Substring(message.IndexOf(message.Split(' ')[2]));
                }
                MatchCollection matchCollection = regexp.Matches(message);
                if (matchCollection.Count() > 0 && matchCollection.Where(x => x.Success == true).Count() > 0)
                {
                    foreach (Match match in matchCollection)
                    {
                        message = message.Replace(match.Value, (int.Parse(match.Value.Replace(" ", "")) + DowJonesOffset).ToString());
                    }
                }
                if (firstPartOfMessage != null)
                {
                    message = firstPartOfMessage + " " + message;
                }
            }
            else if (message.ToLower().Contains("nasdaq"))
            {
                MatchCollection matchCollection = regexp.Matches(message);
                if (matchCollection.Count() > 0 && matchCollection.Where(x => x.Success == true).Count() > 0)
                {
                    foreach (Match match in matchCollection)
                    {
                        message = message.Replace(match.Value, (int.Parse(match.Value.Replace(" ", "")) + NasdaqOffset).ToString());
                    }
                }
            }
            return message;
        }

        // Convert Unix Time to Windows Time
        private static DateTime TimeUnixTOWindows(double TimestampToConvert, bool Local)
        {
            DateTime mdt = new DateTime(1970, 1, 1, 0, 0, 0);
            if (Local)
            {
                return mdt.AddSeconds(TimestampToConvert).ToLocalTime();
            }
            else
            {
                return mdt.AddSeconds(TimestampToConvert);
            }
        }

        private static async Task ReplyMessage(TLMessage tLAbsMessage)
        {
            try
            {
                if (tLAbsMessage.ToId is TLPeerChannel channel)
                {
                    Console.WriteLine($"New Reply from channel {ChannelId[channel.ChannelId][0]} Message {tLAbsMessage.Message} to message {tLAbsMessage.ReplyToMsgId}");
                    TLChannelMessages historyFromSourceCanal = (TLChannelMessages)await Client.GetHistoryAsync(new TLInputPeerChannel() { ChannelId = channel.ChannelId, AccessHash = (long)ChannelId[channel.ChannelId][1] });
                    TLMessage tLMessage = (TLMessage)historyFromSourceCanal.Messages.ToList().FirstOrDefault(x => x is TLMessage tL && tL.Id == tLAbsMessage.ReplyToMsgId);
                    if (tLMessage != null)
                    {
                        Console.WriteLine($"REPLY {tLMessage.Message}\n{tLAbsMessage.Message}");
                        TLChannelMessages historyFromMyNewCanal = (TLChannelMessages)await Client.GetHistoryAsync(new TLInputPeerChannel() { ChannelId = MyChanId, AccessHash = AccessHash });
                        List<TLAbsMessage> tLAbsMessages = historyFromMyNewCanal.Messages.ToList().Where(x => x is TLMessage tL && tL.Message == tLMessage.Message).ToList();
                        if (tLAbsMessages.Count == 0 || tLAbsMessages.Count > 1)
                        {
                            throw new Exception("None or Multiple message found for Reply to unique Message");
                        }
                        else
                        {
                            TLMessage tLMessageInNewchan = (TLMessage)historyFromMyNewCanal.Messages.ToList().FirstOrDefault(x => x is TLMessage tL && tL.Message == tLMessage.Message);
                            if (tLMessageInNewchan != null)
                            {
                                TLRequestSendMessage send = new TLRequestSendMessage
                                {
                                    Peer = new TLInputPeerChannel() { ChannelId = MyChanId, AccessHash = AccessHash },
                                    Message = tLAbsMessage.Message,
                                    ReplyToMsgId = tLMessageInNewchan.Id,
                                    RandomId = Helpers.GenerateRandomLong(),
                                };
                                await Client.SendRequestAsync<TLUpdates>(send);
                            }
                            else
                            {
                                throw new Exception("Original Message not found to Reply");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }
                Console.WriteLine(ex.Message);
            }
        }
    }
}