using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Net.Web.Api.Sdk.Configurations.Token
{
    /// <summary>
    /// Class TokenElementCollection.
    /// Keyed collection of <see cref="TokenElement" /> entries read from the <c>tokens</c> element.
    /// </summary>
    public class TokenElementCollection : IEnumerable<TokenElement>
    {
        #region Private Constants

        /// <summary>
        /// The collection element name
        /// </summary>
        private const string COLLECTION_ELEMENT_NAME = "token";

        #endregion

        #region Private Properties

        /// <summary>
        /// The elements
        /// </summary>
        private readonly List<TokenElement> _elements = new List<TokenElement>();

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _elements.Count;

        /// <summary>
        /// Gets the <see cref="TokenElement"/> with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>TokenElement.</returns>
        public TokenElement this[string key] =>
            _elements.FirstOrDefault(element => string.Equals(element.Name, key, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Gets the <see cref="TokenElement"/> with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>TokenElement.</returns>
        public TokenElement this[int id] => id >= 0 && id < _elements.Count ? _elements[id] : null;

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds an element to the collection.
        /// </summary>
        /// <param name="element">The element.</param>
        public void Add(TokenElement element)
        {
            if (element == null)
            {
                return;
            }

            _elements.Add(element);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Loads the collection from the <c>tokens</c> configuration element.
        /// </summary>
        /// <param name="collectionElement">The collection element.</param>
        /// <returns>TokenElementCollection.</returns>
        internal static TokenElementCollection Load(XElement collectionElement)
        {
            var collection = new TokenElementCollection();

            if (collectionElement == null)
            {
                return collection;
            }

            var tokenElements = collectionElement.Elements()
                .Where(element => element.Name.LocalName.Equals(COLLECTION_ELEMENT_NAME, StringComparison.OrdinalIgnoreCase));

            foreach (var tokenElement in tokenElements)
            {
                collection.Add(TokenElement.Load(tokenElement));
            }

            return collection;
        }

        #endregion

        #region IEnumerable Implementations

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator.</returns>
        public IEnumerator<TokenElement> GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
