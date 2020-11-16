using System.IO;
namespace TeleSharp.TL.Updates
{
    [TLObject(-1459938943)]
    public class TLDifferenceSlice : TLAbsDifference
    {
        public override int Constructor
        {
            get
            {
                return -1459938943;
            }
        }

        public TLVector<TLAbsMessage> NewMessages { get; set; }
        public TLVector<TLAbsEncryptedMessage> NewEncryptedMessages { get; set; }
        public TLVector<TLAbsUpdate> OtherUpdates { get; set; }
        public TLVector<TLAbsChat> Chats { get; set; }
        public TLVector<TLAbsUser> Users { get; set; }
        public TLState IntermediateState { get; set; }


        public void ComputeFlags()
        {

        }

        public override void DeserializeBody(BinaryReader br)
        {
            NewMessages = ObjectUtils.DeserializeVector<TLAbsMessage>(br);
            NewEncryptedMessages = ObjectUtils.DeserializeVector<TLAbsEncryptedMessage>(br);
            OtherUpdates = ObjectUtils.DeserializeVector<TLAbsUpdate>(br);
            Chats = ObjectUtils.DeserializeVector<TLAbsChat>(br);
            Users = ObjectUtils.DeserializeVector<TLAbsUser>(br);
            IntermediateState = (TLState)ObjectUtils.DeserializeObject(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
            bw.Write(Constructor);
            ObjectUtils.SerializeObject(NewMessages, bw);
            ObjectUtils.SerializeObject(NewEncryptedMessages, bw);
            ObjectUtils.SerializeObject(OtherUpdates, bw);
            ObjectUtils.SerializeObject(Chats, bw);
            ObjectUtils.SerializeObject(Users, bw);
            ObjectUtils.SerializeObject(IntermediateState, bw);

        }
    }
}
