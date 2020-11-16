using System.IO;
namespace TeleSharp.TL.Photos
{
    [TLObject(-1916114267)]
    public class TLPhotos : TLAbsPhotos
    {
        public override int Constructor
        {
            get
            {
                return -1916114267;
            }
        }

        public TLVector<TLAbsPhoto> Photos { get; set; }
        public TLVector<TLAbsUser> Users { get; set; }


        public void ComputeFlags()
        {

        }

        public override void DeserializeBody(BinaryReader br)
        {
            Photos = ObjectUtils.DeserializeVector<TLAbsPhoto>(br);
            Users = ObjectUtils.DeserializeVector<TLAbsUser>(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
            bw.Write(Constructor);
            ObjectUtils.SerializeObject(Photos, bw);
            ObjectUtils.SerializeObject(Users, bw);

        }
    }
}
