using System.IO;
namespace TeleSharp.TL.Contacts
{
    [TLObject(1891070632)]
    public class TLTopPeers : TLAbsTopPeers
    {
        public override int Constructor
        {
            get
            {
                return 1891070632;
            }
        }

        public TLVector<TLTopPeerCategoryPeers> Categories { get; set; }
        public TLVector<TLAbsChat> Chats { get; set; }
        public TLVector<TLAbsUser> Users { get; set; }


        public void ComputeFlags()
        {

        }

        public override void DeserializeBody(BinaryReader br)
        {
            Categories = ObjectUtils.DeserializeVector<TLTopPeerCategoryPeers>(br);
            Chats = ObjectUtils.DeserializeVector<TLAbsChat>(br);
            Users = ObjectUtils.DeserializeVector<TLAbsUser>(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
            bw.Write(Constructor);
            ObjectUtils.SerializeObject(Categories, bw);
            ObjectUtils.SerializeObject(Chats, bw);
            ObjectUtils.SerializeObject(Users, bw);

        }
    }
}
