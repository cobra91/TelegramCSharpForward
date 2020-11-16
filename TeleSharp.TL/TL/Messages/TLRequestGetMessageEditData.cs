using System.IO;
namespace TeleSharp.TL.Messages
{
    [TLObject(-39416522)]
    public class TLRequestGetMessageEditData : TLMethod
    {
        public override int Constructor
        {
            get
            {
                return -39416522;
            }
        }

        public TLAbsInputPeer Peer { get; set; }
        public int Id { get; set; }
        public TLMessageEditData Response { get; set; }


        public void ComputeFlags()
        {

        }

        public override void DeserializeBody(BinaryReader br)
        {
            Peer = (TLAbsInputPeer)ObjectUtils.DeserializeObject(br);
            Id = br.ReadInt32();

        }

        public override void SerializeBody(BinaryWriter bw)
        {
            bw.Write(Constructor);
            ObjectUtils.SerializeObject(Peer, bw);
            bw.Write(Id);

        }
        public override void DeserializeResponse(BinaryReader br)
        {
            Response = (TLMessageEditData)ObjectUtils.DeserializeObject(br);

        }
    }
}
