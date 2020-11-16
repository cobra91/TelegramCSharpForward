using System.IO;
namespace TeleSharp.TL.Contacts
{
    [TLObject(-1902823612)]
    public class TLRequestDeleteContact : TLMethod
    {
        public override int Constructor
        {
            get
            {
                return -1902823612;
            }
        }

        public TLAbsInputUser Id { get; set; }
        public TLLink Response { get; set; }


        public void ComputeFlags()
        {

        }

        public override void DeserializeBody(BinaryReader br)
        {
            Id = (TLAbsInputUser)ObjectUtils.DeserializeObject(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
            bw.Write(Constructor);
            ObjectUtils.SerializeObject(Id, bw);

        }
        public override void DeserializeResponse(BinaryReader br)
        {
            Response = (TLLink)ObjectUtils.DeserializeObject(br);

        }
    }
}
