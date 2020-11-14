using Microsoft.Data.Sqlite;

using System;
using System.Collections.Generic;
using System.IO;

namespace DataAccessLibrary
{
    // Class which manages the SQLite database part
    public static class DataAccess
    {
        private static string directoryDB = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TLSharp";
        private static string dbpath = directoryDB + "\\sqliteTelegramHistory.db";

        public static void InitializeDatabase()
        {
            if (!Directory.Exists(directoryDB))
            {
                Directory.CreateDirectory(directoryDB);
            }
            if (!File.Exists(dbpath))
            {
                File.Create(dbpath);
            }
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                string tableCommand = "CREATE TABLE IF NOT EXISTS Message (Id INTEGER PRIMARY KEY, ChanId INT NOT NULL, Message TEXT NULL)";
                SqliteCommand createTable = new SqliteCommand(tableCommand, db);
                createTable.ExecuteReader();

                tableCommand = "CREATE TABLE IF NOT EXISTS Channel (Id INTEGER PRIMARY KEY, Title TEXT NOT NULL, AccessHash BIGINT NOT NULL)";
                createTable = new SqliteCommand(tableCommand, db);
                createTable.ExecuteReader();
            }
        }

        public static void AddMessageData(int id, int chanId, string message)
        {
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();
                SqliteCommand insertCommand = new SqliteCommand
                {
                    Connection = db,
                    CommandText = "INSERT INTO Message(Id, ChanId, Message) VALUES (" + id + ", " + chanId + ", '" + message + "')"
                };
                insertCommand.ExecuteReader();
                db.Close();
            }
        }

        public static void AddChannelData(int id, string title, long accessHash)
        {
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();
                SqliteCommand insertCommand = new SqliteCommand
                {
                    Connection = db,
                    CommandText = "INSERT INTO Channel(Id, title, AccessHash) VALUES (" + id + ", '" + title + "', " + accessHash + ")"
                };
                insertCommand.ExecuteReader();
                db.Close();
            }
        }

        public static List<Message> GetMessageData()
        {
            List<Message> messageList = new List<Message>();

            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand("SELECT * from Message", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    Message message = new Message
                    {
                        Id = query.GetInt32(0),
                        ChanId = query.GetInt32(1),
                        Text = query.GetString(2)
                    };
                    messageList.Add(message);
                }
                db.Close();
            }
            return messageList;
        }

        public static Message GetMessageDataById(int idMessage)
        {
            Message message = null;
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand("SELECT * from Message where Id = " + idMessage, db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    message = new Message
                    {
                        Id = query.GetInt32(0),
                        ChanId = query.GetInt32(1),
                        Text = query.GetString(2)
                    };
                }
                db.Close();
            }
            return message;
        }

        public static List<Channel> GetChannelData()
        {
            List<Channel> channelList = new List<Channel>();
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand("SELECT * from Channel", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    Channel channel = new Channel
                    {
                        Id = query.GetInt32(0),
                        Title = query.GetString(1),
                        AccessHash = query.GetInt64(2)
                    };
                    channelList.Add(channel);
                }
                db.Close();
            }
            return channelList;
        }

        public static void UpdateMessage(int id, string message)
        {
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();
                SqliteCommand removeCommand = new SqliteCommand("UPDATE Message SET Message = '" + message + "' WHERE Id = " + id, db);
                removeCommand.ExecuteReader();
                db.Close();
            }
        }

        public static void RemoveChannel(int id)
        {
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();
                SqliteCommand removeCommand = new SqliteCommand("DELETE from Channel WHERE Id = " + id, db);
                removeCommand.ExecuteReader();
                db.Close();
            }
        }

        public static void RemoveMessageFromChannel(int chanId)
        {
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();
                SqliteCommand removeCommand = new SqliteCommand("DELETE from Message WHERE ChanId = " + chanId, db);
                removeCommand.ExecuteReader();
                db.Close();
            }
        }
    }

    public class Message
    {
        public int Id;
        public int ChanId;
        public string Text;
    }

    public class Channel
    {
        public int Id;
        public string Title;
        public long AccessHash;
    }
}
