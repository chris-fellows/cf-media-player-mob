//using CFMediaPlayer.Interfaces;
//using CFMediaPlayer.Models;
//using CFMediaPlayer.Utilities;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CFMediaPlayer.Services
//{
//    /// <summary>
//    /// Indexed data in XML format
//    /// </summary>
//    internal class XmlIndexedData : IIndexedData
//    {
//        private readonly string _folder;

//        public XmlIndexedData(string folder)
//        {
//            _folder = folder;
//        }

//        public void Clear(string category)
//        {
//            var indexFile = Path.Combine(_folder, $"Index-{category}.xml");
//            if (File.Exists(indexFile))
//            {
//                File.Delete(indexFile);
//            }
//        }

//        public void Write(List<IndexedItem> items, string category)
//        {
//            Directory.CreateDirectory(_folder);

//            // Load index
//            var indexFile = Path.Combine(_folder, $"Index-{category}.xml");
//            var currentData = File.Exists(indexFile) ? XmlUtilities.DeserializeFromString<List<IndexedItem>>(File.ReadAllText(indexFile, Encoding.UTF8)) :
//                                new List<IndexedItem>();

//            // Add items, removing existing item
//            foreach(var item in items)
//            {
//                var currentItem = currentData.Where(i => i.IsSameValues(item)).FirstOrDefault();
//                if (currentItem != null)
//                {
//                    currentData.Remove(currentItem);
//                }
//                currentData.Add(item);
//            }

//            File.WriteAllText(indexFile, XmlUtilities.SerializeToString(currentData), Encoding.UTF8);            
//        }

//        public IEnumerable<IndexedItem> Search(string text, List<string> categories)
//        {
//            var items = new List<IndexedItem>();
            
//            // Get index files to search
//            var indexFiles = new List<string>();
//            foreach (var category in categories)
//            {
//                indexFiles.Add(Path.Combine(_folder, $"Index-{category}.xml"));
//            }

//            Parallel.ForEach(indexFiles,
//                            new ParallelOptions { MaxDegreeOfParallelism = 4 },
//                            indexFile =>
//                            {
//                                var fileItems = SearchFile(categories[indexFiles.IndexOf(indexFile)], indexFile, text);
//                                items.AddRange(fileItems);
//                            });

//            return items;
//        }       

//        private IEnumerable<IndexedItem> SearchFile(string category, string indexFile, string text)
//        {
//            Char elementSplit = (Char)0;

//            var indexedItemsFound = new List<IndexedItem>();

//            var indexedItems = File.Exists(indexFile) ? XmlUtilities.DeserializeFromString<List<IndexedItem>>(File.ReadAllText(indexFile, Encoding.UTF8)) :
//                                    new List<IndexedItem>();    

//            foreach(var indexedItem in indexedItems)
//            {
//                var elements = indexedItem.Items.Split(elementSplit);
//                foreach(var element in elements)
//                {
//                    if (element.Contains(text, StringComparison.InvariantCultureIgnoreCase))
//                    {
//                        indexedItemsFound.Add(indexedItem);
//                        break;
//                    }
//                }          
//            }

//            return indexedItemsFound;
//        }
//    }
//}
