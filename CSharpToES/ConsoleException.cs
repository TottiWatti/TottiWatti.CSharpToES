using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TottiWatti.CSharpToES
{
    static class ConsoleException
    {
        public static void Write(string Source, Exception ex)
        {
            Exception? _ex = ex;
            StringBuilder _StringBuilder = new StringBuilder();            

            if (_ex != null)
            {
                if (!string.IsNullOrEmpty(Source))
                {
                    _StringBuilder.Append($"{Source} caught exception:\n");
                }
                
                string exceptionString = _ex.ToString();
                _StringBuilder.Append($"Exception: {exceptionString}");
                if (!string.IsNullOrEmpty(_ex.StackTrace) && !exceptionString.Contains(_ex.StackTrace))
                    _StringBuilder.Append($"\nException stack trace: {_ex.StackTrace}");

                while (_ex.InnerException != null)
                {
                    exceptionString = _ex.InnerException.ToString();
                    _StringBuilder.Append($"\nInner exception: {exceptionString}");
                    if (!string.IsNullOrEmpty(_ex.InnerException.StackTrace) && !exceptionString.Contains(_ex.InnerException.StackTrace))
                        _StringBuilder.Append($"\nInner exception stack trace: {_ex.InnerException.StackTrace}");
                    _ex = _ex.InnerException;
                }
            }
            Console.WriteLine(_StringBuilder.ToString());
            _StringBuilder.Clear();
        }

        

    }
}
