using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rumle.Golang
{
    /// <summary>
    /// Golang-like select, remember to run the Run() method when all the cases have been defined
    /// </summary>
    public class Select
    {
        private Action _defaultBody;
        private readonly List<ThreadStart> _cases = new List<ThreadStart>();
        private bool _foundCase = false;

        /// <summary>
        /// Defines a select case body for the given channel
        /// </summary>
        /// <param name="channel">the channel to listen on</param>
        /// <param name="caseBody">the method to run if a vakue is received</param>
        public Select Case<T>(Channel<T> channel, Action<T> caseBody)
        {
            var ts = new ThreadStart(() =>
            {
                object @lock = null;
                if (channel.TryEnterExclusive(out @lock))
                {
                    if (!_foundCase)
                    {
                        _foundCase = true;
                        var item = channel.Receive();
                        caseBody(item);
                    }
                    Monitor.Exit(@lock);
                }
            });

            _cases.Add(ts);

            return this;
        }

        /// <summary>
        /// The default select case
        /// </summary>
        /// <param name="defaultBody">the default select case method body</param>
        public Select Default(Action defaultBody)
        {
            _defaultBody = defaultBody;
            return this;
        }

        /// <summary>
        /// This methods hooks everything up, remember to call this after defining the cases
        /// </summary>
        public void Run()
        {
            var threads = _cases.Select(ts => new Thread(ts)).ToList();
            threads.ForEach(t => t.Start());
            threads.ForEach(t => t.Join());

            if (!_foundCase)
            {
                if (_defaultBody != null)
                {
                    _defaultBody();
                }
                else
                {
                    Run();
                }
            }
        }
    }
}
