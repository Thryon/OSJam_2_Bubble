using TMPro;
using UnityEngine;

public class PlayerJoinBoxUI : MonoBehaviour
{
    [SerializeField] TMP_Text playerNameText;
    [SerializeField] RectTransform JoinedTransform;
    [SerializeField] RectTransform WaitForInputTransform;

    public void SetJoined(bool joined)
    {
        JoinedTransform.gameObject.SetActive(joined);
        WaitForInputTransform.gameObject.SetActive(!joined);
    }
}
