using System.IO;
namespace TeleSharp.TL.Auth
{
    [TLObject(1056025023)]
    public class TLRequestResendCode : TLMethod
    {
        public override int Constructor
        {
            get
            {
                return 1056025023;
            }
        }

        public string PhoneNumber { get; set; }
        public string PhoneCodeHash { get; set; }
        public TLSentCode Response { get; set; }


        public void ComputeFlags()
        {

        }

        public override void DeserializeBody(BinaryReader br)
        {
            PhoneNumber = StringUtil.Deserialize(br);
            PhoneCodeHash = StringUtil.Deserialize(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
            bw.Write(Constructor);
            StringUtil.Serialize(PhoneNumber, bw);
            StringUtil.Serialize(PhoneCodeHash, bw);

        }
        public override void DeserializeResponse(BinaryReader br)
        {
            Response = (TLSentCode)ObjectUtils.DeserializeObject(br);

        }
    }
}
