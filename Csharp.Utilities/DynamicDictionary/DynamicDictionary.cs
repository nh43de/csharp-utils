/*    
    The MIT License (MIT)

    Copyright (c) 2014 Randy Burden ( http://randyburden.com ) All rights reserved.

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;

namespace DataPowerTools.DataStructures
{
    /// <summary>
    /// A dynamic dictionary allowing case-insensitive access and returns null when accessing non-existent properties.
    /// </summary>
    /// <example>
    /// // Non-existent properties will return null
    /// dynamic obj = new DynamicDictionary();
    /// var firstName = obj.FirstName;
    /// Assert.Null( firstName );
    /// 
    /// // Allows case-insensitive property access
    /// dynamic obj = new DynamicDictionary();
    /// obj.SuperHeroName = "Superman";
    /// Assert.That( obj.SUPERMAN == "Superman" );
    /// Assert.That( obj.superman == "Superman" );
    /// Assert.That( obj.sUpErMaN == "Superman" );
    /// </example>
    public class DynamicDictionary : DynamicObject, IDictionary<string, object>
    {
        private readonly IDictionary<string, object> _dictionary = new DefaultValueDictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

        #region DynamicObject Overrides

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = _dictionary[binder.Name];

            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (_dictionary.ContainsKey(binder.Name))
            {
                _dictionary[binder.Name] = value;
            }
            else
            {
                _dictionary.Add(binder.Name, value);
            }

            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (_dictionary.ContainsKey(binder.Name) && _dictionary[binder.Name] is Delegate)
            {
                var delegateValue = _dictionary[binder.Name] as Delegate;

                result = delegateValue.DynamicInvoke(args);

                return true;
            }

            return base.TryInvokeMember(binder, args, out result);
        }

        #endregion DynamicObject Overrides

        #region IDictionary<string,object> Members

        public void Add(string key, object value)
        {
            _dictionary.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return _dictionary.Keys; }
        }

        public bool Remove(string key)
        {
            return _dictionary.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public ICollection<object> Values
        {
            get { return _dictionary.Values; }
        }

        public object this[string key]
        {
            get
            {
                object value = null;

                _dictionary.TryGetValue(key, out value);

                return value;
            }
            set { _dictionary[key] = value; }
        }

        #endregion IDictionary<string,object> Members

        #region ICollection<KeyValuePair<string,object>> Members

        public void Add(KeyValuePair<string, object> item)
        {
            _dictionary.Add(item);
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return _dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            _dictionary.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _dictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return _dictionary.IsReadOnly; }
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return _dictionary.Remove(item);
        }

        #endregion ICollection<KeyValuePair<string,object>> Members

        #region IEnumerable<KeyValuePair<string,object>> Members

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        #endregion IEnumerable<KeyValuePair<string,object>> Members

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        #endregion IEnumerable Members

    }
}