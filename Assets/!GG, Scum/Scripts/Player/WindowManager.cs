using UnityEngine;

public class WindowManager : MonoBehaviour
{
    [SerializeField] private GameObject windowPrefab; // Префаб окна
    [SerializeField] private Transform monitorCanvas; // Canvas монитора

    public void OpenNewWindow()
    {
        Instantiate(windowPrefab, monitorCanvas);
    }
}