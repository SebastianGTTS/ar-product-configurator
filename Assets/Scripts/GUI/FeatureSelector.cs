using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ARConfigurator
{
    public class FeatureSelector : MonoBehaviour
    {
        public Transform ListContainer;
        public GameObject ListItemPrefab;

        public void DisplayFeatures(List<Feature> features)
        {
            // Clear the list.
            foreach (var listItem in GetComponentsInChildren<ListItemController>())
            {
                Destroy(listItem.gameObject);
            }

            // Repopulate the list.
            foreach (Feature feature in features)
            {
                var listItem = Instantiate(ListItemPrefab, ListContainer, false);
                listItem.GetComponent<ListItemController>().Init(
                    $"\"{feature.Metadata.Brand}\" {feature.Name}",
                    "$ " + feature.Metadata.Price.ToString()
                    );
                listItem.GetComponent<Button>().onClick.AddListener(() => ClickHandler(feature));
            }
        }

        private void ClickHandler(Feature clickedFeature)
        {
            SendMessageUpwards("FeatureSelected", clickedFeature.Id);
        }
    }
}
