using System.IO;
namespace TeleSharp.TL.Contacts
{
    [TLObject(1871416498)]
    public class TLContacts : TLAbsContacts
    {
        public override int Constructor
        {
            get
            {
                return 1871416498;
            }
        }

        public TLVector<TLContact> Contacts { get; set; }
        public TLVector<TLAbsUser> Users { get; set; }


        public void ComputeFlags()
        {

        }

        public override void DeserializeBody(BinaryReader br)
        {
            Contacts = ObjectUtils.DeserializeVector<TLContact>(br);
            Users = ObjectUtils.DeserializeVector<TLAbsUser>(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
            bw.Write(Constructor);
            ObjectUtils.SerializeObject(Contacts, bw);
            ObjectUtils.SerializeObject(Users, bw);

        }
    }
}
