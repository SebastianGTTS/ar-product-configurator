using UnityEngine;
using System.Collections.Generic;

namespace ARConfigurator
{
    /// <summary>
    /// Stores and initializes the element's metadata.
    /// </summary>
    public class ElementMetadata : MonoBehaviour
    {
        // Associated feature properties.
        public long FeatureId;
        public string Name;

        // Associated metadata properties.
        public string Brand;
        public string ModelFilename;
        public double Price;
        public List<long> LeftSlot;
        public List<long> RightSlot;
        public List<long> UpperSlot;

        public void Init(Feature associatedFeature)
        {
            FeatureId = associatedFeature.Id;
            Name = associatedFeature.Name;

            Brand = associatedFeature.Metadata.Brand;
            ModelFilename = associatedFeature.Metadata.ModelFilename;
            Price = associatedFeature.Metadata.Price;
            LeftSlot = associatedFeature.Metadata.LeftSlot;
            RightSlot = associatedFeature.Metadata.RightSlot;
            UpperSlot = associatedFeature.Metadata.UpperSlot;
        }
    }
}
