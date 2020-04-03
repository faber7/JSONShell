using System.Text;

namespace Skell.Data
{
    class String : SkellData
    {
        string contents;

        public String(string token)
        {
            contents = token;
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append("\"");
            s.Append(contents);
            s.Append("\"");
            return s.ToString();
        }
    }
}