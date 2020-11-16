using System.IO;
namespace TeleSharp.TL.Channels
{
    [TLObject(-177282392)]
    public class TLChannelParticipants : TLObject
    {
        public override int Constructor
        {
            get
            {
                return -177282392;
            }
        }

        public int Count { get; set; }
        public TLVector<TLAbsChannelParticipant> Participants { get; set; }
        public TLVector<TLAbsUser> Users { get; set; }


        public void ComputeFlags()
        {

        }

        public override void DeserializeBody(BinaryReader br)
        {
            Count = br.ReadInt32();
            Participants = ObjectUtils.DeserializeVector<TLAbsChannelParticipant>(br);
            Users = ObjectUtils.DeserializeVector<TLAbsUser>(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
            bw.Write(Constructor);
            bw.Write(Count);
            ObjectUtils.SerializeObject(Participants, bw);
            ObjectUtils.SerializeObject(Users, bw);

        }
    }
}
