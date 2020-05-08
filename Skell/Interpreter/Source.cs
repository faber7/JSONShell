namespace Skell.Interpreter
{
    public class Source
    {
        public readonly string source;
        public readonly int StartLine;
        public readonly int StartColumn;
        public readonly int EndLine;
        public readonly int EndColumn;

        /// <summary>
        /// If the source is user-input
        /// </summary>
        public Source(
            Antlr4.Runtime.IToken start,
            Antlr4.Runtime.IToken end
        )
        {
            source = "";
            StartLine = start.Line;
            StartColumn = start.Column;
            EndLine = end.Line;
            EndColumn = end.Column;
        }

        public Source(
            Antlr4.Runtime.IToken token
        )
        {
            source = "";
            StartLine = token.Line;
            StartColumn = token.StartIndex;
            EndLine = token.Line;
            EndColumn = token.StopIndex;
        }

        /// <summary>
        /// If the source is a file
        /// </summary>
        public Source(
            string filePath,
            Antlr4.Runtime.IToken start,
            Antlr4.Runtime.IToken end
        )
        {
            source = filePath;
            StartLine = start.Line;
            StartColumn = start.Column;
            EndLine = end.Line;
            EndColumn = end.Column;
        }

        override public string ToString()
        {
            string pos;
            if (StartLine == EndLine) {
                pos = $"line {StartLine}, columns {StartColumn}:{EndColumn}";
            } else {
                pos = $"lines {StartLine}:{StartColumn} to {EndLine}:{EndColumn}";
            }
            
            if (source == "") {
                return pos;
            } else {
                return $"{source}, {pos}";
            }
        }
    }
}