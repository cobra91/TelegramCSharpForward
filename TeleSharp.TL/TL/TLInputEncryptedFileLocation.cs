using System.IO;
namespace TeleSharp.TL
{
    [TLObject(-182231723)]
    public class TLInputEncryptedFileLocation : TLAbsInputFileLocation
    {
        public override int Constructor
        {
            get
            {
                return -182231723;
            }
        }

        public long Id { get; set; }
        public long AccessHash { get; set; }


        public void ComputeFlags()
        {

        }

        public override void DeserializeBody(BinaryReader br)
        {
            Id = br.ReadInt64();
            AccessHash = br.ReadInt64();

        }

        public override void SerializeBody(BinaryWriter bw)
        {
            bw.Write(Constructor);
            bw.Write(Id);
            bw.Write(AccessHash);

        }
    }
}
