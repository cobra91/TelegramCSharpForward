using System.IO;
namespace TeleSharp.TL
{
    [TLObject(319588707)]
    public class TLPageBlockSlideshow : TLAbsPageBlock
    {
        public override int Constructor
        {
            get
            {
                return 319588707;
            }
        }

        public TLVector<TLAbsPageBlock> Items { get; set; }
        public TLAbsRichText Caption { get; set; }


        public void ComputeFlags()
        {

        }

        public override void DeserializeBody(BinaryReader br)
        {
            Items = ObjectUtils.DeserializeVector<TLAbsPageBlock>(br);
            Caption = (TLAbsRichText)ObjectUtils.DeserializeObject(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
            bw.Write(Constructor);
            ObjectUtils.SerializeObject(Items, bw);
            ObjectUtils.SerializeObject(Caption, bw);

        }
    }
}
