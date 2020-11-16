using System.IO;
namespace TeleSharp.TL.Contacts
{
    [TLObject(471043349)]
    public class TLBlocked : TLAbsBlocked
    {
        public override int Constructor
        {
            get
            {
                return 471043349;
            }
        }

        public TLVector<TLContactBlocked> Blocked { get; set; }
        public TLVector<TLAbsUser> Users { get; set; }


        public void ComputeFlags()
        {

        }

        public override void DeserializeBody(BinaryReader br)
        {
            Blocked = ObjectUtils.DeserializeVector<TLContactBlocked>(br);
            Users = ObjectUtils.DeserializeVector<TLAbsUser>(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
            bw.Write(Constructor);
            ObjectUtils.SerializeObject(Blocked, bw);
            ObjectUtils.SerializeObject(Users, bw);

        }
    }
}
