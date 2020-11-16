using System.IO;
namespace TeleSharp.TL.Photos
{
    [TLObject(1328726168)]
    public class TLRequestUploadProfilePhoto : TLMethod
    {
        public override int Constructor
        {
            get
            {
                return 1328726168;
            }
        }

        public TLAbsInputFile File { get; set; }
        public TLPhoto Response { get; set; }


        public void ComputeFlags()
        {

        }

        public override void DeserializeBody(BinaryReader br)
        {
            File = (TLAbsInputFile)ObjectUtils.DeserializeObject(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
            bw.Write(Constructor);
            ObjectUtils.SerializeObject(File, bw);

        }
        public override void DeserializeResponse(BinaryReader br)
        {
            Response = (TLPhoto)ObjectUtils.DeserializeObject(br);

        }
    }
}
