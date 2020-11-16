using System.IO;
namespace TeleSharp.TL.Messages
{
    [TLObject(-497756594)]
    public class TLRequestGetPinnedDialogs : TLMethod
    {
        public override int Constructor
        {
            get
            {
                return -497756594;
            }
        }

        public TLPeerDialogs Response { get; set; }


        public void ComputeFlags()
        {

        }

        public override void DeserializeBody(BinaryReader br)
        {

        }

        public override void SerializeBody(BinaryWriter bw)
        {
            bw.Write(Constructor);

        }
        public override void DeserializeResponse(BinaryReader br)
        {
            Response = (TLPeerDialogs)ObjectUtils.DeserializeObject(br);

        }
    }
}
