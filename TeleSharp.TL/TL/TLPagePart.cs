using System.IO;
namespace TeleSharp.TL
{
    [TLObject(-1913754556)]
    public class TLPagePart : TLAbsPage
    {
        public override int Constructor
        {
            get
            {
                return -1913754556;
            }
        }

        public TLVector<TLAbsPageBlock> Blocks { get; set; }
        public TLVector<TLAbsPhoto> Photos { get; set; }
        public TLVector<TLAbsDocument> Videos { get; set; }


        public void ComputeFlags()
        {

        }

        public override void DeserializeBody(BinaryReader br)
        {
            Blocks = ObjectUtils.DeserializeVector<TLAbsPageBlock>(br);
            Photos = ObjectUtils.DeserializeVector<TLAbsPhoto>(br);
            Videos = ObjectUtils.DeserializeVector<TLAbsDocument>(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
            bw.Write(Constructor);
            ObjectUtils.SerializeObject(Blocks, bw);
            ObjectUtils.SerializeObject(Photos, bw);
            ObjectUtils.SerializeObject(Videos, bw);

        }
    }
}
