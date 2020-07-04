using UnityEngine;
using UnityEngine.UI;

namespace ARConfigurator
{
    public class ListItemController : MonoBehaviour
    {
        public Text FirstLine;
        public Text SecondLine;

        public void Init(string firstLine, string secondLine)
        {
            FirstLine.text = firstLine;
            SecondLine.text = secondLine;
        }
    }
}
