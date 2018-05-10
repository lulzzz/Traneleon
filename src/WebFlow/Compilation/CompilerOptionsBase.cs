using System.Collections;
using System.Collections.Generic;

namespace Acklann.WebFlow.Compilation
{
    public abstract class CompilerOptionsBase : ICompilierOptions, IReadOnlyDictionary<string, string>
    {
        public CompilerOptionsBase(Kind kind)
        {
            Kind = kind;
            _dictionary = new Dictionary<string, string>();
        }

        public int Count
        {
            get { return _dictionary.Count; }
        }

        public Kind Kind { get; }

        public IEnumerable<string> Keys
        {
            get { return _dictionary.Keys; }
        }

        public IEnumerable<string> Values
        {
            get { return _dictionary.Values; }
        }

        public string this[string key]
        {
            get { return _dictionary[key]; }
        }

        public bool ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool TryGetValue(string key, out string value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        protected T Add<T>(string key, T value)
        {
            _dictionary.Add(key.ToLowerInvariant(), value.ToString());
            return value;
        }

        #region Private Members

        private IDictionary<string, string> _dictionary;

        #endregion Private Members
    }
}