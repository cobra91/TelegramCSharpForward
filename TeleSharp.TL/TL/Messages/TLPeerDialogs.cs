using System.IO;
namespace TeleSharp.TL.Messages
{
    [TLObject(863093588)]
    public class TLPeerDialogs : TLObject
    {
        public override int Constructor
        {
            get
            {
                return 863093588;
            }
        }

        public TLVector<TLDialog> Dialogs { get; set; }
        public TLVector<TLAbsMessage> Messages { get; set; }
        public TLVector<TLAbsChat> Chats { get; set; }
        public TLVector<TLAbsUser> Users { get; set; }
        public Updates.TLState State { get; set; }


        public void ComputeFlags()
        {

        }

        public override void DeserializeBody(BinaryReader br)
        {
            Dialogs = ObjectUtils.DeserializeVector<TLDialog>(br);
            Messages = ObjectUtils.DeserializeVector<TLAbsMessage>(br);
            Chats = ObjectUtils.DeserializeVector<TLAbsChat>(br);
            Users = ObjectUtils.DeserializeVector<TLAbsUser>(br);
            State = (Updates.TLState)ObjectUtils.DeserializeObject(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
            bw.Write(Constructor);
            ObjectUtils.SerializeObject(Dialogs, bw);
            ObjectUtils.SerializeObject(Messages, bw);
            ObjectUtils.SerializeObject(Chats, bw);
            ObjectUtils.SerializeObject(Users, bw);
            ObjectUtils.SerializeObject(State, bw);

        }
    }
}
