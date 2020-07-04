using System.Collections.Generic;
using UnityEngine;

namespace ARConfigurator
{
    /// <summary>
    /// Initializes and holds a copy of the entire feature model.
    /// Responsible for interpreting the feature model at runtime, e.g., checking which features
    /// can be placed next to another feature, etc.
    /// </summary>
    public class ModelInterpreter : MonoBehaviour
    {
        private FeatureModel Model;


        public void Init(string featureModelJson)
        {
            Model = FeatureModel.FromJson(featureModelJson);
        }

        public Feature GetFeature(long featureId)
        {
            return Model.FeatureMap[featureId];
        }

        public List<Feature> GetAllPlaceable()
        {
            // Id = 1 corresponds to the root feature of the model.
            return GetPhysicalSubfeatures(1);
        }

        public List<Feature> GetAllMaterials()
        {
            var materialFeatures = new List<Feature>();

            var queue = new Queue<Feature>(Model.FeatureMap[1].Features);
            while (queue.Count > 0)
            {
                Feature currentFeature = queue.Dequeue();
                if (currentFeature.IsMaterial && currentFeature.Material != null)
                {
                    materialFeatures.Add(currentFeature);
                }

                foreach (Feature subfeature in currentFeature.Features)
                {
                    queue.Enqueue(subfeature);
                }
            }
            return materialFeatures;
        }

        public List<Feature> GetAllowedLeft(long featureId)
        {
            var list = new List<Feature>();
            foreach (long id in Model.FeatureMap[featureId].Metadata.LeftSlot)
            {
                foreach (Feature feature in GetPhysicalSubfeatures(id))
                {
                    if (!list.Contains(feature)) list.Add(feature);
                }
            }
            return list;
        }

        public List<Feature> GetAllowedRight(long featureId)
        {
            var list = new List<Feature>();
            foreach (long id in Model.FeatureMap[featureId].Metadata.RightSlot)
            {
                foreach (Feature feature in GetPhysicalSubfeatures(id))
                {
                    if (!list.Contains(feature)) list.Add(feature);
                }
            }
            return list;
        }

        public List<Feature> GetAllowedAbove(long featureId)
        {
            var list = new List<Feature>();
            foreach (long id in Model.FeatureMap[featureId].Metadata.UpperSlot)
            {
                foreach (Feature feature in GetPhysicalSubfeatures(id))
                {
                    if (!list.Contains(feature)) list.Add(feature);
                }
            }
            return list;
        }

        public List<Feature> GetMandatoryFeatures()
        {
            var list = new List<Feature>();
            foreach (var feature in Model.FeatureMap.Values)
            {
                if (feature.IsMandatory) { list.Add(feature); }
            }
            return list;
        }

        public List<Feature> GetXorFeatures()
        {
            var list = new List<Feature>();
            foreach (var feature in Model.FeatureMap.Values)
            {
                if (feature.HasXorSubfeatures) { list.Add(feature); }
            }
            return list;
        }

        public List<Feature> GetPhysicalSubfeatures(long rootFeatureId)
        {
            // Value of -1 corresponds to 'forbidden'.
            if (rootFeatureId == -1) return null;
            // Value of 0 corresponds to 'anything'.
            if (rootFeatureId == 0) return GetAllPlaceable();

            var rootFeature = Model.FeatureMap[rootFeatureId];
            var physicalSubfeatures = new List<Feature>();

            if (rootFeature.IsPhysical && rootFeature.Metadata != null)
            {
                physicalSubfeatures.Add(rootFeature);
                // Do not traverse down the feature tree if the root feature is itself physical.
                // This shouldn't be possible, i.e., physical features should not have children.
            }
            else
            {
                var queue = new Queue<Feature>(rootFeature.Features);
                while (queue.Count > 0)
                {
                    Feature currentFeature = queue.Dequeue();
                    if (currentFeature.IsPhysical && currentFeature.Metadata != null)
                    {
                        physicalSubfeatures.Add(currentFeature);
                    }

                    foreach (Feature subfeature in currentFeature.Features)
                    {
                        queue.Enqueue(subfeature);
                    }
                }
            }
            return physicalSubfeatures;
        }
    }
}
