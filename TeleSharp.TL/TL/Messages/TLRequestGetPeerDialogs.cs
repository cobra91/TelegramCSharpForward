using System.IO;
namespace TeleSharp.TL.Messages
{
    [TLObject(764901049)]
    public class TLRequestGetPeerDialogs : TLMethod
    {
        public override int Constructor
        {
            get
            {
                return 764901049;
            }
        }

        public TLVector<TLAbsInputPeer> Peers { get; set; }
        public TLPeerDialogs Response { get; set; }


        public void ComputeFlags()
        {

        }

        public override void DeserializeBody(BinaryReader br)
        {
            Peers = ObjectUtils.DeserializeVector<TLAbsInputPeer>(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
            bw.Write(Constructor);
            ObjectUtils.SerializeObject(Peers, bw);

        }
        public override void DeserializeResponse(BinaryReader br)
        {
            Response = (TLPeerDialogs)ObjectUtils.DeserializeObject(br);

        }
    }
}
