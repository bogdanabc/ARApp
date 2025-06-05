using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System.IO.Compression;
public class MenuControls : MonoBehaviour
{
    public string outputPath = null;
    public void StartPressed()
    {
        SceneManager.LoadScene("Main");
    }
    public void ExitPressed()
    {
        Application.Quit();
        Debug.Log("Exit pressed!");
    }
    public void SettingPressed()
    {
        PickFile();
    }
    public void PickFile()
    {
        NativeFilePicker.PickFile((path) =>
        {
            if (path != null)
            {
                Debug.Log("Файл выбран: " + path);
                outputPath = Path.Combine(Application.persistentDataPath, "ExtractedFiles");
                UnzipFile(path, outputPath);
            }
            else
            {
                Debug.Log("Файл не выбран.");
            }
        }, null);

    }
    private void UnzipFile(string path, string outputPath)
    {
        GameObject startButton = GameObject.Find("StartButton");
        if (Directory.Exists(outputPath))
        {
            Directory.Delete(outputPath, true);
        }
        Debug.Log("Распаковка начата");
        Directory.CreateDirectory(outputPath);
        ZipFile.ExtractToDirectory(path, outputPath);
        startButton.GetComponent<Button>().interactable = true;
        Debug.Log($"Архив успешно распакован в: {outputPath}");
    }

    void OnApplicationQuit()
    {
        string path = Application.persistentDataPath;
        if (Directory.Exists(path))
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(path);
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }

                Debug.Log("PersistentDataPath очищен.");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Ошибка при очистке папки: " + e.Message);
            }
        }
    }
}
