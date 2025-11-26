using UnityEngine;
using UnityEngine.UI;

namespace MyTestTask.Misc
{
    public class CloseAppButton : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(Application.Quit);
        }
    }
}
