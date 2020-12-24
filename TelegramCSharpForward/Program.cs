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
using TLSharp.Core.Network.Exceptions;
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
        // changement le 24.11 ancienne valeur -83.91
        // changement le 30.11 ancienne valeur -54
        // changement le 08.12 ancienne valeur -40
        private static double DowJonesOffset = -11;
        // changement le 24.11 ancienne valeur -10.47
        // changement le 30.11 ancienne valeur -10.22
        // changement le 08.12 ancienne valeur -9
        private static double NasdaqOffset = -3;
        // changement le 24.11 ancienne valeur -7.95
        // changement le 30.11 ancienne valeur -6.5
        // changement le 08.12 ancienne valeur -5.1
        private static double DaxOffset = -4;

        // Anti-Flood
        private static bool Flood = false;

        static async Task Main()
        {
            if(Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TLSharp"))
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TLSharp");
            }
            Console.WriteLine("Load variable from app.config");
            GatherTestConfiguration();
            Console.WriteLine("Variable from app.config loaded");
            Client = new TelegramClient(ApiId, ApiHash, FileSessionStore, SessionName);
            await Connect(Client);
            TLAbsDialogs = await Client.GetUserDialogsAsync();
            ListChannel(TLAbsDialogs);
            ChooseChannel();
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

            string dowJonesOffset = ConfigurationManager.AppSettings["DowJonesOffset"];
            if (string.IsNullOrEmpty(dowJonesOffset))
            {
                Debug.WriteLine(appConfigMsgWarning, nameof(dowJonesOffset));
            }
            else
            {
                DowJonesOffset = double.Parse(dowJonesOffset);
            }

            string daxOffset = ConfigurationManager.AppSettings["DaxOffset"];
            if (string.IsNullOrEmpty(daxOffset))
            {
                Debug.WriteLine(appConfigMsgWarning, nameof(daxOffset));
            }
            else
            {
                DaxOffset = double.Parse(daxOffset);
            }

            string nasdaqOffset = ConfigurationManager.AppSettings["NasdaqOffset"];
            if (string.IsNullOrEmpty(nasdaqOffset))
            {
                Debug.WriteLine(appConfigMsgWarning, nameof(nasdaqOffset));
            }
            else
            {
                NasdaqOffset = double.Parse(nasdaqOffset);
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

        static void ListChannel(TLAbsDialogs tLAbsDialogs)
        {
            // List all channel and store ID, channelTitle and AccessHash
            if (tLAbsDialogs is TLDialogsSlice slice)
            {
                foreach (TLAbsChat tLAbsChat in slice.Chats.Where(x => x is TLChannel channel))
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
            else
            {
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
        }

        static void ChooseChannel()
        {
            Console.WriteLine("List all existing channel :");
            int cpt = 0;
            foreach (KeyValuePair<int, List<object>> channel in ChannelId.OrderBy(obj => obj.Key).ToDictionary(obj => obj.Key, obj => obj.Value))
            {
                Console.WriteLine(cpt + ") " + channel.Value[0]);
                cpt++;
            }
            Console.WriteLine("Type each channel number you want to transfer separate by comma (or type Enter to take 8,19,23 and 30)");
            string input = Console.ReadLine();
            if (input == "")
            {
                input = "8,19,23,30";
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
                if (!Flood)
                {
                    Console.WriteLine("On timer event");
                    DateTime nowDateTime = DateTime.Now.ToLocalTime();
                    // Check that we are well connected
                    if (Client != null && Client.IsConnected && UserId != 0)
                    {
                        if (ChannelId != null && ChannelId.Count > 0)
                        {
                            TLAbsDialogs = await Client.GetUserDialogsAsync();
                            if (TLAbsDialogs is TLDialogsSlice slice)
                            {
                                foreach (TLAbsMessage tLAbsMessage in slice.Messages.Where(x => x is TLMessage message && TimeUnixTOWindows(message.Date, true) >= nowDateTime.AddMilliseconds(-(TimerIntervalInMs - 1))))
                                {
                                    Treatment(tLAbsMessage);
                                }
                            }
                            else
                            {
                                foreach (TLAbsMessage tLAbsMessage in ((TLDialogs)TLAbsDialogs).Messages.Where(x => x is TLMessage message && TimeUnixTOWindows(message.Date, true) >= nowDateTime.AddMilliseconds(-(TimerIntervalInMs - 1))))
                                {
                                    Treatment(tLAbsMessage);
                                }
                            }
                        }
                    }
                }                
            }
            catch (FloodException floodException)
            {
                while (floodException.InnerException != null)
                {
                    floodException = (FloodException)floodException.InnerException;
                }
                Console.WriteLine(floodException.Message);
                Flood = true;
                System.Threading.Thread.Sleep(floodException.TimeToWait);
                Flood = false;
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

        private static async void Treatment(TLAbsMessage tLAbsMessage)
        {
            try
            {
                ((TLMessage)tLAbsMessage).Message = FilterMessage(((TLMessage)tLAbsMessage).Message);
                if (((TLMessage)tLAbsMessage).Message != null || ((TLMessage)tLAbsMessage).Media != null)
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
                                if (!string.IsNullOrEmpty(((TLMessage)tLAbsMessage).Message))
                                {
                                    if (((TLMessage)tLAbsMessage).Message.ToLower().StartsWith("tp") || ((TLMessage)tLAbsMessage).Message.ToLower().StartsWith("sl"))
                                    {
                                        TLChannelMessages historyFromSourceCanal = (TLChannelMessages)await Client.GetHistoryAsync(new TLInputPeerChannel() { ChannelId = channel.ChannelId, AccessHash = (long)ChannelId[channel.ChannelId][1] });
                                        List<TLAbsMessage> tLMessageList = historyFromSourceCanal.Messages.ToList().Where(x => x is TLMessage tL).ToList();
                                        List<TLMessage> orderedtLMessageList = tLMessageList.Cast<TLMessage>().OrderByDescending(x => x.Id).ToList();
                                        string newMessage = CalculOffset(orderedtLMessageList[1].Message + "\n" + ((TLMessage)tLAbsMessage).Message);
                                        bool sent = false;
                                        if (orderedtLMessageList[1].Message.ToLower().Contains("sell") || orderedtLMessageList[1].Message.ToLower().Contains("vente") || orderedtLMessageList[1].Message.ToLower().Contains("buy")
                                            || orderedtLMessageList[1].Message.ToLower().Contains("achat"))
                                        {
                                            await Client.SendMessageAsync(new TLInputPeerChannel() { ChannelId = MyChanId, AccessHash = AccessHash }, newMessage);
                                            sent = true;
                                        }
                                        if (sent)
                                        {
                                            TLChannelMessages tLChannelMessagesFromSourceCanal = (TLChannelMessages)await Client.GetHistoryAsync(new TLInputPeerChannel() { ChannelId = MyChanId, AccessHash = AccessHash });
                                            TeleSharp.TL.Channels.TLRequestDeleteMessages request = new TeleSharp.TL.Channels.TLRequestDeleteMessages
                                            {
                                                Channel = new TLInputChannel() { ChannelId = MyChanId, AccessHash = AccessHash },
                                                Id = new TLVector<int>()
                                                {
                                                    ((TLMessage)tLChannelMessagesFromSourceCanal.Messages.ToList().FirstOrDefault(x => x is TLMessage tl && tl.Message == CalculOffset(orderedtLMessageList[1].Message))).Id
                                                }
                                            };
                                            //await Client.SendRequestAsync<TLAffectedMessages>(request);
                                        }
                                    }
                                    else
                                    {
                                        await Client.SendMessageAsync(new TLInputPeerChannel() { ChannelId = MyChanId, AccessHash = AccessHash }, ((TLMessage)tLAbsMessage).Message);
                                    }
                                }
                                else if (((TLMessage)tLAbsMessage).Media != null && (crtChannelId == 1360607920 || crtChannelId == 1302796093))
                                {
                                    if (((TLMessage)tLAbsMessage).Media.GetType().ToString() == "TeleSharp.TL.TLMessageMediaPhoto")
                                    {
                                        TLMessageMediaPhoto tLMessageMediaPhoto = (TLMessageMediaPhoto)((TLMessage)tLAbsMessage).Media;
                                        TLPhoto tLPhoto = (TLPhoto)tLMessageMediaPhoto.Photo;
                                        TLPhotoSize tLPhotoSize = tLPhoto.Sizes.ToList().OfType<TLPhotoSize>().Last();
                                        TLFileLocation tLFileLocation = (TLFileLocation)tLPhotoSize.Location;
                                        TLAbsInputFileLocation tLAbsInputFileLocation = new TLInputFileLocation()
                                        {
                                            LocalId = tLFileLocation.LocalId,
                                            Secret = tLFileLocation.Secret,
                                            VolumeId = tLFileLocation.VolumeId
                                        };
                                        TLInputFileLocation TLInputFileLocation = tLAbsInputFileLocation as TLInputFileLocation;
                                        TLFile buffer = await Client.GetFile(TLInputFileLocation, 1024 * 512);
                                        TLInputFile fileResult = (TLInputFile)await UploadHelper.UploadFile(Client, "", new StreamReader(new MemoryStream(buffer.Bytes)));
                                        await Client.SendUploadedPhoto(new TLInputPeerChannel() { ChannelId = MyChanId, AccessHash = AccessHash }, fileResult, tLMessageMediaPhoto.Caption);
                                    }
                                    else if (((TLMessage)tLAbsMessage).Media.GetType().ToString() == "TeleSharp.TL.TLMessageMediaDocument")
                                    {
                                        TLMessageMediaDocument tLMessageMediaDocument = (TLMessageMediaDocument)((TLMessage)tLAbsMessage).Media;
                                        TLDocument tLDocument = (TLDocument)tLMessageMediaDocument.Document;
                                        TLVector<TLAbsDocumentAttribute> tLAbsDocumentAttributes = tLDocument.Attributes;
                                        TLInputDocumentFileLocation tLInputDocumentFileLocation = new TLInputDocumentFileLocation()
                                        {
                                            AccessHash = tLDocument.AccessHash,
                                            Id = tLDocument.Id,
                                            Version = tLDocument.Version,
                                        };
                                        TLFile buffer = await Client.GetFile(tLInputDocumentFileLocation, 1024 * 512);
                                        TLInputFile fileResult = (TLInputFile)await UploadHelper.UploadFile(Client, ((TLDocumentAttributeFilename)tLAbsDocumentAttributes[0]).FileName, new StreamReader(new MemoryStream(buffer.Bytes)));
                                        await Client.SendUploadedDocument(new TLInputPeerChannel() { ChannelId = MyChanId, AccessHash = AccessHash }, fileResult, tLMessageMediaDocument.Caption, tLDocument.MimeType, tLAbsDocumentAttributes);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                throw;
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
                "supprimez",
                "pips en cours ✅♻️",
                "décalez",
                "annulez",
                "mettez le sl",
                "pips actuellement ⏳",
                "move stop loss to",
                "small accounts can quit",
                "running +",
                "all signals from today in profits",
                "you can close"
            };

            if (message.ToLower().Contains("http"))
            {
                return null;
            }

            if(message.ToLower().Contains("🅂🄻"))
            {
                message = message.Replace("🅂🄻", "SL");
                message = message.Replace("✅ 𝗧𝗣", "✅ TP");
                message = message.Replace("DowJones30", "dj30");
                message = message.Replace("Oil WTI", "Oil");
                if (message.Contains("𝗢𝗿𝗱𝗿𝗲 𝗲𝗻 𝗔𝘁𝘁𝗲𝗻𝘁𝗲") || message.Contains("Ordre en Attente"))
                {
                    message = message.Replace("💶 🄿🅁🄸🅇 : ", "@");
                    if (message.ToLower().Contains("vente"))
                    {
                        message = message.Replace("Vente", "Sell limit");
                        message = message.Replace("vente", "Sell limit");
                    }
                    if (message.ToLower().Contains("achat"))
                    {
                        message = message.Replace("Achat", "Buy limit");
                        message = message.Replace("achat", "Buy limit");
                    }                    
                }
            }

            foreach (string keyword in keywordList)
            {
                if (message.ToLower().Contains(keyword))
                {
                    if (message.ToLower().Contains("clôture partielle") || message.ToLower().Contains("clôturez partiellement") || message.ToLower().Contains("fermez la moitié"))
                    {
                        message = message.Replace("Clôture partielle", "Clôture partielle (Close half)").Replace("Clôturez partiellement", "Clôturez partiellement (Close half)");
                    }
                    if(message.ToLower().Contains("supprimez le buy limit"))
                    {
                        message = message.Replace("Supprimez le buy limit ⚠️", "Supprimez/Close le buy limit ⚠️");
                    }
                    if (message.ToLower().Contains("supprimez le sell limit"))
                    {
                        message = message.Replace("Supprimez le sell limit ⚠️", "Supprimez/Close le sell limit ⚠️");
                    }
                    if(message.ToLower().Contains("Annulez l’ordre en attente"))
                    {
                        message = message.Replace("Annulez l’ordre en attente", "Annulez(Close) l’ordre en attente");
                    }
                    if (message.ToLower().Contains("fermé à"))
                    {
                        message = message.Replace("Fermé à", "Cloturé à");
                    }
                    if (message.ToLower().Contains("fermez à"))
                    {
                        message = message.Replace("Fermez à", "Cloturé à");
                    }
                    if (message.ToLower().Contains("sécurisez"))
                    {
                        message = message.Replace("Sécurisez", "Break even");
                    }
                    if (message.ToLower().Contains("décalez"))
                    {
                        message = message.Replace("Décalez", "Modifier ");
                    }
                    if(message.ToLower().Contains("mettez le sl"))
                    {
                        message = message.Replace("Mettez le SL", "Modifier SL");
                    }
                    if (message.ToLower().Contains("you can close some profits now"))
                    {
                        message = message.Replace("You can close some profits now", "Vous pouvez prendre quelques profits maintenant via une cloture partiel si vous le souhaitez");
                    }
                    if (message.ToLower().Contains("move stop loss to"))
                    {
                        message = message.Replace("move stop loss to", "modifier SL");
                    }
                    if (message.ToLower().Contains("small accounts can quit"))
                    {
                        message = message.Replace("Small accounts can quit", "Les petit compte peuvent quitter");
                    }
                    if (message.ToLower().Contains("running +"))
                    {
                        message = message.Replace("running +", "en cours +");
                    }         
                    if(message.ToLower().Contains("all signals from today in profits"))
                    {
                        message = message.Replace("All signals from today in profits", "Tous les signaux depuis aujourd'hui sont en profits !");
                    }
                    if (message.ToLower().Contains("if you risk too much of your account you can close now"))
                    {
                        message = message.Replace("If you risk too much of your account you can close now", "Si vous risquez trop de votre compte, vous pouvez quitter maintenant !");
                    }
                    if (message.ToLower().Contains("can go on drowdown before Take profit.If you dont want to wait this trade you can close now on Entry"))
                    {
                        message = message.Replace("can go on drowdown before Take profit.If you dont want to wait this trade you can close now on Entry", "peut aller en latent négatif avant le TP.Si vous ne voulez pas attendre cette transition, vous pouvez fermer maintenant au point d'entrée");
                    }                    
                    if (message.ToLower().Contains("you can close"))
                    {
                        message = message.Replace("you can close", "vous pouvez quitter");
                    }
                    return message;
                }
            }
            Regex regexp = new Regex("([+] [0-9]{0,} pips$)|([+][0-9]{0,}pips$)|([+][0-9]{0,} pips$)|([+] [0-9]{0,}pips$)");
            if (regexp.Match(message).Success)
            {
                return message;
            }
            regexp = new Regex("([-] [0-9]{0,} pips$)|([-][0-9]{0,}pips$)|([-][0-9]{0,} pips$)|([-] [0-9]{0,}pips$)");
            if (regexp.Match(message).Success)
            {
                return message;
            }
            return null;
        }

        private static string CalculOffset(string message)
        {
            if (message != null)
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
                        tLMessage.Message = FilterMessage(CalculOffset(tLMessage.Message));
                        Console.WriteLine($"REPLY {tLMessage.Message}\n{tLAbsMessage.Message}");
                        TLChannelMessages historyFromMyNewCanal = (TLChannelMessages)await Client.GetHistoryAsync(new TLInputPeerChannel() { ChannelId = MyChanId, AccessHash = AccessHash });
                        List<TLAbsMessage> tLAbsMessages = historyFromMyNewCanal.Messages.ToList().Where(x => x is TLMessage tL && tL.Message == tLMessage.Message).ToList();
                        if (tLAbsMessages.Count == 0)
                        {
                            tLAbsMessages = historyFromMyNewCanal.Messages.ToList().Where(x => x is TLMessage tL && tL.Message == tLMessage.Message).ToList();
                        }
                        if (tLAbsMessages.Count == 0)
                        {
                            tLAbsMessages = historyFromMyNewCanal.Messages.ToList().Where(x => x is TLMessage tL && tL.Message.Contains(tLMessage.Message)).ToList();
                        }
                        if (tLAbsMessages.Count == 0)
                        {
                            throw new Exception("None message found for Reply to unique Message");
                        }
                        else if (tLAbsMessages.Count > 1)
                        {
                            List<TLMessage> tLMessageList = tLAbsMessages.Cast<TLMessage>().ToList();
                            int maxId = 0;
                            if (tLMessageList.Count > 0)
                            {
                                maxId = tLMessageList.Max(y => y.Id);
                            }
                            else
                            {
                                tLMessageList = historyFromMyNewCanal.Messages.ToList().Where(x => x is TLMessage tL && tL.Message == tLMessage.Message).Cast<TLMessage>().ToList();
                                if (tLMessageList.Count > 0)
                                {
                                    maxId = tLMessageList.Max(y => y.Id);
                                }
                            }
                            TLMessage tLMessageInNewchan = tLMessageList.FirstOrDefault(x => x.Id == maxId);
                            if (tLMessageInNewchan != null)
                            {
                                TLRequestSendMessage send;
                                if (tLMessageInNewchan.Media != null)
                                {
                                    send = new TLRequestSendMessage
                                    {
                                        Peer = new TLInputPeerChannel() { ChannelId = MyChanId, AccessHash = AccessHash },
                                        Message = tLAbsMessage.Message.Replace("Achetez", "Achetez " + ((TLMessageMediaPhoto)tLMessageInNewchan.Media).Caption).Replace("Vendez", "Vendez " + ((TLMessageMediaPhoto)tLMessageInNewchan.Media).Caption),
                                        ReplyToMsgId = tLMessageInNewchan.Id,
                                        RandomId = Helpers.GenerateRandomLong(),
                                    };
                                }
                                else
                                {
                                    send = new TLRequestSendMessage
                                    {
                                        Peer = new TLInputPeerChannel() { ChannelId = MyChanId, AccessHash = AccessHash },
                                        Message = tLAbsMessage.Message,
                                        ReplyToMsgId = tLMessageInNewchan.Id,
                                        RandomId = Helpers.GenerateRandomLong(),
                                    };
                                }
                                await Client.SendRequestAsync<TLUpdates>(send);
                            }
                            else
                            {
                                throw new Exception("Original Message not found to Reply");
                            }
                        }
                        else
                        {
                            TLMessage tLMessageInNewchan = (TLMessage)historyFromMyNewCanal.Messages.ToList().FirstOrDefault(x => x is TLMessage tL && tL.Message == tLMessage.Message);
                            if (tLMessageInNewchan == null)
                            {
                                tLMessageInNewchan = (TLMessage)historyFromMyNewCanal.Messages.ToList().FirstOrDefault(x => x is TLMessage tL && tL.Message == tLMessage.Message);
                            }
                            if (tLMessageInNewchan == null)
                            {
                                tLMessageInNewchan = (TLMessage)historyFromMyNewCanal.Messages.ToList().FirstOrDefault(x => x is TLMessage tL && tL.Message.Contains(tLMessage.Message));
                            }
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