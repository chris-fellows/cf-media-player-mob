using System.Xml.Serialization;

namespace CFMediaPlayer.Models
{
    /// <summary>
    /// Indexed item
    /// </summary>
    [XmlType("II")]
    public class IndexedItem
    {
        /// <summary>
        /// Values indicating which item(s) have been indexed
        /// </summary>
        [XmlArray("V")]
        [XmlArrayItem("VI")]
        public List<IndexedItemValue> Values = new List<IndexedItemValue>();

        /// <summary>
        /// Index items that can be searched.
        /// </summary>
        [XmlElement("I")]
        public string Items { get; set; } = String.Empty;

        /// <summary>
        /// Whether other item has same values
        /// </summary>
        /// <param name="otherIndexedItem"></param>
        /// <returns></returns>
        public bool IsSameValues(IndexedItem otherIndexedItem)
        {
            if (Values.Count == otherIndexedItem.Values.Count)
            {
                // Get keys
                List<string> namesThis = Values.Select(v => v.Name).ToList();
                namesThis.Sort();
                List<string> namesOther = otherIndexedItem.Values.Select(v => v.Name).ToList();
                namesOther.Sort();

                if (namesThis.SequenceEqual(namesOther))  // Keys the same
                {
                    foreach(var name in namesThis)
                    {
                        if (Values.First(v => v.Name == name).Value != otherIndexedItem.Values.First(v => v.Name == name).Value)
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            return false;
        }
    }
}
