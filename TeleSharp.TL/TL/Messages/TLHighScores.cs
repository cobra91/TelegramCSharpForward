using System.IO;
namespace TeleSharp.TL.Messages
{
    [TLObject(-1707344487)]
    public class TLHighScores : TLObject
    {
        public override int Constructor
        {
            get
            {
                return -1707344487;
            }
        }

        public TLVector<TLHighScore> Scores { get; set; }
        public TLVector<TLAbsUser> Users { get; set; }


        public void ComputeFlags()
        {

        }

        public override void DeserializeBody(BinaryReader br)
        {
            Scores = ObjectUtils.DeserializeVector<TLHighScore>(br);
            Users = ObjectUtils.DeserializeVector<TLAbsUser>(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
            bw.Write(Constructor);
            ObjectUtils.SerializeObject(Scores, bw);
            ObjectUtils.SerializeObject(Users, bw);

        }
    }
}
