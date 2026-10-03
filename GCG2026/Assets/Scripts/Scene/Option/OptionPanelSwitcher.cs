using UnityEngine;

public class OptionPanelSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private GameObject graphicPanel;

    public void ShowAudio()
    {
        ShowOnly(audioPanel);
    }

    public void ShowGraphic()
    {
        ShowOnly(graphicPanel);
    }

    private void ShowOnly(GameObject panelToShow)
    {
        if (audioPanel != null) audioPanel.SetActive(false);
        if (graphicPanel != null) graphicPanel.SetActive(false);

        if (panelToShow != null) panelToShow.SetActive(true);
    }
}
