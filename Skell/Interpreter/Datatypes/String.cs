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
            return contents;
        }
    }
}