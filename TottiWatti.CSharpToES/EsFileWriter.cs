using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TottiWatti.CSharpToES
{
    public class EsFileWriter
    {
        private System.Text.StringBuilder _sb = new System.Text.StringBuilder();
        private int _indentation = 0;

        /// <summary>
        /// Indents code from this call onwards
        /// </summary>
        public void Indent()
        {
            _indentation++;
        }

        /// <summary>
        /// Outdents coden from this call onwards
        /// </summary>
        public void Outdent()
        {
            if (_indentation > 0)
            {
                _indentation--;
            }
           
        }

        /// <summary>
        /// Appends automatically intended line to writer
        /// </summary>
        /// <param name="Line"></param>
        public void AppendLine(string Line)
        {
            _sb.AppendLine(_IndentedString(Line));
        }        

        /// <summary>
        /// Appends empty line
        /// </summary>
        public void AppendLine()
        {
            _sb.AppendLine();
        }

        /// <summary>
        /// Appends trivia comments in JSDoc format
        /// </summary>
        /// <param name="triviaLines"></param>
        public void AppendTrivia(List<string> triviaLines)
        {
            if (triviaLines.Count > 0)
            {
                if (triviaLines.Count == 1)
                {
                    _sb.AppendLine(_IndentedString($"/** {triviaLines[0]} */"));
                }
                else
                {
                    _sb.AppendLine(_IndentedString("/**"));
                    for (var i = 0; i < triviaLines.Count; i++)
                    {
                        _sb.AppendLine(_IndentedString($"* {triviaLines[i]}"));
                    }
                    _sb.AppendLine(_IndentedString("*/"));
                }
            }
        }

        /// <summary>
        /// Returns provided string with current indentation
        /// </summary>
        /// <param name="s">String to indent</param>
        /// <returns></returns>
        private string _IndentedString(string s)
        {
            return s.PadLeft(s.Length + (this._indentation * 4));
        }
            
        /// <summary>
        /// Writes content to file path
        /// </summary>
        /// <param name="DestinationFile">File path</param>
        public void Write(string DestinationFile)
        {
            try
            {
                var destDir = Path.GetDirectoryName(DestinationFile);
                if (destDir != null && !Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }
                using (StreamWriter sw = new StreamWriter(DestinationFile))
                {
                    sw.Write(_sb.ToString());
                    sw.Close();
                }
                Console.WriteLine($"Destination file '{DestinationFile}' written succesfully" );
            }
            catch (Exception ex)
            {
                //var eex = ex;
                //throw;
                ConsoleException.Write($"EsFileWriter.Write(DestinationFile := {DestinationFile})", ex);
            }
        }


    }
}
