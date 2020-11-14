using DataAccessLibrary;

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
        // RealChanId
        private static int MyChanId = 0;

        // Offset Indice
        private static double DowJonesOffset = -83.91;
        private static double NasdaqOffset = -10.47;
        private static double DaxOffset = -7.95;


        static async Task Main()
        {
            DataAccess.InitializeDatabase();
            Console.WriteLine("db initialized");
            Console.WriteLine("Load variable from app.config");
            GatherTestConfiguration();
            Console.WriteLine("Variable from app.config loaded");
            Client = new TelegramClient(ApiId, ApiHash, FileSessionStore, "sessionTL");
            await Connect(Client);
            TLAbsDialogs = await Client.GetUserDialogsAsync();
            StoreDeleteChannelToDb(TLAbsDialogs);
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

        static void StoreDeleteChannelToDb(TLAbsDialogs tLAbsDialogs)
        {
            // List all channel and store ID, channelTitle and AccessHash
            Console.WriteLine("List all channel and store ID, channelTitle and AccessHash");
            foreach (TLAbsChat tLAbsChat in ((TLDialogs)tLAbsDialogs).Chats.Where(x => x is TLChannel channel))
            {
                if (tLAbsChat is TLChannel channel)
                {
                    List<Channel> channelListFromBDD = DataAccess.GetChannelData();
                    if (channelListFromBDD.FirstOrDefault(x => x.Id == channel.Id) == null)
                    {
                        DataAccess.AddChannelData(channel.Id, channel.Title, channel.AccessHash.Value);
                    }
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
            // Delete all channel deleted in Telegram
            Console.WriteLine("Delete all channel deleted in Telegram");
            List<Channel> channelList = DataAccess.GetChannelData();
            List<Channel> toRemovechannelList = channelList.Where(x => !ChannelId.ContainsKey(x.Id)).ToList();
            foreach (Channel channel1 in toRemovechannelList)
            {
                DataAccess.RemoveChannel(channel1.Id);
                DataAccess.RemoveMessageFromChannel(channel1.Id);
            }
        }

        static void Chooselistchannel()
        {
            Console.WriteLine("Liste des canal existants en Base de données :");
            //1) Ajouter un mécanisme avant de faire les Timer pour choisir la liste des Channel qui seront pris en compte !
            int cpt = 0;
            foreach (Channel channel in DataAccess.GetChannelData().Where(x => x.Id != MyChanId).OrderByDescending(x => x.Id))
            {
                Console.WriteLine(cpt + ") " + channel.Title);
                cpt++;
            }
            Console.WriteLine("Type each channel number you want to transfer separate by comma (or type Enter to take 0,17 and 19)");
            string input = Console.ReadLine();
            if (input == "")
            {
                input = "0,17,19";
            }
            Dictionary<int, List<object>> channelIdToKeep = new Dictionary<int, List<object>>();
            foreach (string chanId in input.Split(','))
            {
                Channel crtChannel = DataAccess.GetChannelData().Where(x => x.Id != MyChanId).OrderByDescending(x => x.Id).ToList()[int.Parse(chanId)];
                if (!channelIdToKeep.ContainsKey(crtChannel.Id))
                {
                    List<object> paramList = new List<object>
                    {
                        crtChannel.Title,
                        crtChannel.AccessHash
                    };
                    channelIdToKeep.Add(crtChannel.Id, paramList);
                }
            }
            Channel myChannel = DataAccess.GetChannelData().First(x => x.Id == MyChanId);
            if (!channelIdToKeep.ContainsKey(myChannel.Id))
            {
                List<object> paramList = new List<object>
                {
                    myChannel.Title,
                    myChannel.AccessHash
                };
                channelIdToKeep.Add(myChannel.Id, paramList);
            }
            ChannelId = channelIdToKeep;
        }

        private static async void OnMyTimedEvent(Object source, ElapsedEventArgs e)
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
                                    Console.WriteLine("ReplyId " + ((TLMessage)tLAbsMessage).ReplyToMsgId);
                                    await HistoryFromBDD((TLMessage)tLAbsMessage);
                                }
                            }
                            else if (((TLMessage)tLAbsMessage).ToId is TLPeerChat chat && ((TLMessage)tLAbsMessage).ReplyToMsgId != null)
                            {
                                Console.WriteLine("ReplyId " + ((TLMessage)tLAbsMessage).ReplyToMsgId);
                                await HistoryFromBDD((TLMessage)tLAbsMessage);
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
                                            DataAccess.AddMessageData(((TLMessage)tLAbsMessage).Id, crtChannelId, ((TLMessage)tLAbsMessage).Message);
                                            await Client.SendMessageAsync(new TLInputPeerChannel() { ChannelId = MyChanId, AccessHash = AccessHash }, "Channel " + ChannelId[crtChannelId][0] + " \n" + ((TLMessage)tLAbsMessage).Message);
                                        }
                                        else if (((TLMessage)tLAbsMessage).Media != null)
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
                                                await Client.SendUploadedPhoto(new TLInputPeerChannel() { ChannelId = MyChanId, AccessHash = AccessHash }, fileResult, "Channel " + ChannelId[crtChannelId][0] + " \n" + tLMessageMediaPhoto.Caption);
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
                                                await Client.SendUploadedDocument(new TLInputPeerChannel() { ChannelId = MyChanId, AccessHash = AccessHash }, fileResult, "Channel " + ChannelId[crtChannelId][0] + " \n" + tLMessageMediaDocument.Caption, tLDocument.MimeType, tLAbsDocumentAttributes);
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

        private static string CalculOffset(string message)
        {
            Regex regexp = new Regex("([0-9]{2} [0-9]{3})|([0-9]{5})");
            if (message.ToLower().Contains("dax30") || message.ToLower().Contains("dax"))
            {
                string firstPartOfMessage = null;
                if (message.ToLower().Contains("dax30"))
                {
                    firstPartOfMessage = message.Substring(0, message.LastIndexOf("dax") + 5);
                    message = message.Substring(message.LastIndexOf("dax30") + 6); 
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
            else if (message.ToLower().Contains("dj30") || message.ToLower().Contains("dowjones"))
            {
                string firstPartOfMessage = null;
                if (message.ToLower().Contains("dj30"))
                {
                    firstPartOfMessage = message.Substring(0, message.LastIndexOf("dj") + 4);
                    message = message.Substring(message.LastIndexOf("dj30") + 5);
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

        private static async Task HistoryFromBDD(TLMessage tLAbsMessage)
        {
            try
            {
                if (tLAbsMessage.ToId is TLPeerChannel channel)
                {
                    Console.WriteLine($"New Reply from channel {ChannelId[channel.ChannelId][0]} Message {tLAbsMessage.Message} to message {tLAbsMessage.ReplyToMsgId}");
                    Message message = DataAccess.GetMessageDataById(tLAbsMessage.ReplyToMsgId.Value);
                    TLChannelMessages rest = (TLChannelMessages)await Client.GetHistoryAsync(new TLInputPeerChannel() { ChannelId = channel.ChannelId, AccessHash = (long)ChannelId[channel.ChannelId][1] });
                    TLMessage tLMessage = (TLMessage)rest.Messages.ToList().FirstOrDefault(x => x is TLMessage tL && tL.Id == tLAbsMessage.ReplyToMsgId);
                    if (message != null)
                    {
                        if (tLMessage != null && tLMessage.Message != message.Text)
                        {
                            DataAccess.UpdateMessage(tLAbsMessage.ReplyToMsgId.Value, tLMessage.Message);
                            message.Text = tLMessage.Message;
                        }
                        Console.WriteLine($"REPLY{message.Text}\n{tLAbsMessage.Message}");
                        DataAccess.AddMessageData(tLAbsMessage.Id, channel.ChannelId, tLAbsMessage.Message);
                        await Client.SendMessageAsync(new TLInputPeerChannel() { ChannelId = MyChanId, AccessHash = AccessHash }, $"Channel {ChannelId[channel.ChannelId][0]}\n{message.Text}\nREPLY \n{tLAbsMessage.Message}");
                    }
                    else
                    {
                        Console.WriteLine("REPLY NOT HISTORIZED MESSAGE \n" + tLAbsMessage.Message);
                        DataAccess.AddMessageData(tLAbsMessage.Id, channel.ChannelId, tLAbsMessage.Message);
                        await Client.SendMessageAsync(new TLInputPeerChannel() { ChannelId = MyChanId, AccessHash = AccessHash }, $"Channel {ChannelId[channel.ChannelId][0]}\n NOT HISTORIZED MESSAGE \nREPLY \n {tLAbsMessage.Message}");
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
