using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ARConfigurator
{
    public class GUIController : MonoBehaviour
    {
        public ConfigurationManager ConfigurationManager;
        public GameObject MenuButton;
        public GameObject MainMenu;
        public FeatureSelector FeatureSelector;
        public MaterialSelector MaterialSelector;
        public PriceInputController PriceInput;
        public GameObject LoadingScreen;
        public GameObject ValidationOutput;

        public void ReadyGUI()
        {
            LoadingScreen.SetActive(false);
            ShowMenuButton();
        }

        // Opening and closing of UI elements.
        public void ResetGUI()
        {
            CloseFeatureSelector();
            CloseMaterialSelector();
            ClosePriceInput();
            CloseMainMenu();
            ShowMenuButton();
        }

        public void HideMenuButton()
        {
            MenuButton.SetActive(false);
        }

        public void ShowMenuButton()
        {
            MenuButton.SetActive(true);
        }

        public void CloseFeatureSelector()
        {
            FeatureSelector.gameObject.SetActive(false);
            ShowMenuButton();
        }

        public void OpenFeatureSelector()
        {
            ResetGUI();
            HideMenuButton();
            FeatureSelector.gameObject.SetActive(true);
        }

        public void CloseMainMenu()
        {
            MainMenu.SetActive(false);
            ShowMenuButton();
        }

        public void OpenMainMenu()
        {
            ResetGUI();
            HideMenuButton();
            MainMenu.SetActive(true);
        }

        public void CloseMaterialSelector()
        {
            MaterialSelector.gameObject.SetActive(false);
            ShowMenuButton();
        }

        public void OpenMaterialSelector()
        {
            ResetGUI();
            HideMenuButton();
            MaterialSelector.gameObject.SetActive(true);
        }

        public void ClosePriceInput()
        {
            PriceInput.gameObject.SetActive(false);
            ShowMenuButton();
        }

        public void OpenPriceInput()
        {
            ResetGUI();
            HideMenuButton();
            PriceInput.ShowCurrentLimit(ConfigurationManager.GetPriceLimit());
            PriceInput.gameObject.SetActive(true);
        }

        public void ShowValidationResults()
        {
            var vtext = ValidationOutput.GetComponentInChildren<Text>();
            vtext.text = "Hi!";
            var res = ConfigurationManager.ValidateConfiguration();
            vtext.text = res;
            ResetGUI();
            HideMenuButton();
            ValidationOutput.SetActive(true);
        }

        public void HideValidationResults()
        {
            ValidationOutput.SetActive(false);
            ShowMenuButton();
        }

        // Filling lists.
        public void SetSelectableFeatures(List<Feature> features)
        {
            FeatureSelector.DisplayFeatures(features);
        }

        public void SetSelectableMaterials(List<Feature> features)
        {
            MaterialSelector.DisplayMaterials(features);
        }

        // UI messages. These will be called by GUI sub-components when appropriate.
        public void FeatureSelected(long reportedFeatureId)
        {
            CloseFeatureSelector();
            ConfigurationManager.OnFeatureSelected(reportedFeatureId);
        }

        public void MaterialSelected(long reportedFeatureId)
        {
            CloseMaterialSelector();
            ConfigurationManager.OnMaterialSelected(reportedFeatureId);
        }

        public void PriceLimitChanged(double newLimit)
        {
            ConfigurationManager.SetPriceLimit(newLimit);
            ClosePriceInput();
        }
    }
}
