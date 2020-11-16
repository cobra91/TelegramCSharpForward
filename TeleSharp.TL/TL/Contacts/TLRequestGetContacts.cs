using System.IO;
namespace TeleSharp.TL.Contacts
{
    [TLObject(583445000)]
    public class TLRequestGetContacts : TLMethod
    {
        public override int Constructor
        {
            get
            {
                return 583445000;
            }
        }

        public string Hash { get; set; }
        public TLAbsContacts Response { get; set; }


        public void ComputeFlags()
        {

        }

        public override void DeserializeBody(BinaryReader br)
        {
            Hash = StringUtil.Deserialize(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
            bw.Write(Constructor);
            StringUtil.Serialize(Hash, bw);

        }
        public override void DeserializeResponse(BinaryReader br)
        {
            Response = (TLAbsContacts)ObjectUtils.DeserializeObject(br);

        }
    }
}
