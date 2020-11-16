using System.IO;
namespace TeleSharp.TL.Messages
{
    [TLObject(-1725551049)]
    public class TLChannelMessages : TLAbsMessages
    {
        public override int Constructor
        {
            get
            {
                return -1725551049;
            }
        }

        public int Flags { get; set; }
        public int Pts { get; set; }
        public int Count { get; set; }
        public TLVector<TLAbsMessage> Messages { get; set; }
        public TLVector<TLAbsChat> Chats { get; set; }
        public TLVector<TLAbsUser> Users { get; set; }


        public void ComputeFlags()
        {
            Flags = 0;

        }

        public override void DeserializeBody(BinaryReader br)
        {
            Flags = br.ReadInt32();
            Pts = br.ReadInt32();
            Count = br.ReadInt32();
            Messages = ObjectUtils.DeserializeVector<TLAbsMessage>(br);
            Chats = ObjectUtils.DeserializeVector<TLAbsChat>(br);
            Users = ObjectUtils.DeserializeVector<TLAbsUser>(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
            bw.Write(Constructor);
            ComputeFlags();
            bw.Write(Flags);
            bw.Write(Pts);
            bw.Write(Count);
            ObjectUtils.SerializeObject(Messages, bw);
            ObjectUtils.SerializeObject(Chats, bw);
            ObjectUtils.SerializeObject(Users, bw);

        }
    }
}
