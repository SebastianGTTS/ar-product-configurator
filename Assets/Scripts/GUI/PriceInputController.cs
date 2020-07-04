using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PriceInputController : MonoBehaviour
{
    public Button ConfirmButton;
    public Button ResetButton;
    public InputField Input;
    public Text CurrentLimitDisplay;

    void Start()
    {
        ConfirmButton.onClick.AddListener(() => InputHandler());
        ResetButton.onClick.AddListener(() => ResetHandler());
    }

    public void ShowCurrentLimit(double limit)
    {
        Input.text = "";

        if (limit > 0)
        {
            CurrentLimitDisplay.text = $"Current limit: {limit}";
        }
        else
        {
            CurrentLimitDisplay.text = "Current limit: ---";
        }
    }

    private void InputHandler()
    {
        double newLimit;
        if (Double.TryParse(Input.text, out newLimit))
        {
            if (newLimit > 0)
            {
                ShowCurrentLimit(newLimit);
                SendMessageUpwards("PriceLimitChanged", newLimit);
            }
        }
    }

    private void ResetHandler()
    {
        ShowCurrentLimit(-1.0);
        SendMessageUpwards("PriceLimitChanged", -1.0);
    }
}
