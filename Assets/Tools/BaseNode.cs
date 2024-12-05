using UnityEditor.Experimental.GraphView;

namespace QuizGraphEditor
{
    public class BaseNode : Node
    {
        public string GUID { get; protected set; }

        public void UpdatePortColors()
        {
            foreach (var port in inputContainer.Children())
            {
                port.style.backgroundColor = UserPreferences.PortColor;
            }
            foreach (var port in outputContainer.Children())
            {
                port.style.backgroundColor = UserPreferences.PortColor;
            }
        }

    }
}
