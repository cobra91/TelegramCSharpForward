using System.IO;
namespace TeleSharp.TL.Auth
{
    [TLObject(1877286395)]
    public class TLRequestCheckPhone : TLMethod
    {
        public override int Constructor
        {
            get
            {
                return 1877286395;
            }
        }

        public string PhoneNumber { get; set; }
        public TLCheckedPhone Response { get; set; }


        public void ComputeFlags()
        {

        }

        public override void DeserializeBody(BinaryReader br)
        {
            PhoneNumber = StringUtil.Deserialize(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
            bw.Write(Constructor);
            StringUtil.Serialize(PhoneNumber, bw);

        }
        public override void DeserializeResponse(BinaryReader br)
        {
            Response = (TLCheckedPhone)ObjectUtils.DeserializeObject(br);

        }
    }
}
