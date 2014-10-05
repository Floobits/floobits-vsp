namespace Floobits.Common.Dmp
{
    public class FlooPatchPosition
    {
        public int start;
        public int end;
        public string text;

        public FlooPatchPosition(int start, int end, string text)
        {
            this.start = start;
            this.end = end;
            this.text = text;
        }
    }
}

