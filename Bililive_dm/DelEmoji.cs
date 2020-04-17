using System.Text;

namespace Bililive_dm
{
    class DelEmoji
    {
        public static string delEmoji(string str)
        {
            string strout = " ";
            foreach (var a in str)
            {
                byte[] bts = Encoding.UTF32.GetBytes(a.ToString());

                if (!(bts[0].ToString() == "253" && bts[1].ToString() == "255"))
                {
                    //str = str.Replace(a.ToString(), "");
                    strout += a.ToString();
                }
            }
            return strout;
        }
    }
}
