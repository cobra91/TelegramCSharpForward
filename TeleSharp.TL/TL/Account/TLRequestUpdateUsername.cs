using System.IO;
namespace TeleSharp.TL.Account
{
    [TLObject(1040964988)]
    public class TLRequestUpdateUsername : TLMethod
    {
        public override int Constructor
        {
            get
            {
                return 1040964988;
            }
        }

        public string Username { get; set; }
        public TLAbsUser Response { get; set; }


        public void ComputeFlags()
        {

        }

        public override void DeserializeBody(BinaryReader br)
        {
            Username = StringUtil.Deserialize(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
            bw.Write(Constructor);
            StringUtil.Serialize(Username, bw);

        }
        public override void DeserializeResponse(BinaryReader br)
        {
            Response = (TLAbsUser)ObjectUtils.DeserializeObject(br);

        }
    }
}
