using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateUserMenu : MonoBehaviour
{
    [SerializeField]
    private Button createButton = null;

    [SerializeField]
    private TMP_InputField inputField = null;

    public string UserName
    {
        get { return inputField.text; }
    }

    private void Awake()
    {
        createButton.interactable = false;
        string oldName = PlayerPrefs.GetString("UserName", "");
        if (oldName.Length > 0)
        {
            inputField.text = oldName;
            createButton.interactable = true;
        }
    }

    public void OnInputFieldChange()
    {
        createButton.interactable = inputField.text.Length > 0;
    }
}