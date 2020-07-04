using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ARConfigurator
{
    public class MaterialSelector : MonoBehaviour
    {
        public Transform ListContainer;
        public GameObject ListItemPrefab;

        public void DisplayMaterials(List<Feature> features)
        {
            // Clear the list.
            foreach (var listItem in GetComponentsInChildren<ListItemController>())
            {
                Destroy(listItem.gameObject);
            }

            var restoreButton = Instantiate(ListItemPrefab, ListContainer, false);
            restoreButton.GetComponent<ListItemController>().Init($"<Reset Textures>", "");
            restoreButton.GetComponent<Button>().onClick.AddListener(() => RestoreHandler());

            // Repopulate the list.
            foreach (Feature feature in features)
            {
                var listItem = Instantiate(ListItemPrefab, ListContainer, false);
                listItem.GetComponent<ListItemController>().Init(
                    $"{feature.Name}",
                    "$ " + feature.Material.Price.ToString());
                listItem.GetComponent<Button>().onClick.AddListener(() => ClickHandler(feature));
            }
        }

        private void ClickHandler(Feature clickedFeature)
        {
            SendMessageUpwards("MaterialSelected", clickedFeature.Id);
        }

        private void RestoreHandler()
        {
            SendMessageUpwards("MaterialSelected", -1);
        }
    }
}
